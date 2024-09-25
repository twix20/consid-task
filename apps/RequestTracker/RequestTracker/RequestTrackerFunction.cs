using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RequestTracker.Entities;

namespace RequestTracker
{
    public class RequestTrackerFunction
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public RequestTrackerFunction(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = loggerFactory.CreateLogger<RequestTrackerFunction>();
            _httpClient = httpClientFactory.CreateClient();
        }

        [Function("RequestTrackerFunction")]
        [TableOutput(RequestResultTableEntity.TableName, Connection = "AzureWebJobsStorage")]
        public async Task<RequestResultTableEntity> RunRequestTrackerFunction(
            [TimerTrigger("0 * * * * *")] TimerInfo myTimer,
            [BlobInput(RequestResultTableEntity.PayloadContainerName)] BlobContainerClient blobContainerClient
        )
        {
            var now = DateTimeOffset.UtcNow;
            var result = new RequestResultTableEntity()
            {
                IsSuccess = true,
                PartitionKey = "api-response",
                RowKey = $"api-response-{now.ToString("yyyy-MM-dd-HH:mm")}.txt",
                RequestTriggerAt = now
            };

            try
            {
                _logger.LogInformation("Making an HTTP GET request");

                var response = await _httpClient.GetAsync("https://google.com");
                result.ResponseRecievedAt = DateTimeOffset.UtcNow;

                await UploadResponseContentToBlob(blobContainerClient, response, result.RowKey);

                response.EnsureSuccessStatusCode();
                _logger.LogInformation("HTTP request was successful.");
            }
            catch (HttpRequestException ex)
            {
                result.IsSuccess = false;
                result.FailureReason = ex.Message;

                _logger.LogError($"HTTP request failed: {ex.Message}");
            }

            return result;
        }

        [Function("GetLogs")]
        public async Task<IActionResult> RunGetLogs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            DateTime? from, DateTime? to,
            [TableInput(RequestResultTableEntity.TableName, Connection = "AzureWebJobsStorage")] TableClient tableClient
        )
        {
            var fromDate = from ?? DateTime.MinValue;
            var toDate = to ?? DateTime.MaxValue;

            var filter = $"RequestTriggerAt ge datetime'{fromDate:O}' and RequestTriggerAt le datetime'{toDate:O}'";
            var queryResults = tableClient.QueryAsync<RequestResultTableEntity>(filter);

            var results = new List<RequestResultTableEntity>();
            await foreach (var entity in queryResults)
            {
                results.Add(entity);
            }

            _logger.LogInformation($"Retrieved {results.Count} log entries from {fromDate} to {toDate}.");

            return new JsonResult(results);
        }

        [Function("GetLogPayload")]
        public async Task<IActionResult> RunGetLogPayload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            string rowKey,
            [BlobInput(RequestResultTableEntity.PayloadContainerName)] BlobContainerClient blobContainerClient
        )
        {
            var blobClient = blobContainerClient.GetBlobClient(rowKey);
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning($"Blob with RowKey {rowKey} not found.");
                return new NotFoundResult();
            }

            var blobDownloadInfo = await blobClient.DownloadStreamingAsync();
            return new FileStreamResult(blobDownloadInfo.Value.Content, "text/plain")
            {
                FileDownloadName = rowKey
            };
        }

        private async Task UploadResponseContentToBlob(BlobContainerClient blobContainerClient, HttpResponseMessage response, string rowKey)
        {
            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(rowKey);

            // Read the response content as a stream and upload it to Blob storage
            await using var stream = await response.Content.ReadAsStreamAsync();
            await blobClient.UploadAsync(stream);

            _logger.LogInformation($"HTTP response data saved to Blob: {rowKey}");
        }
    }
}
