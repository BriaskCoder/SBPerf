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
            //while (!stoppingToken.IsCancellationRequested)
            //{

            // Create an array of threads

            int nThreads = 8;
            Thread[] threads = new Thread[nThreads];
            PerfThreadInfo[] perfThreadInfo = new PerfThreadInfo[nThreads];

            for (int i = 0; i < nThreads; i++)
            {
                threads[i] = new Thread(SendMessages);

                perfThreadInfo[i] = new PerfThreadInfo() { Id = i + 1, NumberMessages = 1000};
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

                //using (tc.StartOperation<RequestTelemetry>("workeroperation"))
                //{
                //    var res = httpClient.GetAsync("https://bing.com").Result.StatusCode;
                //    _logger.LogInformation("bing http call completed with status:" + res);
                //}

                // number of messages to be sent to the queue
                const int numOfMessages = 10;

                // The Service Bus client types are safe to cache and use as a singleton for the lifetime
                // of the application, which is best practice when messages are being published or read
                // regularly.
                //
                // set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
                // If you use the default AmqpTcp, you will need to make sure that the ports 5671 and 5672 are open

                // TODO: Replace the <NAMESPACE-CONNECTION-STRING> and <QUEUE-NAME> placeholders

                //try
                //{
                //    var connectionString = "Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=";

                //    await using (ServiceBusClient client = new ServiceBusClient(connectionString))
                //    {
                //        var queueOrTopicName = "brwstestqueue1";

                //        var queueSender = client.CreateSender(queueOrTopicName);
                //        Console.WriteLine($"Sending messages to the queue: {queueOrTopicName}");

                //        for (int i = 0; i < numOfMessages; i++)
                //        {
                //            await queueSender.SendMessageAsync(new ServiceBusMessage($"This is a single message that we sent {i}"));                            
                //        }
                //    }

                //    //Console.WriteLine($"Sent {numOfMessages} messages to the queue: {queueOrTopicName}");
                //    //Console.WriteLine("Press any key to end the application");
                //    //Console.ReadKey();

                //    await Task.Delay(1000, stoppingToken);
                //}
                //catch (Exception ex)
                //{
                //    tc.TrackException(ex);
                //    tc.Flush();
                //    throw;
                //}
            //}
        }

        static void SendMessages(object threadInfo)
        {
            PerfThreadInfo perfThreadInfo = (PerfThreadInfo)threadInfo;

            int index = (int)perfThreadInfo.Id;

            ILogger myLogger = perfThreadInfo.logger;

            myLogger.LogInformation("Thread started");


            int numOfMessages = perfThreadInfo.NumberMessages;
            var connectionString = "Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=";

            ServiceBusClient client = new ServiceBusClient(connectionString);

            var queueOrTopicName = "brwstestqueue1";

            var queueSender = client.CreateSender(queueOrTopicName);

            Stopwatch sw = new Stopwatch();
           
            string now = DateTime.Now.ToString();
            myLogger.LogInformation($"STARTED!! Sending messages to the queue: {queueOrTopicName} : Thread {index} :Time {now}");

            sw.Start();
            try
            {
                for (int i = 0; i < numOfMessages; i++)
                {
                    queueSender.SendMessageAsync(new ServiceBusMessage($"This is a single message that we sent {i}")).Wait();
                }
            }
            catch (Exception ex)
            {
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

            myLogger.LogInformation($"FINISHED!! Sending messages to the queue: {queueOrTopicName} : Thread {index} :Time {now} : Rate :{ratePerSecond} Seconds:{seconds} Total: {numOfMessages}::");
        }
    }
}