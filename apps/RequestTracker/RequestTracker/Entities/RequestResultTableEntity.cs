using Azure;
using Azure.Data.Tables;

namespace RequestTracker.Entities;

public class RequestResultTableEntity : ITableEntity
{
    public const string TableName = "RequestResultTableEntityTablexxx";
    public const string PayloadContainerName = "payload-containerxxx";

    public DateTimeOffset RequestTriggerAt { get; set; }
    public DateTimeOffset ResponsceRecievedAt { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
