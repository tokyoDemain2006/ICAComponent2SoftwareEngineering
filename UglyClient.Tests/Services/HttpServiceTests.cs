using System.Net;
using Moq;
using Moq.Protected;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="HttpService"/>.
/// </summary>
public class HttpServiceTests
{
    [Fact]
    public async Task GetAsync_SuccessResponse_ReturnsBody()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok")
        });
        var httpService = new HttpService(CreateClient(handler));

        var result = await httpService.GetAsync("api/test");

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task GetAsync_NonSuccessStatus_ThrowsDeviceServiceException()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var httpService = new HttpService(CreateClient(handler));

        await Assert.ThrowsAsync<DeviceServiceException>(() => httpService.GetAsync("api/test"));
    }

    [Fact]
    public async Task PostAsync_TransportFailure_ThrowsDeviceServiceException()
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network error"));
        var httpService = new HttpService(CreateClient(handler));

        await Assert.ThrowsAsync<DeviceServiceException>(() => httpService.PostAsync("api/test", "{}"));
    }

    private static HttpClient CreateClient(Mock<HttpMessageHandler> handler)
    {
        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://test-server/")
        };
    }

    private static Mock<HttpMessageHandler> CreateHandler(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return handler;
    }
}
