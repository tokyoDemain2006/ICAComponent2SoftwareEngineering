using Moq;
using UglyClient.Adapters;
using UglyClient.Services;

namespace UglyClient.Tests.Adapters;

/// <summary>
/// Unit tests for <see cref="Sensor1Adapter"/>.
/// </summary>
public class Sensor1AdapterTests
{
    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Sensor1Adapter(null!));
    }

    [Fact]
    public async Task GetTemperatureAsync_ValidResponse_ReturnsDouble()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/Sensor/sensor1")).ReturnsAsync("21.5");
        var adapter = new Sensor1Adapter(httpService.Object);

        var result = await adapter.GetTemperatureAsync();

        Assert.Equal(21.5, result, precision: 5);
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.GetAsync("api/Sensor/sensor1"))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var adapter = new Sensor1Adapter(httpService.Object);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => adapter.GetTemperatureAsync());

        Assert.True(exception.Message.Contains("sensor 1", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetTemperatureAsync_WhenResponseIsInvalid_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/Sensor/sensor1")).ReturnsAsync("not-a-number");
        var adapter = new Sensor1Adapter(httpService.Object);

        await Assert.ThrowsAsync<DeviceServiceException>(() => adapter.GetTemperatureAsync());
    }

    [Fact]
    public void SensorId_ReturnsOne()
    {
        var httpService = new Mock<IHttpService>();
        var adapter = new Sensor1Adapter(httpService.Object);

        Assert.Equal(1, adapter.SensorId);
    }
}
