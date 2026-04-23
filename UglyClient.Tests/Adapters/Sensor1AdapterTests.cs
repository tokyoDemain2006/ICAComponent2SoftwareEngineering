using System.Net;
using Moq;
using Moq.Protected;
using UglyClient.Adapters;
using Xunit;

namespace UglyClient.Tests.Adapters;

/// <summary>
/// Unit tests for <see cref="Sensor1Adapter"/>.
/// Verifies that the adapter correctly converts the raw <see cref="string"/> HTTP response
/// to a <see cref="double"/> and that HTTP failure paths throw the expected exceptions.
/// </summary>
public class Sensor1AdapterTests
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> whose <see cref="HttpMessageHandler"/> is backed by
    /// the supplied mock, allowing HTTP responses to be controlled in tests.
    /// </summary>
    /// <param name="handlerMock">The mocked <see cref="HttpMessageHandler"/>.</param>
    /// <returns>An <see cref="HttpClient"/> wired to the mock handler.</returns>
    private static HttpClient BuildClient(Mock<HttpMessageHandler> handlerMock)
    {
        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://test-server/")
        };
    }

    /// <summary>
    /// Creates a mock handler that returns the given body with the given status code.
    /// </summary>
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
                Content = new StringContent(responseBody)
            });
        return mock;
    }

    /// <summary>
    /// When the API returns a valid numeric string, <see cref="Sensor1Adapter.GetTemperatureAsync"/>
    /// should parse it and return the expected <see cref="double"/>.
    /// </summary>
    [Fact]
    public async Task GetTemperatureAsync_ValidStringResponse_ReturnsDouble()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, "21.5");
        var adapter = new Sensor1Adapter(BuildClient(handler));

        // Act
        var result = await adapter.GetTemperatureAsync();

        // Assert
        Assert.Equal(21.5, result, precision: 5);
    }

    /// <summary>
    /// When the API returns an integer string, the adapter should still parse it correctly.
    /// </summary>
    [Fact]
    public async Task GetTemperatureAsync_IntegerStringResponse_ReturnsDouble()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, "19");
        var adapter = new Sensor1Adapter(BuildClient(handler));

        // Act
        var result = await adapter.GetTemperatureAsync();

        // Assert
        Assert.Equal(19.0, result, precision: 5);
    }

    /// <summary>
    /// When the HTTP response is not a success status code, an <see cref="Exception"/> should
    /// be thrown describing the failure.
    /// </summary>
    [Fact]
    public async Task GetTemperatureAsync_HttpFailure_ThrowsException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.ServiceUnavailable, string.Empty);
        var adapter = new Sensor1Adapter(BuildClient(handler));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => adapter.GetTemperatureAsync());
        Assert.Contains("Sensor 1", ex.Message);
    }

    /// <summary>
    /// When the HTTP response body is not a parseable number, an <see cref="Exception"/>
    /// should be thrown with an appropriate message.
    /// </summary>
    [Fact]
    public async Task GetTemperatureAsync_UnparsableBody_ThrowsException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, "not-a-number");
        var adapter = new Sensor1Adapter(BuildClient(handler));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => adapter.GetTemperatureAsync());
        Assert.Contains("Sensor 1", ex.Message);
    }

    /// <summary>
    /// Passing a null <see cref="HttpClient"/> to the constructor should throw
    /// <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Sensor1Adapter(null!));
    }
}
