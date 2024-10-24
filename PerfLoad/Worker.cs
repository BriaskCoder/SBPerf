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
using System.Drawing;
using System.Security.Policy;
using Microsoft.Azure.Amqp.Framing;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private TelemetryClient tc;
        private static HttpClient httpClient = new HttpClient();

        private Random r = new Random();
        private List<int> unshuffled = new List<int>();

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

            int nThreads = 1;
            int totalMessages = 10000;
            MsgSize size = MsgSize.KB1;
            int messagesPerThread = totalMessages / nThreads;
            int numberConcurrentCalls = 10;
            bool sessions = true;
            
            unshuffled = Enumerable.Range(0, numberConcurrentCalls).ToList();

            Thread[] threads = new Thread[nThreads];
            PerfThreadInfo[] perfThreadInfo = new PerfThreadInfo[nThreads];

            var runDeets = await GetRunDetails();

            for (int i = 0; i < nThreads; i++)
            {
                threads[i] = new Thread(SendMessages);

                //todo get the connection strings from AKS ConfigMap
                //https://learn.microsoft.com/en-us/azure/azure-app-configuration/reference-kubernetes-provider?tabs=default#use-connection-string
                //Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=
                perfThreadInfo[i] = new PerfThreadInfo()
                {
                    RunId = runDeets.Id,
                    Id = i + 1,
                    MinimumDuration = 1,
                    NumberMessages = messagesPerThread,
                    NumberThreads = nThreads,
                    Size = size,
                    //ASB_ConnectionString = "Endpoint=sb://brwspremiumsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hvY2LIhJIx3j6vvvRVPIIvsJk3XhcZXCs+ASbINLEUE=",
                    ASB_ConnectionString = "Endpoint=sb://brwsstandardsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3eyJ9mnOJdH1Nd+zTAZAbpZoKqF+UrHXj+ASbDcjGjY=",
                    //ASB_ConnectionString = "Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=",
                    //QueueName = "q-default",
                    //QueueName = "q-sessions-on",
                    //QueueName = "q-partitioning-on",
                    //QueueName = "q-duplicatedetection-on",
                    QueueName = "t-default",
                    //QueueName = "t-subs1",
                    //QueueName = "t-subs5",
                    //QueueName = "t-subs50",
                    //QueueName = "t-filter-correlation",
                    NumberConcurrentCalls = numberConcurrentCalls
                };
                perfThreadInfo[i].logger = _logger;
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
            //todo tidy this up
            var response = await httpClient.GetAsync("api/Results/newrun");
            var result = response.Content.ReadAsStringAsync().Result;

            response = await httpClient.GetAsync("api/Results/currentrun");

            result = response.Content.ReadAsStringAsync().Result;
            var runDetails = JsonSerializer.Deserialize<Run>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return runDetails;
        }

        private async void SendMessages(object threadInfo)
        {
            PerfThreadInfo perfThreadInfo = (PerfThreadInfo)threadInfo;

            int threadId = (int)perfThreadInfo.Id;
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
            myLogger.LogInformation($"STARTED!! Sending messages to the queue: {queueOrTopicName} : Thread {threadId} :Time {now}");

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
                    myLogger.LogInformation($"Thread {threadId} : Batch {batchNo} ");

                    //this is how long it should run for minimum duration
                    if ((minDuration > 0) && (sw.ElapsedMilliseconds > (minDuration * 1000)))
                    {
                        myLogger.LogInformation($"Thread {threadId} : Minimum Duration reached. Exiting!");
                        keepRunning = false;
                        break;
                    }
                    else
                    {
                        myLogger.LogInformation($"KEEP ON RUNNING!!!");
                    }

                    for (int i = 0; i < numOfMessages / numConcurrentCalls; i++)
                    {
                        var customerEventOrder = GenerateRandomOrder(numConcurrentCalls);
                        int numberOdd = 0;
                        int numberEven = 0;
                        var sessionIdOdd = Guid.NewGuid().ToString();
                        var sessionIdEven = Guid.NewGuid().ToString();                       

                        for (int j = 0; j < numConcurrentCalls; j++)
                        {
                            bool lastMsg = false;
                            var sessionId = (customerEventOrder[j] % 2) == 0 ? sessionIdEven : sessionIdOdd;

                            bool isEven = int.IsEvenInteger(customerEventOrder[j]);

                            if (isEven)
                            {
                                numberEven++;
                            }
                            else
                            {
                                numberOdd++;
                            }

                            if ((isEven && numberEven == 5) || (!isEven && numberOdd == 5))
                            {    
                                lastMsg = true;
                                const string msgText = ":: This is the last message that we sent :: ";
                                Console.WriteLine($"Thread {threadId} : Batch {batchNo} : i {i} : j {j} : Last Message {msgText}  {isEven}::{numberEven} {numberOdd}");
                            }

                            int id = i * numConcurrentCalls + j;
                            await semaphore.WaitAsync();

                            Console.WriteLine($"Thread {threadId} : Batch {batchNo} : i {i} : j {j} : Sending message {id} : {msg} : {sessionId} : {lastMsg}");
                            tasks.Add(Task.Run(async () =>
                            {
                                await SendMessage(queueSender, myLogger, msg, i, isEven, id, threadId, sessionId, lastMsg);
                                semaphore.Release();
                                Interlocked.Increment(ref numMess);
                            }));
                        }

                        var allTasks = Task.WhenAll(tasks);
                        try
                        {
                            await allTasks;
                        }
                        catch(Exception ex)
                        {
                            AggregateException exception = allTasks.Exception;
                            foreach (var error in exception.InnerExceptions)
                            {
                                Console.WriteLine(error);
                                myLogger.LogError(ex, " : Failed??");
                            }

                            Console.WriteLine(ex);
                        }

                        if (allTasks.IsCompleted && allTasks.Status == TaskStatus.RanToCompletion)
                        {
                            actualNumOfMessages++;
                            myLogger.LogInformation($"task completed Batch {batchNo}  Thread {threadId} :: i {i} :: numMess {numMess} ");
                        }
                        else
                        {
                            bool sleepRequired = false;
                            allTasks.Exception.Handle(ex =>
                            {
                                if (ex is ServiceBusException)
                                {
                                    myLogger.LogError(ex, $"ServiceBusException : {ex.Message}");
                                    sleepRequired = true;
                                }
                                else
                                {
                                    myLogger.LogError(ex, " : Aggregate Failed??");
                                }

                                return true;
                            });

                            if (sleepRequired)
                            {
                                myLogger.LogInformation($"Sleeping for 10 seconds");
                                Thread.Sleep(10000);
                            }

                            myLogger.LogError($"Task failed Batch {batchNo}  Thread {threadId}");
                        }
                    }

                    myLogger.LogInformation($"Thread {threadId} : BatchRun {keepRunning}");
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
            perfThreadInfo.NumCreated = numMess;
            perfThreadInfo.ActualNumberMessages = numMess;

            var resultJson = JsonSerializer.Serialize(perfThreadInfo);

            var response = httpClient.PostAsJsonAsync("api/Results", resultJson);

            var result = response.Result.Content.ReadAsStringAsync().Result;
            var runDetails = JsonSerializer.Deserialize<Run>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            myLogger.LogInformation($"FINISHED!! Sending messages to the queue: {queueOrTopicName} : {sw.ElapsedMilliseconds} Thread {threadId} :Time {now} : Rate :{ratePerSecond}/s Seconds:{seconds} Total: {numOfMessages}:: {numMess} @@ {actualRatePerSecond}");

            return;
        }

        private static Task SendMessage(ServiceBusSender queueSender, ILogger theLogger, string msg, int corrId, bool isEven, int id, int index, string sessionId, bool lastMsg)
        {
            try
            {
                Console.WriteLine($"Thread {index} : Sending message {id} : {msg} : {sessionId} : {lastMsg}");
                return queueSender.SendMessageAsync(
                                                new ServiceBusMessage(id + msg)
                                                {
                                                    MessageId = "Thread:" + index + ":Perf:" + id,
                                                    CorrelationId = "Message Sequence: " + corrId + " :: " + isEven,
                                                    SessionId = sessionId, 
                                                    Subject = lastMsg ? "LastMessage" : "NotLastMessage",
                                                    ApplicationProperties = { { "Priority", isEven ? "High":"" }}
                                                }
                                                );
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
            int headerSize = 0;
            if (size > MsgSize.KB128)
            {
                headerSize = 6000;
            }

            // Also remove 6kb (6000) from the size to account for the message overhead
            const string msgText = ":: This is a single message that we sent :: ";

            string characters = new('X', ((int)size - msgText.Length - 7 - headerSize));

            string stringToReturn = msgText + characters;

            return stringToReturn;
        }

        /// <summary>
        /// Fisher-Yates shuffle c# implementation
        /// modified from https://exceptionnotfound.net/understanding-the-fisher-yates-card-shuffling-algorithm/
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private List<int> GenerateRandomOrder(int size)
        {
            List<int> shuffledNumbers = new List<int>(unshuffled);
            //Step 1: For each unshuffled item in the collection
            for (int n = unshuffled.Count - 1; n > 0; --n)
            {
                //Step 2: Randomly pick an item which has not been shuffled
                int k = r.Next(n + 1);

                //Step 3: Swap the selected item with the last "unstruck" letter in the collection
                int temp = unshuffled[n];
                unshuffled[n] = unshuffled[k];
                unshuffled[k] = temp;
            }

            return shuffledNumbers;
        }
    }
}