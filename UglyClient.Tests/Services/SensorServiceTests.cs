using Moq;
using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SensorService"/>.
/// Verifies adapter delegation, average calculation, guard clauses, and error propagation.
/// </summary>
public class SensorServiceTests
{
    /// <summary>
    /// Passing a null sensor collection should throw <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_NullSensors_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SensorService(null!));
    }

    /// <summary>
    /// Passing an empty sensor collection should throw <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Constructor_EmptySensors_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SensorService([]));
    }

    /// <summary>
    /// Duplicate sensor identifiers should be rejected because the service must map each ID
    /// to exactly one adapter.
    /// </summary>
    [Fact]
    public void Constructor_DuplicateSensorIds_ThrowsArgumentException()
    {
        var sensor1 = CreateSensorMock(1, 20.0);
        var duplicateSensor1 = CreateSensorMock(1, 22.0);

        var ex = Assert.Throws<ArgumentException>(() => new SensorService([sensor1.Object, duplicateSensor1.Object]));

        Assert.Contains("Duplicate sensor ID", ex.Message);
    }

    /// <summary>
    /// <see cref="SensorService.GetTemperatureAsync"/> should delegate to the adapter matching
    /// the requested sensor identifier.
    /// </summary>
    [Fact]
    public async Task GetTemperatureAsync_ValidSensorId_DelegatesToMatchingAdapter()
    {
        var sensor1 = CreateSensorMock(1, 20.0);
        var sensor2 = CreateSensorMock(2, 24.5);
        var service = new SensorService([sensor1.Object, sensor2.Object]);

        var result = await service.GetTemperatureAsync(2);

        Assert.Equal(24.5, result, precision: 5);
        sensor1.Verify(s => s.GetTemperatureAsync(), Times.Never);
        sensor2.Verify(s => s.GetTemperatureAsync(), Times.Once);
    }

    /// <summary>
    /// Requesting a sensor ID that is not present should fail clearly.
    /// </summary>
    [Fact]
    public async Task GetTemperatureAsync_InvalidSensorId_ThrowsKeyNotFoundException()
    {
        var sensor1 = CreateSensorMock(1, 20.0);
        var service = new SensorService([sensor1.Object]);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetTemperatureAsync(3));

        Assert.Contains("Sensor 3", ex.Message);
    }

    /// <summary>
    /// <see cref="SensorService.GetAverageTemperatureAsync"/> should average all managed sensors.
    /// </summary>
    [Fact]
    public async Task GetAverageTemperatureAsync_AllSensorsPresent_ReturnsAverage()
    {
        var sensor1 = CreateSensorMock(1, 18.0);
        var sensor2 = CreateSensorMock(2, 21.0);
        var sensor3 = CreateSensorMock(3, 24.0);
        var service = new SensorService([sensor1.Object, sensor2.Object, sensor3.Object]);

        var result = await service.GetAverageTemperatureAsync();

        Assert.Equal(21.0, result, precision: 5);
    }

    /// <summary>
    /// When an adapter throws while calculating the average, the exception should propagate.
    /// </summary>
    [Fact]
    public async Task GetAverageTemperatureAsync_AdapterThrows_PropagatesException()
    {
        var sensor1 = CreateSensorMock(1, 18.0);
        var sensor2 = new Mock<ISensor>();
        sensor2.SetupGet(s => s.SensorId).Returns(2);
        sensor2.Setup(s => s.GetTemperatureAsync()).ThrowsAsync(new Exception("Sensor 2 failed."));

        var service = new SensorService([sensor1.Object, sensor2.Object]);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetAverageTemperatureAsync());

        Assert.Contains("Sensor 2 failed", ex.Message);
    }

    /// <summary>
    /// Builds a mock sensor adapter with a fixed identifier and temperature response.
    /// </summary>
    /// <param name="sensorId">The identifier exposed by the adapter.</param>
    /// <param name="temperature">The temperature returned by <see cref="ISensor.GetTemperatureAsync"/>.</param>
    /// <returns>A configured <see cref="Mock{ISensor}"/>.</returns>
    private static Mock<ISensor> CreateSensorMock(int sensorId, double temperature)
    {
        var sensor = new Mock<ISensor>();
        sensor.SetupGet(s => s.SensorId).Returns(sensorId);
        sensor.Setup(s => s.GetTemperatureAsync()).ReturnsAsync(temperature);
        return sensor;
    }
}
