using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Throttle
{
    class Program
    {
        static string connectionString = "[REPLACE-WITH-CONNECTION-STRING]";
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

            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(connectionString)
            {
                EntityPath = eventhubName
            };

            var settings = new MessagingFactorySettings();
            settings.TransportType = Microsoft.ServiceBus.Messaging.TransportType.Amqp;
            settings.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey);
            var factory = MessagingFactory.Create(connectionStringBuilder.Endpoints.FirstOrDefault<Uri>().ToString(), settings);
            var ehClient = factory.CreateEventHubClient(eventhubName);

            var sasTP = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey);
            var token = SharedAccessSignatureTokenProvider.GetSharedAccessSignature(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey,
                connectionStringBuilder.Endpoints.FirstOrDefault<Uri>().ToString(), TimeSpan.FromHours(12));

            await Task.WhenAll(
                SendMessagesRestAync($"https://{connectionStringBuilder.Endpoints.FirstOrDefault<Uri>().Host}/{connectionStringBuilder.EntityPath}", token)
                , ReceiveMessagesAsync(ehClient));
        }

        static async Task SendMessagesRestAync(string endpoint, string token)
        {
            var httpClient = new HttpClient();
            var authHeaderValue = AuthenticationHeaderValue.Parse(token);
            while (true)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s} Sending data...");
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/messages?timeout=60&api-version=2014-01");
                    requestMessage.Headers.Authorization = authHeaderValue;
                    
                    //Batch of 100 messsages
                    requestMessage.Content = new StringContent("[\r\n\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t}, \r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t},\r\n\t\t{\r\n\t\t\"Body\": \"Message1\",\r\n\t\t\"BrokerProperties\": {\r\n\t\t\t\"CorrelationId\",\r\n\t\t\t\"32119834-65f3-48c1-b366-619df2e4c400\"\r\n\t\t}\r\n\t}\r\n]", 
                        Encoding.UTF8, "application/vnd.microsoft.servicebus.json");

                    var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException httpRequestException)
                {
                    ConsoleColor currentForeground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(httpRequestException.Message);
                    Console.ForegroundColor = currentForeground;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        static async Task ReceiveMessagesAsync(EventHubClient client, string consumerGroup = "$default")
        {
            var cg = client.GetConsumerGroup(consumerGroup);
            var receiver0 = cg.CreateReceiver("0");
            var receiver1 = cg.CreateReceiver("1");

            while (true)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now:s} Receiving data...");
                    await Task.WhenAll(receiver0.ReceiveAsync(), receiver1.ReceiveAsync());
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}