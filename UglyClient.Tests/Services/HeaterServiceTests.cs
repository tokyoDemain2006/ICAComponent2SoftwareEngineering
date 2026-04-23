using System.Net;
using System.Text;
using Moq;
using Moq.Protected;
using UglyClient.Services;
using Xunit;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="HeaterService"/>.
/// Verifies that the service sends correctly structured HTTP requests and correctly
/// parses responses or throws descriptive exceptions on failure.
/// </summary>
public class HeaterServiceTests
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
        Assert.Throws<ArgumentNullException>(() => new HeaterService(null!));
    }

    /// <summary>
    /// Passing a heater count of zero should throw <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void Constructor_ZeroHeaterCount_ThrowsArgumentOutOfRangeException()
    {
        var client = BuildClient(CreateHandler(HttpStatusCode.OK, string.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HeaterService(client, 0));
    }

    // ── SetHeaterLevelAsync ────────────────────────────────────────────────────

    /// <summary>
    /// When the API returns 200 OK, <see cref="HeaterService.SetHeaterLevelAsync"/> should
    /// complete without throwing.
    /// </summary>
    [Fact]
    public async Task SetHeaterLevelAsync_SuccessResponse_DoesNotThrow()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, string.Empty);
        var service = new HeaterService(BuildClient(handler));

        // Act &amp; Assert — no exception expected
        await service.SetHeaterLevelAsync(1, 3);
    }

    /// <summary>
    /// <see cref="HeaterService.SetHeaterLevelAsync"/> must POST the integer level as a
    /// string to <c>api/heat/{heaterId}</c>.
    /// </summary>
    [Fact]
    public async Task SetHeaterLevelAsync_PostsCorrectEndpointAndBody()
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

        var service = new HeaterService(BuildClient(mock));

        // Act
        await service.SetHeaterLevelAsync(2, 5);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.EndsWith("api/heat/2", captured.RequestUri!.ToString());
        var body = await captured.Content!.ReadAsStringAsync();
        Assert.Equal("5", body);
    }

    /// <summary>
    /// When the API returns a non-success status code, <see cref="HeaterService.SetHeaterLevelAsync"/>
    /// must throw an <see cref="Exception"/> that mentions the heater ID.
    /// </summary>
    [Fact]
    public async Task SetHeaterLevelAsync_HttpFailure_ThrowsExceptionWithHeaterId()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.ServiceUnavailable, string.Empty);
        var service = new HeaterService(BuildClient(handler));

        // Act &amp; Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.SetHeaterLevelAsync(1, 3));
        Assert.Contains("1", ex.Message);
    }

    // ── GetHeaterLevelAsync ────────────────────────────────────────────────────

    /// <summary>
    /// When the API returns a valid integer string body, <see cref="HeaterService.GetHeaterLevelAsync"/>
    /// should parse and return the correct integer value.
    /// </summary>
    [Fact]
    public async Task GetHeaterLevelAsync_ValidIntegerResponse_ReturnsLevel()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, "4");
        var service = new HeaterService(BuildClient(handler));

        // Act
        var level = await service.GetHeaterLevelAsync(1);

        // Assert
        Assert.Equal(4, level);
    }

    /// <summary>
    /// <see cref="HeaterService.GetHeaterLevelAsync"/> should GET from <c>api/heat/{heaterId}/level</c>.
    /// </summary>
    [Fact]
    public async Task GetHeaterLevelAsync_CallsCorrectEndpoint()
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
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("2", Encoding.UTF8, "application/json")
            });

        var service = new HeaterService(BuildClient(mock));

        // Act
        await service.GetHeaterLevelAsync(3);

        // Assert
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.EndsWith("api/heat/3/level", captured.RequestUri!.ToString());
    }

    /// <summary>
    /// When the API returns a non-success status code, <see cref="HeaterService.GetHeaterLevelAsync"/>
    /// must throw an <see cref="Exception"/> that mentions the heater ID.
    /// </summary>
    [Fact]
    public async Task GetHeaterLevelAsync_HttpFailure_ThrowsExceptionWithHeaterId()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.NotFound, string.Empty);
        var service = new HeaterService(BuildClient(handler));

        // Act &amp; Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetHeaterLevelAsync(2));
        Assert.Contains("2", ex.Message);
    }

    /// <summary>
    /// When the API returns a body that cannot be parsed as an integer,
    /// <see cref="HeaterService.GetHeaterLevelAsync"/> must throw an <see cref="Exception"/>.
    /// </summary>
    [Fact]
    public async Task GetHeaterLevelAsync_NonIntegerResponse_ThrowsException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, "not-a-number");
        var service = new HeaterService(BuildClient(handler));

        // Act &amp; Assert
        await Assert.ThrowsAsync<Exception>(() => service.GetHeaterLevelAsync(1));
    }

    // ── SetAllHeatersAsync ─────────────────────────────────────────────────────

    /// <summary>
    /// <see cref="HeaterService.SetAllHeatersAsync"/> must issue one POST per heater
    /// (heaterCount = 2 here), all returning 200, without throwing.
    /// </summary>
    [Fact]
    public async Task SetAllHeatersAsync_AllSucceed_DoesNotThrow()
    {
        // Arrange — handler always returns 200; heaterCount = 2 means exactly 2 POST calls
        var handler = CreateHandler(HttpStatusCode.OK, string.Empty);
        var service = new HeaterService(BuildClient(handler), heaterCount: 2);

        // Act &amp; Assert
        await service.SetAllHeatersAsync(3);
    }

    /// <summary>
    /// When any heater POST fails, <see cref="HeaterService.SetAllHeatersAsync"/> must
    /// propagate the exception from <see cref="HeaterService.SetHeaterLevelAsync"/>.
    /// </summary>
    [Fact]
    public async Task SetAllHeatersAsync_OneFails_ThrowsException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.InternalServerError, string.Empty);
        var service = new HeaterService(BuildClient(handler), heaterCount: 3);

        // Act &amp; Assert
        await Assert.ThrowsAsync<Exception>(() => service.SetAllHeatersAsync(0));
    }

    // ── GetAllHeaterLevelsAsync ────────────────────────────────────────────────

    /// <summary>
    /// <see cref="HeaterService.GetAllHeaterLevelsAsync"/> should return exactly
    /// <c>heaterCount</c> integer values when all requests succeed.
    /// </summary>
    [Fact]
    public async Task GetAllHeaterLevelsAsync_AllSucceed_ReturnsCorrectCount()
    {
        // Arrange — same level returned for every heater
        var handler = CreateHandler(HttpStatusCode.OK, "2");
        var service = new HeaterService(BuildClient(handler), heaterCount: 3);

        // Act
        var levels = await service.GetAllHeaterLevelsAsync();

        // Assert
        Assert.Equal(3, levels.Count());
    }

    /// <summary>
    /// Each item returned by <see cref="HeaterService.GetAllHeaterLevelsAsync"/> should
    /// equal the parsed integer from the API response body.
    /// </summary>
    [Fact]
    public async Task GetAllHeaterLevelsAsync_AllSucceed_ReturnsCorrectValues()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, "5");
        var service = new HeaterService(BuildClient(handler), heaterCount: 2);

        // Act
        var levels = await service.GetAllHeaterLevelsAsync();

        // Assert — every heater should report level 5
        Assert.All(levels, l => Assert.Equal(5, l));
    }

    /// <summary>
    /// When any individual heater GET fails, <see cref="HeaterService.GetAllHeaterLevelsAsync"/>
    /// must propagate the exception.
    /// </summary>
    [Fact]
    public async Task GetAllHeaterLevelsAsync_OneFails_ThrowsException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.NotFound, string.Empty);
        var service = new HeaterService(BuildClient(handler), heaterCount: 2);

        // Act &amp; Assert
        await Assert.ThrowsAsync<Exception>(() => service.GetAllHeaterLevelsAsync());
    }
}
