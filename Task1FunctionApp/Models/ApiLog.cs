using Azure;
using Azure.Data.Tables;
using System;

namespace Task1FunctionApp.Models
{
    public record ApiLog : ITableEntity
    {
        public string RowKey { get; set; } = default!;
        public string PartitionKey { get; set; } = default!;
        public string Api { get; init; } = default!;
        public string Link { get; set; } = default!;
        public string Description { get; init; }
        public bool Status { get; init; }
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
    }
}
