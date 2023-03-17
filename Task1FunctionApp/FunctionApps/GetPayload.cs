using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using System.Net;
using Azure.Storage.Blobs;

namespace Task1FunctionApp.FunctionApps
{
    public static class GetPayload
    {
        [FunctionName("GetPayload")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]

        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string responseMessage = string.Empty;
            try
            {
                string blobName = req.Query["name"];
                string blobConnectionString = Environment.GetEnvironmentVariable("BlobConnectionString");
                string containerName = Environment.GetEnvironmentVariable("BlobContainerName");
                BlobContainerClient blobContainerClient = new(blobConnectionString, containerName);
                BlobClient blob = blobContainerClient.GetBlobClient(blobName);
                var response = await blob.DownloadAsync();
                using StreamReader sr = new(response.Value.Content);
                responseMessage = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                log.LogInformation($"Internal server error : {ex}");
                throw;
            }
            return new OkObjectResult(responseMessage);
        }
    }
}
