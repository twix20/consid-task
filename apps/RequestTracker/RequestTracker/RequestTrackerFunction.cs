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
        const string EntryDateFormat = "yyyy-MM-dd-HH:mm";

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
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var now = DateTimeOffset.UtcNow;

            var result = new RequestResultTableEntity()
            {
                IsSuccess = true,
                PartitionKey = "aa",
                RowKey = $"api-response-{now.ToString(EntryDateFormat)}.txt",
                RequestTriggerAt = now,
            };

            try
            {
                var response = await _httpClient.GetAsync("https://stackoverflow.com");
                result.ResponsceRecievedAt = DateTimeOffset.UtcNow;

                // Read the response content
                var content = await response.Content.ReadAsStringAsync();

                // Step 2: Ensure the blob container exists
                await blobContainerClient.CreateIfNotExistsAsync();

                // Step 3: Get a reference to the blob and upload the content
                var blobClient = blobContainerClient.GetBlobClient(result.RowKey);

                // Upload the content as a stream or as bytes
                await using var stream = await response.Content.ReadAsStreamAsync();
                await blobClient.UploadAsync(stream, true);

                _logger.LogInformation($"HTTP response data saved to Blob: {result.RowKey}");

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                result.IsSuccess = false;
                result.FailureReason = ex.Message;
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
            _logger.LogInformation($"C# GET trigger function executed at: {DateTime.Now}");

            // Set default values for from and to if they are null
            var fromDate = from ?? DateTime.MinValue;
            var toDate = to ?? DateTime.MaxValue;

            // Build the OData filter for querying the entities within the given time range
            var filter = $"RequestTriggerAt ge datetime'{fromDate:O}' and RequestTriggerAt le datetime'{toDate:O}'";

            // Query the table storage with the filter
            var queryResults = tableClient.QueryAsync<RequestResultTableEntity>(filter);

            var results = new List<RequestResultTableEntity>();
            await foreach (var entity in queryResults)
            {
                results.Add(entity);
            }

            return new JsonResult(results);
        }

        [Function("GetLogPayload")]
        public async Task<IActionResult> RunGetLogPayload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            string rowKey,
            [BlobInput(RequestResultTableEntity.PayloadContainerName)] BlobContainerClient blobContainerClient
        )
        {
            _logger.LogInformation($"C# GET trigger function executed at: {DateTime.Now}");

            var blobClient = blobContainerClient.GetBlobClient(rowKey);

            if (!await blobClient.ExistsAsync())
            {
                return new NotFoundResult(); // Return 404 if the entry does not exist
            }

            var blobDownloadInfo = await blobClient.DownloadStreamingAsync();

            // Return the file as a response
            return new FileStreamResult(blobDownloadInfo.Value.Content, "text/plain")
            {
                FileDownloadName = rowKey
            };
        }
    }
}
