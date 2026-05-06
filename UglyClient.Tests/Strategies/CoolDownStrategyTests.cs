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

    [Fact]
    public void Constructor_NullHeaterService_ThrowsArgumentNullException()
    {
        var fanService = new Mock<IFanService>();
        var sensorService = new Mock<ISensorService>();

        Assert.Throws<ArgumentNullException>(() =>
            new CoolDownStrategy(null!, fanService.Object, sensorService.Object));
    }

    [Fact]
    public async Task ExecuteAsync_WithinTolerance_ExitsWithoutCallingDevices()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        var strategy = new CoolDownStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(16.05, 16.0, 5);

        Assert.Equal(16.05, result, precision: 5);
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

        var strategy = new CoolDownStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => strategy.ExecuteAsync(19.0, 16.0, 5, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_TargetNotReached_StopsAfterDurationIterations()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        heaterService.Setup(s => s.SetAllHeatersAsync(0)).Returns(Task.CompletedTask);
        fanService.Setup(s => s.SetAllFansAsync(true)).Returns(Task.CompletedTask);
        sensorService.Setup(s => s.GetAverageTemperatureAsync()).ReturnsAsync(20.0);

        var strategy = new CoolDownStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(20.0, 16.0, durationSeconds: 3);

        heaterService.Verify(s => s.SetAllHeatersAsync(0), Times.Exactly(3));
        fanService.Verify(s => s.SetAllFansAsync(true), Times.Exactly(3));
        sensorService.Verify(s => s.GetAverageTemperatureAsync(), Times.Exactly(3));
        Assert.Equal(20.0, result, precision: 5);
    }

    [Fact]
    public void Constructor_NullFanService_ThrowsArgumentNullException()
    {
        var heaterService = new Mock<IHeaterService>();
        var sensorService = new Mock<ISensorService>();

        Assert.Throws<ArgumentNullException>(() =>
            new CoolDownStrategy(heaterService.Object, null!, sensorService.Object));
    }

    [Fact]
    public void Constructor_NullSensorService_ThrowsArgumentNullException()
    {
        var heaterService = new Mock<IHeaterService>();
        var fanService = new Mock<IFanService>();

        Assert.Throws<ArgumentNullException>(() =>
            new CoolDownStrategy(heaterService.Object, fanService.Object, null!));
    }
}
