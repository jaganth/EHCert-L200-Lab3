using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Throttle
{
    class Program
    {
        static string eventHubsNamespace = "yournamespace.servicebus.windows.net";
        static string eventhubName = "EHLab3Hub";

        static int degreeOfParallelism = 30;

        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 2000;
            ThreadPool.SetMaxThreads(1024, 1024);

            var tasks = new List<Task>();
            for (int i = 0; i < degreeOfParallelism; i++)
            {
                tasks.Add(Task.Run(EventHubSendReceiveLoopAsync));
            }

            Task.WaitAll(tasks.ToArray());
        }

        static async Task EventHubSendReceiveLoopAsync()
        {
            var credential = new DefaultAzureCredential();
            var producerClient = new EventHubProducerClient(eventHubsNamespace, eventhubName, credential);
            var consumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, eventHubsNamespace, eventhubName, credential);

            await Task.WhenAll(
                SendMessagesAsync(producerClient),
                ReceiveMessagesAsync(consumerClient));
        }

        static async Task SendMessagesAsync(EventHubProducerClient producerClient)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s} Sending data...");

                    using (EventDataBatch batch = await producerClient.CreateBatchAsync())
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            var eventData = new EventData(Encoding.UTF8.GetBytes("Message1"));
                            eventData.Properties["CorrelationId"] = "32119834-65f3-48c1-b366-619df2e4c400";

                            if (!batch.TryAdd(eventData))
                            {
                                throw new InvalidOperationException("Unable to add an event to the EventDataBatch.");
                            }
                        }

                        await producerClient.SendAsync(batch);
                    }
                }
                catch (Exception exception)
                {
                    ConsoleColor currentForeground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception.Message);
                    Console.ForegroundColor = currentForeground;
                }
            }
        }

        static async Task ReceiveMessagesAsync(EventHubConsumerClient consumerClient)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s} Receiving data...");
                    await Task.WhenAll(
                        ReceiveSingleMessageFromPartitionAsync(consumerClient, "0"),
                        ReceiveSingleMessageFromPartitionAsync(consumerClient, "1"));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        static async Task ReceiveSingleMessageFromPartitionAsync(EventHubConsumerClient consumerClient, string partitionId)
        {
            var enumerator = consumerClient.ReadEventsFromPartitionAsync(partitionId, EventPosition.Latest).GetAsyncEnumerator();

            try
            {
                await enumerator.MoveNextAsync();
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }
    }
}
