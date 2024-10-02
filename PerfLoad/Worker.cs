using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private TelemetryClient tc;
        private static HttpClient httpClient = new HttpClient();

        public Worker(ILogger<Worker> logger, TelemetryClient tc)
        {
            _logger = logger;
            this.tc = tc;

            httpClient.BaseAddress = new Uri("https://resultsservice.azurewebsites.net/");
            //httpClient.BaseAddress = new Uri("https://localhost:7000/");
        
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker starting running at: {time}", DateTimeOffset.Now);

            int nThreads = 5;
            int totalMessages = 10000;
            int messagesPerThread = totalMessages / nThreads;

            Thread[] threads = new Thread[nThreads];
            PerfThreadInfo[] perfThreadInfo = new PerfThreadInfo[nThreads];

            var runDeets = await GetRunDetails();

            for (int i = 0; i < nThreads; i++)
            {
                threads[i] = new Thread(SendMessages);

                //threads[i] = await Task.Run(() => SendMessages(perfThreadInfo[i]));

                //todo get the connection strings from AKS ConfigMap
                //https://learn.microsoft.com/en-us/azure/azure-app-configuration/reference-kubernetes-provider?tabs=default#use-connection-string
                //Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=
                perfThreadInfo[i] = new PerfThreadInfo()
                {
                    RunId = runDeets.Id,
                    Id = i + 1,
                    MinimumDuration = 1,
                    NumberMessages = messagesPerThread,
                    Size = MsgSize.KB1,
                    ASB_ConnectionString = "Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=",
                    //QueueName = "queue1",
                    QueueName = "test",
                    NumberConcurrentCalls = 10
                };
                perfThreadInfo[i].logger = _logger;

                //threads[i] = new Thread(async () => await SendMessages(perfThreadInfo[i]));
            }

            for (int i = 0; i < nThreads; i++)
            {
                threads[i].Start(perfThreadInfo[i]);
            }

            // By default only Warning of above is captured.
            // However the following Info level will be captured by ApplicationInsights,
            // as appsettings.json configured Information level for the category 'WorkerServiceSampleWithApplicationInsights.Worker'
            _logger.LogInformation("Worker Threads running at: {time}", DateTimeOffset.Now);
        }

        private async Task<Run> GetRunDetails()
        {
            //var response = await httpClient.GetAsync("api/Results/newrun");
            var response = await httpClient.GetAsync("api/Results/currentrun");

            var result = response.Content.ReadAsStringAsync().Result;
            var runDetails = JsonSerializer.Deserialize<Run>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return runDetails;
        }

        private async void SendMessages(object threadInfo)
        {
            PerfThreadInfo perfThreadInfo = (PerfThreadInfo)threadInfo;

            int index = (int)perfThreadInfo.Id;
            int minDuration = perfThreadInfo.MinimumDuration;

            ILogger myLogger = perfThreadInfo.logger;

            myLogger.LogInformation("Thread started");

            int numOfMessages = perfThreadInfo.NumberMessages;
            int actualNumOfMessages = 0;
            int numConcurrentCalls = perfThreadInfo.NumberConcurrentCalls;
            var queueOrTopicName = perfThreadInfo.QueueName;

            string msg = GenerateMessage(perfThreadInfo.Size);

            ServiceBusClient client = new ServiceBusClient(perfThreadInfo.ASB_ConnectionString);

            var queueSender = client.CreateSender(queueOrTopicName);

            Stopwatch sw = new Stopwatch();
           
            string now = DateTime.Now.ToString();
            myLogger.LogInformation($"STARTED!! Sending messages to the queue: {queueOrTopicName} : Thread {index} :Time {now}");

            var semaphore = new SemaphoreSlim(10);

            var tasks = new List<Task>();
            // synchronous call to threads.
            //https://learn.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent?view=net-8.0

            bool keepRunning = true;
            int batchNo = 0;
            int numMess = 0;
            sw.Start();
            try
            {
                do
                {
                    batchNo++;
                    myLogger.LogInformation($"Thread {index} : Batch {batchNo} ");

                    //this is how long I want it to run for min duration
                    if ((minDuration > 0) && (sw.ElapsedMilliseconds > (minDuration * 1000)))
                    {
                        myLogger.LogInformation($"Thread {index} : Minimum Duration reached. Exiting!");
                        keepRunning = false;
                        break;
                    }
                    else
                    {
                        myLogger.LogInformation($"KEEP ON RUNNING!!!");
                    }

                    for (int i = 0; i < numOfMessages / numConcurrentCalls; i++)
                    {
                        for (int j = 0; j < numConcurrentCalls; j++)
                        {
                            //myLogger.LogInformation($"task {numMess}");
                            int id = i * numConcurrentCalls + j;
                            await semaphore.WaitAsync();

                            tasks.Add(Task.Run(async () =>
                            {
                                await SendMessage(queueSender, myLogger, msg, id);
                                semaphore.Release();
                                Interlocked.Increment(ref numMess);
                            }));
                        }

                        var t = Task.WhenAll(tasks);
                        try
                        {
                            await t;
                        }
                        catch 
                        {
                        }

                        if (t.IsCompleted && t.Status == TaskStatus.RanToCompletion)
                        {
                            actualNumOfMessages++;
                            myLogger.LogInformation($"task completed Batch {batchNo}  Thread {index} :: i {i} :: numMess {numMess} ");
                        }
                        else
                        {
                            myLogger.LogError($"task failed Batch {batchNo}  Thread {index}");
                        }
                    }

                    myLogger.LogInformation($"Thread {index} : BatchRun {keepRunning}");
                } 
                while (keepRunning);
            }
            catch (ServiceBusException ex)
            {
                myLogger.LogInformation($"ServiceBusException1: {ex.Message}");
            }
            catch (Exception ex)
            {
                myLogger.LogError($"Exception1: {ex.Message}");
            }
            finally
            {
                queueSender.DisposeAsync();
                client.DisposeAsync();
            }

            sw.Stop();
            decimal seconds = sw.ElapsedMilliseconds / 1000;
            decimal ratePerSecond = numOfMessages / seconds;

            decimal actualRatePerSecond = numMess / seconds;

            now = DateTime.Now.ToString();

            perfThreadInfo.FinishTime = DateTime.Now;
            perfThreadInfo.ActualRate = ratePerSecond;
            perfThreadInfo.Elapsed = seconds;
            perfThreadInfo.TopicName = queueOrTopicName;

            var resultJson = JsonSerializer.Serialize(perfThreadInfo);

            var response = httpClient.PostAsJsonAsync("api/Results", resultJson);

            var result = response.Result.Content.ReadAsStringAsync().Result;
            var runDetails = JsonSerializer.Deserialize<Run>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            myLogger.LogInformation($"FINISHED!! Sending messages to the queue: {queueOrTopicName} : {sw.ElapsedMilliseconds} Thread {index} :Time {now} : Rate :{ratePerSecond}/s Seconds:{seconds} Total: {numOfMessages}:: {numMess} @@ {actualRatePerSecond}");

            return;
        }

        private static Task SendMessage(ServiceBusSender queueSender, ILogger theLogger, string msg, int id)
        {
            try
            {
                return queueSender.SendMessageAsync(
                                                new ServiceBusMessage(id + msg)
                                                {
                                                    MessageId = "Perf" + id,
                                                });
            }
            catch(ServiceBusException ex)
            {
                theLogger.LogInformation($"ServiceBusException2: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                theLogger.LogError($"Exception2: {ex.Message}");
                throw;
            }
        }

        private static string GenerateMessage(MsgSize size)
        {
            const string msgText = ":: This is a single message that we sent :: ";

            string characters = new('X', ((int)size - msgText.Length - 7));

            string stringToReturn = msgText + characters;

            return stringToReturn;
        }
    }
}