using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using Azure.Storage.Blobs;
using System.Text;
using Task1FunctionApp.Models;

namespace Task1FunctionApp.FunctionApps
{
    public class Scheduler
    {
        [FunctionName("Scheduler")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("PublicRandomApi"));
                    HttpResponseMessage response = await client.GetAsync("random");
                    if (response.IsSuccessStatusCode)
                    {
                        var resp = await response.Content.ReadAsAsync<RandomApiResponse>();

                        var apilog = new ApiLog()
                        {
                            RowKey = resp.Entries?[0].API,
                            PartitionKey = resp.Entries?[0].Category,
                            Api = resp.Entries?[0].API,
                            Link = resp.Entries?[0].Link,
                            Description = resp.Entries?[0].Description,
                            Status = true
                        };

                        string tableConnectionString = Environment.GetEnvironmentVariable("TableConnectionString");
                        TableServiceClient tableServiceClient = new(tableConnectionString);
                        TableClient tableClient = tableServiceClient.GetTableClient(tableName: "apilog");
                        await tableClient.CreateIfNotExistsAsync();
                        await tableClient.AddEntityAsync(apilog);

                        string blobConnectionString = Environment.GetEnvironmentVariable("BlobConnectionString");
                        string containerName = Environment.GetEnvironmentVariable("BlobContainerName");
                        BlobContainerClient blobContainerClient = new BlobContainerClient(blobConnectionString, containerName);
                        BlobClient blob = blobContainerClient.GetBlobClient(resp.Entries?[0].API);

                        using MemoryStream ms = new(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(apilog)));
                        await blob.UploadAsync(ms);
                    }
                    else
                    {
                        log.LogInformation("Internal server Error");
                    }
                }
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                log.LogInformation($"Internal server error : {ex}");
                throw;
            }
        }
    }
}
