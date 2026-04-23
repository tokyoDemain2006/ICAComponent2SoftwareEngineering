using System.Net;
using System.Text;
using Moq;
using Moq.Protected;
using UglyClient.Services;
using Xunit;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FanService"/>.
/// Verifies that the service sends correctly structured HTTP requests and correctly
/// deserializes responses or throws descriptive exceptions on failure.
/// </summary>
public class FanServiceTests
{
    // ── helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds an <see cref="HttpClient"/> whose transport is backed by the supplied mock
    /// handler, with a predictable base address for URL assertions.
    /// </summary>
    /// <param name="handlerMock">The mocked <see cref="HttpMessageHandler"/>.</param>
    /// <returns>An <see cref="HttpClient"/> wired to the mock handler.</returns>
    private static HttpClient BuildClient(Mock<HttpMessageHandler> handlerMock)
        => new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://test-server/")
        };

    /// <summary>
    /// Creates a mock handler that always returns the given status code and body,
    /// regardless of the request URI or method.
    /// </summary>
    /// <param name="statusCode">HTTP status code to return.</param>
    /// <param name="responseBody">Response body text to return.</param>
    /// <returns>A configured <see cref="Mock{HttpMessageHandler}"/>.</returns>
    private static Mock<HttpMessageHandler> CreateHandler(HttpStatusCode statusCode, string responseBody)
    {
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        return mock;
    }

    // ── Constructor ────────────────────────────────────────────────────────────

    /// <summary>
    /// Passing a null <see cref="HttpClient"/> should throw <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FanService(null!));
    }

    /// <summary>
    /// Passing a fan count of zero should throw <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void Constructor_ZeroFanCount_ThrowsArgumentOutOfRangeException()
    {
        var client = BuildClient(CreateHandler(HttpStatusCode.OK, string.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FanService(client, 0));
    }

    // ── SetFanStateAsync ───────────────────────────────────────────────────────

    /// <summary>
    /// When the API returns 200 OK, <see cref="FanService.SetFanStateAsync"/> should complete
    /// without throwing.
    /// </summary>
    [Fact]
    public async Task SetFanStateAsync_SuccessResponse_DoesNotThrow()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, string.Empty);
        var service = new FanService(BuildClient(handler));

        // Act &amp; Assert — no exception expected
        await service.SetFanStateAsync(1, true);
    }

    /// <summary>
    /// When turning a fan ON, <see cref="FanService.SetFanStateAsync"/> must POST the body
    /// <c>"true"</c> (lower-case) to <c>api/fans/{fanId}</c>.
    /// </summary>
    [Fact]
    public async Task SetFanStateAsync_TurnOn_PostsTrueLowercase()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var service = new FanService(BuildClient(mock));

        // Act
        await service.SetFanStateAsync(2, true);

        // Assert — correct endpoint and body
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.EndsWith("api/fans/2", captured.RequestUri!.ToString());
        var body = await captured.Content!.ReadAsStringAsync();
        Assert.Equal("true", body);
    }

    /// <summary>
    /// When turning a fan OFF, the body must be <c>"false"</c> (lower-case).
    /// </summary>
    [Fact]
    public async Task SetFanStateAsync_TurnOff_PostsFalseLowercase()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var service = new FanService(BuildClient(mock));

        // Act
        await service.SetFanStateAsync(3, false);

        // Assert
        var body = await captured!.Content!.ReadAsStringAsync();
        Assert.Equal("false", body);
    }

    /// <summary>
    /// When the API returns a non-success status code, <see cref="FanService.SetFanStateAsync"/>
    /// must throw an <see cref="Exception"/> that mentions the fan ID.
    /// </summary>
    [Fact]
    public async Task SetFanStateAsync_HttpFailure_ThrowsExceptionWithFanId()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.ServiceUnavailable, string.Empty);
        var service = new FanService(BuildClient(handler));

        // Act &amp; Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.SetFanStateAsync(1, true));
        Assert.Contains("1", ex.Message);
    }

    // ── GetFanStateAsync ───────────────────────────────────────────────────────

    /// <summary>
    /// When the API returns valid JSON, <see cref="FanService.GetFanStateAsync"/> should
    /// deserialize it into a <see cref="UglyClient.Models.FanDTO"/> with correct values.
    /// </summary>
    [Fact]
    public async Task GetFanStateAsync_ValidJson_ReturnsFanDTO()
    {
        // Arrange
        const string json = """{"Id":2,"IsOn":true}""";
        var handler = CreateHandler(HttpStatusCode.OK, json);
        var service = new FanService(BuildClient(handler));

        // Act
        var fan = await service.GetFanStateAsync(2);

        // Assert
        Assert.Equal(2, fan.Id);
        Assert.True(fan.IsOn);
    }

    /// <summary>
    /// <see cref="FanService.GetFanStateAsync"/> should GET from <c>api/fans/{fanId}/state</c>.
    /// </summary>
    [Fact]
    public async Task GetFanStateAsync_CallsCorrectEndpoint()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        const string json = """{"Id":1,"IsOn":false}""";
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var service = new FanService(BuildClient(mock));

        // Act
        await service.GetFanStateAsync(1);

        // Assert
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.EndsWith("api/fans/1/state", captured.RequestUri!.ToString());
    }

    /// <summary>
    /// When the API returns a non-success status code, <see cref="FanService.GetFanStateAsync"/>
    /// must throw an <see cref="Exception"/> that mentions the fan ID.
    /// </summary>
    [Fact]
    public async Task GetFanStateAsync_HttpFailure_ThrowsExceptionWithFanId()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.NotFound, string.Empty);
        var service = new FanService(BuildClient(handler));

        // Act &amp; Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetFanStateAsync(5));
        Assert.Contains("5", ex.Message);
    }

    // ── SetAllFansAsync ────────────────────────────────────────────────────────

    /// <summary>
    /// <see cref="FanService.SetAllFansAsync"/> must issue one POST per fan (fanCount = 2 here),
    /// all returning 200, without throwing.
    /// </summary>
    [Fact]
    public async Task SetAllFansAsync_AllSucceed_DoesNotThrow()
    {
        // Arrange — handler always returns 200; fanCount = 2 means exactly 2 POST calls
        var handler = CreateHandler(HttpStatusCode.OK, string.Empty);
        var service = new FanService(BuildClient(handler), fanCount: 2);

        // Act &amp; Assert
        await service.SetAllFansAsync(true);
    }

    /// <summary>
    /// When any fan POST fails, <see cref="FanService.SetAllFansAsync"/> must propagate the
    /// exception from <see cref="FanService.SetFanStateAsync"/>.
    /// </summary>
    [Fact]
    public async Task SetAllFansAsync_OneFails_ThrowsException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.InternalServerError, string.Empty);
        var service = new FanService(BuildClient(handler), fanCount: 3);

        // Act &amp; Assert
        await Assert.ThrowsAsync<Exception>(() => service.SetAllFansAsync(false));
    }

    // ── GetAllFanStatesAsync ───────────────────────────────────────────────────

    /// <summary>
    /// <see cref="FanService.GetAllFanStatesAsync"/> should return exactly <c>fanCount</c>
    /// <see cref="UglyClient.Models.FanDTO"/> items when all requests succeed.
    /// </summary>
    [Fact]
    public async Task GetAllFanStatesAsync_AllSucceed_ReturnsCorrectCount()
    {
        // Arrange — same JSON returned for every fan
        const string json = """{"Id":1,"IsOn":true}""";
        var handler = CreateHandler(HttpStatusCode.OK, json);
        var service = new FanService(BuildClient(handler), fanCount: 3);

        // Act
        var fans = await service.GetAllFanStatesAsync();

        // Assert
        Assert.Equal(3, fans.Count());
    }

    /// <summary>
    /// When any individual fan GET fails, <see cref="FanService.GetAllFanStatesAsync"/>
    /// must propagate the exception.
    /// </summary>
    [Fact]
    public async Task GetAllFanStatesAsync_OneFails_ThrowsException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.NotFound, string.Empty);
        var service = new FanService(BuildClient(handler), fanCount: 2);

        // Act &amp; Assert
        await Assert.ThrowsAsync<Exception>(() => service.GetAllFanStatesAsync());
    }
}
