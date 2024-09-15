using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    // Application Insights

                    // Add custom TelemetryInitializer
                    //services.AddSingleton<ITelemetryInitializer, MyCustomTelemetryInitializer>();

                    //// Add custom TelemetryProcessor
                    //services.AddApplicationInsightsTelemetryProcessor<MyCustomTelemetryProcessor>();

                    //// Example on Configuring TelemetryModules.
                    //// [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Not a real api key, this is example code.")]
                    //services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((mod, opt) => mod.AuthenticationApiKey = "InstrumentationKey=f6307a4b-4e1a-4448-864e-e9d96cafb529;IngestionEndpoint=https://northeurope-2.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/;ApplicationId=f7f334f3-77ba-4f0e-930a-ebc883a9afaf");

                    // instrumentation key is read automatically from appsettings.json
                    services.AddApplicationInsightsTelemetryWorkerService();
                });

        internal class MyCustomTelemetryInitializer : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                // Replace with actual properties.
                (telemetry as ISupportProperties).Properties["MyCustomKey"] = "MyCustomValue";
            }
        }

        internal class MyCustomTelemetryProcessor : ITelemetryProcessor
        {
            ITelemetryProcessor next;

            public MyCustomTelemetryProcessor(ITelemetryProcessor next)
            {
                this.next = next;
            }

            public void Process(ITelemetry item)
            {
                // Example processor - not filtering out anything.
                // This should be replaced with actual logic.
                this.next.Process(item);
            }
        }
    }
}

//using Azure.Messaging.ServiceBus;

//namespace PerfLoad
//{
//    internal class Program
//    {
//        static async Task Main(string[] args)
//        {
//            Console.WriteLine("Hello, World!");

//            await test();

//        }

//        private static async Task test()
//        {

//            // the client that owns the connection and can be used to create senders and receivers
////            ServiceBusClient client;

//            // the sender used to publish messages to the queue
//            ServiceBusSender sender;

//            // number of messages to be sent to the queue
//            const int numOfMessages = 10;

//            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
//            // of the application, which is best practice when messages are being published or read
//            // regularly.
//            //
//            // set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
//            // If you use the default AmqpTcp, you will need to make sure that the ports 5671 and 5672 are open

//            // TODO: Replace the <NAMESPACE-CONNECTION-STRING> and <QUEUE-NAME> placeholders

//            try
//            {
//                var connectionString = "Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=";

//                ServiceBusClient client = new ServiceBusClient(connectionString);

//                var queueOrTopicName = "brwstestqueue1";

//                var queueSender = client.CreateSender(queueOrTopicName);
//                Console.WriteLine($"Sending messages to the queue: {queueOrTopicName}");

//                for (int i = 0; i < numOfMessages; i++)
//                {
//                    await queueSender.SendMessageAsync(new ServiceBusMessage($"This is a single message that we sent {i}"));//.GetAwaiter().GetResult();
//                }
                
//                Console.WriteLine($"Sent {numOfMessages} messages to the queue: {queueOrTopicName}");
//                Console.WriteLine("Press any key to end the application");
//                Console.ReadKey();

//            //    client = new ServiceBusClient("Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=", clientOptions);
//                sender = client.CreateSender("brwstestqueue1");

                

//                //Endpoint=sb://brwstestnamespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hCtK3tapXto2J3S2ix5FGsyxR0/UmbZ5q+ASbPFRfVk=

//                // create a batch 
//                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

//                for (int i = 1; i <= numOfMessages; i++)
//                {
//                    // try adding a message to the batch
//                    if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
//                    {
//                        // if it is too large for the batch
//                        throw new Exception($"The message {i} is too large to fit in the batch.");
//                    }
//                }

//                try
//                {
//                    // Use the producer client to send the batch of messages to the Service Bus queue
//                    await sender.SendMessagesAsync(messageBatch);
//                    Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
//                }
//                finally
//                {
//                    // Calling DisposeAsync on client types is required to ensure that network
//                    // resources and other unmanaged objects are properly cleaned up.
//                    await sender.DisposeAsync();
//                    await client.DisposeAsync();
//                }


//            }
//            catch (Exception e)
//            {
//                int i = 0;
//            }

//            Console.WriteLine("Press any key to end the application");
//            Console.ReadKey();
//        }
//    }
//}
