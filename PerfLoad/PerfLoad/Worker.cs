using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
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
            //while (!stoppingToken.IsCancellationRequested)
            //{
            Thread thread1 = new Thread(SendMessages);
            Thread thread2 = new Thread(SendMessages);
            Thread thread3 = new Thread(SendMessages);
            Thread thread4 = new Thread(SendMessages);

            thread1.Start(1);
            thread2.Start(2);
            thread3.Start(3);
            thread4.Start(4);

            Thread thread11 = new Thread(SendMessages);
            Thread thread21 = new Thread(SendMessages);
            Thread thread31 = new Thread(SendMessages);
            Thread thread41 = new Thread(SendMessages);

            thread11.Start(11);
            thread21.Start(21);
            thread31.Start(31);
            thread41.Start(41);

            // By default only Warning of above is captured.
            // However the following Info level will be captured by ApplicationInsights,
            // as appsettings.json configured Information level for the category 'WorkerServiceSampleWithApplicationInsights.Worker'
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                //using (tc.StartOperation<RequestTelemetry>("workeroperation"))
                //{
                //    var res = httpClient.GetAsync("https://bing.com").Result.StatusCode;
                //    _logger.LogInformation("bing http call completed with status:" + res);
                //}

                // number of messages to be sent to the queue
                const int numOfMessages = 1000;

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

        static void SendMessages(Object stateInfo)
        {
            int index = (int)stateInfo;

            int numOfMessages = 1000;
            var connectionString = "Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=";

            ServiceBusClient client = new ServiceBusClient(connectionString);

            var queueOrTopicName = "brwstestqueue1";

            var queueSender = client.CreateSender(queueOrTopicName);
            Console.WriteLine($"Sending messages to the queue: {queueOrTopicName} : Thread {index}");

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

            Console.WriteLine($"FINISHED!! Sending messages to the queue: {queueOrTopicName} : Thread {index}");
        }
    }
}