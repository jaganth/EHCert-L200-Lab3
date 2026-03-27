using Azure.Identity;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Throttle
{
    class Program
    {
        static string eventHubsNamespace = "<EventHub Namespace>.servicebus.windows.net";
        static string eventhubName = "EHLab3Hub";
        static readonly string[] receivePartitionIds = new[] { "0", "1" };
        static readonly DefaultAzureCredential credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                CredentialProcessTimeout = TimeSpan.FromMinutes(2)
            });

        static int degreeOfParallelism = 30;
        static int receiveWorkerClaimed = 0;

        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 2000;
            ThreadPool.SetMaxThreads(1024, 1024);

            // Spread process startup auth calls to avoid simultaneous Azure CLI token timeouts.
            int startupJitterMs = (Process.GetCurrentProcess().Id % 10) * 1000;
            if (startupJitterMs > 0)
            {
                Console.WriteLine($"{DateTime.Now:s} Applying startup jitter: {startupJitterMs} ms");
                Thread.Sleep(startupJitterMs);
            }

            WarmUpCredentialAsync().GetAwaiter().GetResult();

            var tasks = new List<Task>();
            for (int i = 0; i < degreeOfParallelism; i++)
            {
                tasks.Add(Task.Run(EventHubSendReceiveLoopAsync));
            }

            Task.WaitAll(tasks.ToArray());
        }

        static async Task EventHubSendReceiveLoopAsync()
        {
            var producerClient = new EventHubProducerClient(eventHubsNamespace, eventhubName, credential);
            var consumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, eventHubsNamespace, eventhubName, credential);

            // Keep receive path enabled, but only one worker per process owns receive links.
            if (Interlocked.CompareExchange(ref receiveWorkerClaimed, 1, 0) == 0)
            {
                Console.WriteLine($"{DateTime.Now:s} Receive worker active on partitions {string.Join(", ", receivePartitionIds)}.");
                await Task.WhenAll(
                    SendMessagesAsync(producerClient),
                    ReceiveMessagesAsync(consumerClient));
            }
            else
            {
                await SendMessagesAsync(producerClient);
            }
        }

        static async Task WarmUpCredentialAsync()
        {
            Console.WriteLine($"{DateTime.Now:s} Warming up DefaultAzureCredential token cache...");

            Exception? lastException = null;
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                try
                {
                    await credential.GetTokenAsync(
                        new TokenRequestContext(new[] { "https://eventhubs.azure.net/.default" }),
                        CancellationToken.None);
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Console.WriteLine($"{DateTime.Now:s} Token warm-up attempt {attempt} failed: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                }
            }

            throw lastException ?? new InvalidOperationException("Token warm-up failed.");
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
                    Console.WriteLine($"{DateTime.Now:s} Receiving data from partitions {string.Join(", ", receivePartitionIds)}...");
                    await Task.WhenAll(receivePartitionIds.Select(partitionId => ReceiveSingleMessageFromPartitionAsync(consumerClient, partitionId)));
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
