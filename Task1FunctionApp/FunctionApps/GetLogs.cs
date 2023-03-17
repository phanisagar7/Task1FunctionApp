using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using Azure.Data.Tables;
using System.Collections.Generic;
using Task1FunctionApp.Models;

namespace Task1FunctionApp.FunctionApps
{
    public static class GetLogs
    {
        [FunctionName("GetLogs")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            IList<ApiLog> logs = new List<ApiLog>();
            try
            {
                string tableConnectionString = Environment.GetEnvironmentVariable("TableConnectionString");
                DateTime from = DateTime.Parse(req.Query["from"]);
                DateTime to = DateTime.Parse(req.Query["to"]);
                TableServiceClient tableServiceClient = new(tableConnectionString);
                TableClient tableClient = tableServiceClient.GetTableClient(tableName: "apilog");
                var records = tableClient.QueryAsync<ApiLog>(x => x.Timestamp >= from && x.Timestamp <= to, maxPerPage: 10);
                await foreach (var celeb in records)
                {
                    logs.Add(celeb);
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"Internal server error : {ex}");
                throw;
            }
            return new OkObjectResult(logs);
        }
    }
}
