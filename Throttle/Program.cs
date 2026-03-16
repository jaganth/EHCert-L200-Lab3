using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Throttle
{
    class Program
    {
        // Replace with your Event Hub fully qualified namespace (e.g., "mynamespace.servicebus.windows.net")
        static string fullyQualifiedNamespace = "[REPLACE-WITH-NAMESPACE].servicebus.windows.net";
        static string eventhubName = "EHLab3Hub";

        static int degreeOfParallelism = 30;

        static void Main(string[] args)
        {
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

            await using var producer = new EventHubProducerClient(fullyQualifiedNamespace, eventhubName, credential);
            await using var consumer = new EventHubConsumerClient(
                EventHubConsumerClient.DefaultConsumerGroupName, fullyQualifiedNamespace, eventhubName, credential);

            await Task.WhenAll(
                SendMessagesAsync(producer),
                ReceiveMessagesAsync(consumer));
        }

        static async Task SendMessagesAsync(EventHubProducerClient producer)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s} Sending data...");

                    using EventDataBatch batch = await producer.CreateBatchAsync();
                    for (int i = 0; i < 100; i++)
                    {
                        var eventData = new EventData(Encoding.UTF8.GetBytes("Message1"));
                        eventData.Properties["CorrelationId"] = "32119834-65f3-48c1-b366-619df2e4c400";
                        batch.TryAdd(eventData);
                    }

                    await producer.SendAsync(batch);
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

        static async Task ReceiveMessagesAsync(EventHubConsumerClient consumer)
        {
            await Task.WhenAll(
                ReceiveFromPartitionAsync(consumer, "0"),
                ReceiveFromPartitionAsync(consumer, "1"));
        }

        static async Task ReceiveFromPartitionAsync(EventHubConsumerClient consumer, string partitionId)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s} Receiving data...");
                    await foreach (var partitionEvent in consumer.ReadEventsFromPartitionAsync(partitionId, EventPosition.Latest))
                    {
                        // Event received - partitionEvent.Data contains the EventData payload
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}