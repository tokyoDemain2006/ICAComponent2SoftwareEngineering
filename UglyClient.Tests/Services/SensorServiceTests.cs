using Moq;
using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SensorService"/>.
/// </summary>
public class SensorServiceTests
{
    [Fact]
    public void Constructor_NullSensors_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SensorService(null!));
    }

    [Fact]
    public void Constructor_EmptySensors_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SensorService([]));
    }

    [Fact]
    public void Constructor_DuplicateSensorIds_ThrowsArgumentException()
    {
        var sensor1 = CreateSensorMock(1, 20.0);
        var duplicateSensor1 = CreateSensorMock(1, 22.0);

        Assert.Throws<ArgumentException>(() => new SensorService([sensor1.Object, duplicateSensor1.Object]));
    }

    [Fact]
    public async Task GetTemperatureAsync_ValidSensorId_DelegatesToMatchingAdapter()
    {
        var sensor1 = CreateSensorMock(1, 20.0);
        var sensor2 = CreateSensorMock(2, 24.5);
        var sensorService = new SensorService([sensor1.Object, sensor2.Object]);

        var result = await sensorService.GetTemperatureAsync(2);

        Assert.Equal(24.5, result, precision: 5);
        sensor1.Verify(sensor => sensor.GetTemperatureAsync(), Times.Never);
        sensor2.Verify(sensor => sensor.GetTemperatureAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTemperatureAsync_InvalidSensorId_ThrowsDeviceServiceException()
    {
        var sensor1 = CreateSensorMock(1, 20.0);
        var sensorService = new SensorService([sensor1.Object]);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => sensorService.GetTemperatureAsync(3));

        Assert.True(exception.Message.Contains("Sensor 3", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAverageTemperatureAsync_ReturnsAverage()
    {
        var sensor1 = CreateSensorMock(1, 18.0);
        var sensor2 = CreateSensorMock(2, 21.0);
        var sensor3 = CreateSensorMock(3, 24.0);
        var sensorService = new SensorService([sensor1.Object, sensor2.Object, sensor3.Object]);

        var result = await sensorService.GetAverageTemperatureAsync();

        Assert.Equal(21.0, result, precision: 5);
    }

    [Fact]
    public async Task GetAverageTemperatureAsync_WhenAdapterFails_PropagatesDeviceServiceException()
    {
        var sensor1 = CreateSensorMock(1, 18.0);
        var sensor2 = new Mock<ISensor>();
        sensor2.SetupGet(sensor => sensor.SensorId).Returns(2);
        sensor2
            .Setup(sensor => sensor.GetTemperatureAsync())
            .ThrowsAsync(new DeviceServiceException("Unable to load sensor 2 right now."));
        var sensorService = new SensorService([sensor1.Object, sensor2.Object]);

        await Assert.ThrowsAsync<DeviceServiceException>(() => sensorService.GetAverageTemperatureAsync());
    }

    private static Mock<ISensor> CreateSensorMock(int sensorId, double temperature)
    {
        var sensor = new Mock<ISensor>();
        sensor.SetupGet(adapter => adapter.SensorId).Returns(sensorId);
        sensor.Setup(adapter => adapter.GetTemperatureAsync()).ReturnsAsync(temperature);
        return sensor;
    }
}
