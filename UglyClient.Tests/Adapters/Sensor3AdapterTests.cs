using Moq;
using UglyClient.Adapters;
using UglyClient.Services;

namespace UglyClient.Tests.Adapters;

/// <summary>
/// Unit tests for <see cref="Sensor3Adapter"/>.
/// </summary>
public class Sensor3AdapterTests
{
    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Sensor3Adapter(null!));
    }

    [Fact]
    public async Task GetTemperatureAsync_ValidResponse_ReturnsDouble()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/Sensor/sensor3")).ReturnsAsync("23.75");
        var adapter = new Sensor3Adapter(httpService.Object);

        var result = await adapter.GetTemperatureAsync();

        Assert.Equal(23.75, result, precision: 5);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.GetAsync("api/Sensor/sensor3"))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var adapter = new Sensor3Adapter(httpService.Object);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => adapter.GetTemperatureAsync());

        Assert.True(exception.Message.Contains("sensor 3", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenResponseIsInvalid_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/Sensor/sensor3")).ReturnsAsync("not-a-number");
        var adapter = new Sensor3Adapter(httpService.Object);

        await Assert.ThrowsAsync<DeviceServiceException>(() => adapter.GetTemperatureAsync());
    }
}
