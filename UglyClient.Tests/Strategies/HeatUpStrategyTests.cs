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

    [Fact]
    public void Constructor_NullHeaterService_ThrowsArgumentNullException()
    {
        var fanService = new Mock<IFanService>();
        var sensorService = new Mock<ISensorService>();

        Assert.Throws<ArgumentNullException>(() =>
            new HeatUpStrategy(null!, fanService.Object, sensorService.Object));
    }

    [Fact]
    public async Task ExecuteAsync_WithinTolerance_ExitsWithoutCallingDevices()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        var strategy = new HeatUpStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        // 19.95 is within 0.1°C of 20.0 so HasReachedTarget returns true immediately
        var result = await strategy.ExecuteAsync(19.95, 20.0, 5);

        Assert.Equal(19.95, result, precision: 5);
        heaterService.VerifyNoOtherCalls();
        fanService.VerifyNoOtherCalls();
        sensorService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Loose);
        var fanService = new Mock<IFanService>(MockBehavior.Loose);
        var sensorService = new Mock<ISensorService>(MockBehavior.Loose);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var strategy = new HeatUpStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => strategy.ExecuteAsync(18.0, 20.0, 5, cts.Token));
    }
}
