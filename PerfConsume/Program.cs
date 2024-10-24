using Azure.Core;
using Azure.Messaging.ServiceBus;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

namespace PerfConsume
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // the client that owns the connection and can be used to create senders and receivers
            ServiceBusClient client;

            // the processor that reads and processes messages from the queue
            ServiceBusProcessor processor;
            ServiceBusSessionProcessor sessionProcessor;

            bool enableSessions = true;

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            client = new ServiceBusClient("Endpoint=sb://brwsstandardsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3eyJ9mnOJdH1Nd+zTAZAbpZoKqF+UrHXj+ASbDcjGjY=", clientOptions);
            //client = new ServiceBusClient("Endpoint=sb://briask-msc-sb-run1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Z8BjpFxcAczu/Pw6dE4YdXacgv8Ixs621+ASbA92Xzw=");

            // create a processor that we can use to process the messages            
            //processor = client.CreateProcessor("brwstestqueue1", new ServiceBusProcessorOptions() {  } );
            //processor = client.CreateProcessor("queue1", new ServiceBusProcessorOptions() { });
            //q-duplicatedetecton

            if (enableSessions)
            {
                sessionProcessor = client.CreateSessionProcessor("q-sessions-on", new ServiceBusSessionProcessorOptions() {  });

                try
                {
                    // add handler to process messages
                    sessionProcessor.ProcessMessageAsync += MessageHandler;

                    // add handler to process any errors
                    sessionProcessor.ProcessErrorAsync += ErrorHandler;

                    // start processing 
                    await sessionProcessor.StartProcessingAsync();

                    while (true)
                    {
                        Thread.Sleep(10);
                    }

                    Console.WriteLine("Wait for a minute and then press any key to end the processing");

                    // stop processing 
                    Console.WriteLine("\nStopping the receiver...");
                    await sessionProcessor.StopProcessingAsync();
                    Console.WriteLine("Stopped receiving messages");
                }
                finally
                {
                    // Calling DisposeAsync on client types is required to ensure that network
                    // resources and other unmanaged objects are properly cleaned up.
                    await sessionProcessor.DisposeAsync();
                    await client.DisposeAsync();
                }
            }
            else
            {
                processor = client.CreateProcessor("q-default", new ServiceBusProcessorOptions() { });

                try
                {
                    // add handler to process messages
                    processor.ProcessMessageAsync += MessageHandler;

                    // add handler to process any errors
                    processor.ProcessErrorAsync += ErrorHandler;

                    // start processing 
                    await processor.StartProcessingAsync();

                    while (true)
                    {
                        Thread.Sleep(10);
                    }

                    Console.WriteLine("Wait for a minute and then press any key to end the processing");

                    // stop processing 
                    Console.WriteLine("\nStopping the receiver...");
                    await processor.StopProcessingAsync();
                    Console.WriteLine("Stopped receiving messages");
                }
                finally
                {
                    // Calling DisposeAsync on client types is required to ensure that network
                    // resources and other unmanaged objects are properly cleaned up.
                    await processor.DisposeAsync();
                    await client.DisposeAsync();
                }
            }
        }

        // handle received messages

        async static Task MessageHandler(ProcessSessionMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            string subject = args.Message.Subject;
            string sessiondId = args.Message.SessionId;
            string id = args.Message.MessageId;
            var corrrelationId = args.Message.CorrelationId;

            Console.WriteLine($"Received Session Message: {id} {corrrelationId} {sessiondId} {subject} {body}");
            if (subject.Contains("Last"))
            {
                await args.CompleteMessageAsync(args.Message);
                args.ReleaseSession();
            }
            else
            {
                // complete the message. message is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message);
            }
        }

        async static Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            string subject = args.Message.Subject;
            string sessiodId = args.Message.SessionId;

            Console.WriteLine($"Received: {sessiodId} {subject} {body}");

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
