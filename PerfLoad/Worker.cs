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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker starting running at: {time}", DateTimeOffset.Now);

            int nThreads = 12;
            Thread[] threads = new Thread[nThreads];
            PerfThreadInfo[] perfThreadInfo = new PerfThreadInfo[nThreads];

            for (int i = 0; i < nThreads; i++)
            {
                threads[i] = new Thread(SendMessages);

                //todo get the connection strings from AKS ConfigMap
                //https://learn.microsoft.com/en-us/azure/azure-app-configuration/reference-kubernetes-provider?tabs=default#use-connection-string
                perfThreadInfo[i] = new PerfThreadInfo() 
                { 
                    Id = i + 1,
                    MinimumDuration = 10,
                    NumberMessages = 100, 
                    Size = MsgSize.KB1, 
                    ASB_ConnectionString = "Endpoint=sb://briask-msc-sb-run1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8BjpFxcAczu/Pw6dE4YdXacgv8Ixs621+ASbA92Xzw=",
                    QueueName = "queue1"
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

        static void SendMessages(object threadInfo)
        {
            PerfThreadInfo perfThreadInfo = (PerfThreadInfo)threadInfo;

            int index = (int)perfThreadInfo.Id;
            int minDuration = perfThreadInfo.MinimumDuration;

            ILogger myLogger = perfThreadInfo.logger;

            myLogger.LogInformation("Thread started");

            int numOfMessages = perfThreadInfo.NumberMessages;
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
            sw.Start();
            try
            {
                do
                {
                    for (int i = 0; i < numOfMessages / numConcurrentCalls; i++)
                    {
                        if ((minDuration > 0) && (sw.ElapsedMilliseconds > (minDuration * 1000)))
                        {
                            myLogger.LogInformation($"Thread {index} : Minimum Duration reached. Exiting!");
                            keepRunning = false;
                            break;
                        }

                        for (int j = 0; j < numConcurrentCalls; j++)
                        {
                            int id = i * numConcurrentCalls + j;
                            semaphore.WaitAsync();
                            tasks.Add(SendMessage(queueSender, myLogger, msg, id)
                                .ContinueWith((t) => semaphore.Release()));
                        }
                        Task.WhenAll(tasks);
                    }
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

            now = DateTime.Now.ToString();

            myLogger.LogInformation($"FINISHED!! Sending messages to the queue: {queueOrTopicName} : Thread {index} :Time {now} : Rate :{ratePerSecond}/s Seconds:{seconds} Total: {numOfMessages}::");
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
            }
            catch (Exception ex)
            {
                theLogger.LogError($"Exception2: {ex.Message}");
            }
            return Task.CompletedTask;
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