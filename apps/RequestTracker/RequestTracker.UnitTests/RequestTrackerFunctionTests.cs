using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace RequestTracker.UnitTests;

public class RequestTrackerFunctionTests
{
    private TimerInfo _timerInfoMock;
    private BlobClient _blobClientMock;
    private BlobContainerClient _blobContainerClientMock;
    private HttpClient _httpClient;
    private RequestTrackerFunction _sut;
    private MockHttpMessageHandler _httpMessageHandlerMock;

    [SetUp]
    public void Setup()
    {
        _httpMessageHandlerMock = Substitute.ForPartsOf<MockHttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock);

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().ReturnsForAnyArgs(_httpClient);

        _timerInfoMock = Substitute.For<TimerInfo>();
        _blobClientMock = Substitute.For<BlobClient>();
        _blobContainerClientMock = Substitute.For<BlobContainerClient>();
        _blobContainerClientMock.GetBlobClient(default).ReturnsForAnyArgs(_blobClientMock);

        _sut = new RequestTrackerFunction(loggerFactory, httpClientFactory);
    }

    [TearDown]
    public void Teardown()
    {
        _httpClient.Dispose();
        _httpMessageHandlerMock.Dispose();
    }

    [Test]
    public async Task RunRequestTrackerFunction_ShouldLogSuccessResponse()
    {
        // Arrange
        var httpResponse = Substitute.For<HttpResponseMessage>();
        httpResponse.StatusCode = HttpStatusCode.OK;
        httpResponse.Content = new StringContent("test");

        _httpMessageHandlerMock.Send(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()).Returns(httpResponse);

        // Act
        var result = await _sut.RunRequestTrackerFunction(_timerInfoMock, _blobContainerClientMock);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.FailureReason, Is.Null);
        Assert.That(result.RowKey, Does.StartWith("api-response-"));
        await _httpMessageHandlerMock.ReceivedWithAnyArgs(1).Send(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>());
        await _blobContainerClientMock.ReceivedWithAnyArgs(1).CreateIfNotExistsAsync();
        await _blobClientMock.ReceivedWithAnyArgs(1).UploadAsync(Arg.Any<Stream>());
    }

    [Test]
    public async Task RunRequestTrackerFunction_ShouldLogFailureResponse()
    {
        // Arrange
        var httpResponse = Substitute.For<HttpResponseMessage>();
        httpResponse.StatusCode = HttpStatusCode.Unauthorized;
        httpResponse.Content = new StringContent("test");

        _httpMessageHandlerMock.Send(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()).Returns(httpResponse);

        // Act
        var result = await _sut.RunRequestTrackerFunction(_timerInfoMock, _blobContainerClientMock);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.FailureReason, Does.Contain("Unauthorized"));
        Assert.That(result.RowKey, Does.StartWith("api-response-"));
        await _httpMessageHandlerMock.ReceivedWithAnyArgs(1).Send(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>());
        await _blobContainerClientMock.ReceivedWithAnyArgs(1).CreateIfNotExistsAsync();
        await _blobClientMock.ReceivedWithAnyArgs(1).UploadAsync(Arg.Any<Stream>());
    }

    [Test]
    public async Task RunGetLogPayload_ShouldReturn404WhenEntryNotFound()
    {
        // Arrange
        _blobClientMock.ExistsAsync().Returns(Task.FromResult(Response.FromValue(false, Substitute.For<Response>())));

        // Act
        var result = await _sut.RunGetLogPayload(default, default, _blobContainerClientMock);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
        Assert.That(((NotFoundResult)result).StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task RunGetLogPayload_ShouldReturnFileStreamResultWhenEntryFound()
    {
        // Arrange
        _blobClientMock.ExistsAsync().Returns(Task.FromResult(Response.FromValue(true, Substitute.For<Response>())));

        var streamingResult = Response.FromValue(BlobsModelFactory.BlobDownloadStreamingResult(content: new MemoryStream()), Substitute.For<Response>());
        _blobClientMock.DownloadStreamingAsync().Returns(Task.FromResult(streamingResult));

        // Act
        var result = await _sut.RunGetLogPayload(default, default, _blobContainerClientMock);

        // Assert
        Assert.That(result, Is.TypeOf<FileStreamResult>());
        await _blobClientMock.ReceivedWithAnyArgs(1).DownloadStreamingAsync();
    }
}