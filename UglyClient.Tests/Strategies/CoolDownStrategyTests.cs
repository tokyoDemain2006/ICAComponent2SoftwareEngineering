using Moq;
using UglyClient.Services;
using UglyClient.Strategies;

namespace UglyClient.Tests.Strategies;

/// <summary>
/// Unit tests for <see cref="CoolDownStrategy"/>.
/// </summary>
public class CoolDownStrategyTests
{
    [Fact]
    public async Task ExecuteAsync_AboveTarget_CoolsUntilTargetIsReached()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        heaterService.Setup(service => service.SetAllHeatersAsync(0)).Returns(Task.CompletedTask).Verifiable();
        fanService.Setup(service => service.SetAllFansAsync(true)).Returns(Task.CompletedTask).Verifiable();
        sensorService.Setup(service => service.GetAverageTemperatureAsync()).ReturnsAsync(16.0).Verifiable();

        var strategy = new CoolDownStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(19.0, 16.0, 5);

        Assert.Equal(16.0, result, precision: 5);
        heaterService.Verify(service => service.SetAllHeatersAsync(0), Times.Once);
        fanService.Verify(service => service.SetAllFansAsync(true), Times.Once);
        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyBelowTarget_ReturnsWithoutChangingDevices()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        var strategy = new CoolDownStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(15.9, 16.0, 5);

        Assert.Equal(15.9, result, precision: 5);
        heaterService.Verify(service => service.SetAllHeatersAsync(It.IsAny<int>()), Times.Never);
        fanService.Verify(service => service.SetAllFansAsync(It.IsAny<bool>()), Times.Never);
        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Never);
    }
}
