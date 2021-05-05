using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;
using static System.Console;

namespace QueueApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                string value = string.Join(" ", args);

                await SendArticleAsync(value);

                WriteLine($"Sent: {value}");
            }
            else
            {
                string value = await ReceiveArticleAsync();

                WriteLine($"Received: {value}");
            }
        }

        static async Task SendArticleAsync(string newsMessage)
        {
            CloudQueue queue = GetQueue();

            bool createdQueue = await queue.CreateIfNotExistsAsync();
            if (createdQueue)
            {
                WriteLine("The queue of news articles was created.");
            }

            CloudQueueMessage articleMessage = new CloudQueueMessage(newsMessage);

            await queue.AddMessageAsync(articleMessage);
        }

        static async Task<string> ReceiveArticleAsync()
        {
            CloudQueue queue = GetQueue();

            bool exists = await queue.ExistsAsync();
            if (exists)
            {
                CloudQueueMessage retrievedArticle = await queue.GetMessageAsync();
                if (retrievedArticle != null)
                {
                    string newsMessage = retrievedArticle.AsString;

                    await queue.DeleteMessageAsync(retrievedArticle);

                    return newsMessage;
                }
            }

            return "Queue empty or not created";
        }

        static CloudQueue GetQueue()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            // Get a connection string to our Azure Storage account.
            var connectionString = configuration["ConnectionStrings:StorageAccount"];

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            
            return queueClient.GetQueueReference("newsqueue");
        }
    }
}
