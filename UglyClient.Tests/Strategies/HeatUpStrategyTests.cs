using Moq;
using UglyClient.Services;
using UglyClient.Strategies;

namespace UglyClient.Tests.Strategies;

/// <summary>
/// Unit tests for <see cref="HeatUpStrategy"/>.
/// </summary>
public class HeatUpStrategyTests
{
    [Fact]
    public async Task ExecuteAsync_BelowTarget_HeatsUntilTargetIsReached()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        heaterService.Setup(service => service.SetAllHeatersAsync(3)).Returns(Task.CompletedTask).Verifiable();
        fanService.Setup(service => service.SetAllFansAsync(false)).Returns(Task.CompletedTask).Verifiable();
        sensorService.Setup(service => service.GetAverageTemperatureAsync()).ReturnsAsync(20.0).Verifiable();

        var strategy = new HeatUpStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(18.0, 20.0, 5);

        Assert.Equal(20.0, result, precision: 5);
        heaterService.Verify(service => service.SetAllHeatersAsync(3), Times.Once);
        fanService.Verify(service => service.SetAllFansAsync(false), Times.Once);
        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyAtTarget_ReturnsWithoutChangingDevices()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        var strategy = new HeatUpStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(20.0, 20.0, 5);

        Assert.Equal(20.0, result, precision: 5);
        heaterService.Verify(service => service.SetAllHeatersAsync(It.IsAny<int>()), Times.Never);
        fanService.Verify(service => service.SetAllFansAsync(It.IsAny<bool>()), Times.Never);
        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Never);
    }
}
