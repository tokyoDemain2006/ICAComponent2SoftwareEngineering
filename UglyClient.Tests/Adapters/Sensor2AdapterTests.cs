using Moq;
using UglyClient.Adapters;
using UglyClient.Services;

namespace UglyClient.Tests.Adapters;

/// <summary>
/// Unit tests for <see cref="Sensor2Adapter"/>.
/// </summary>
public class Sensor2AdapterTests
{
    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Sensor2Adapter(null!));
    }

    [Fact]
    public async Task GetTemperatureAsync_ValidResponse_ReturnsDouble()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/Sensor/sensor2")).ReturnsAsync("22");
        var adapter = new Sensor2Adapter(httpService.Object);

        var result = await adapter.GetTemperatureAsync();

        Assert.Equal(22.0, result, precision: 5);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.GetAsync("api/Sensor/sensor2"))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var adapter = new Sensor2Adapter(httpService.Object);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => adapter.GetTemperatureAsync());

        Assert.True(exception.Message.Contains("sensor 2", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenResponseIsInvalid_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/Sensor/sensor2")).ReturnsAsync("21.7");
        var adapter = new Sensor2Adapter(httpService.Object);

        await Assert.ThrowsAsync<DeviceServiceException>(() => adapter.GetTemperatureAsync());
    }

    [Fact]
    public void SensorId_ReturnsTwo()
    {
        var httpService = new Mock<IHttpService>();
        var adapter = new Sensor2Adapter(httpService.Object);

        Assert.Equal(2, adapter.SensorId);
    }
}
