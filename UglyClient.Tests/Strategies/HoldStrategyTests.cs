using Moq;
using UglyClient.Services;
using UglyClient.Strategies;

namespace UglyClient.Tests.Strategies;

/// <summary>
/// Unit tests for <see cref="HoldStrategy"/>.
/// </summary>
public class HoldStrategyTests
{
    [Fact]
    public async Task ExecuteAsync_BelowTarget_AppliesMinimalHeatingAndPollsTemperature()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        heaterService.Setup(service => service.SetAllHeatersAsync(1)).Returns(Task.CompletedTask).Verifiable();
        fanService.Setup(service => service.SetAllFansAsync(false)).Returns(Task.CompletedTask).Verifiable();
        sensorService.Setup(service => service.GetAverageTemperatureAsync()).ReturnsAsync(16.0).Verifiable();

        var strategy = new HoldStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(15.0, 16.0, 1);

        Assert.Equal(16.0, result, precision: 5);
        heaterService.Verify(service => service.SetAllHeatersAsync(1), Times.Once);
        fanService.Verify(service => service.SetAllFansAsync(false), Times.Once);
        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AboveTarget_AppliesCoolingAndPollsTemperature()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        heaterService.Setup(service => service.SetAllHeatersAsync(0)).Returns(Task.CompletedTask).Verifiable();
        fanService.Setup(service => service.SetAllFansAsync(true)).Returns(Task.CompletedTask).Verifiable();
        sensorService.Setup(service => service.GetAverageTemperatureAsync()).ReturnsAsync(16.0).Verifiable();

        var strategy = new HoldStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(17.5, 16.0, 1);

        Assert.Equal(16.0, result, precision: 5);
        heaterService.Verify(service => service.SetAllHeatersAsync(0), Times.Once);
        fanService.Verify(service => service.SetAllFansAsync(true), Times.Once);
        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AtTarget_PollsWithoutChangingDevices()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);

        sensorService.Setup(service => service.GetAverageTemperatureAsync()).ReturnsAsync(16.0).Verifiable();

        var strategy = new HoldStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        var result = await strategy.ExecuteAsync(16.0, 16.0, 1);

        Assert.Equal(16.0, result, precision: 5);
        heaterService.Verify(service => service.SetAllHeatersAsync(It.IsAny<int>()), Times.Never);
        fanService.Verify(service => service.SetAllFansAsync(It.IsAny<bool>()), Times.Never);
        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Once);
    }

    [Fact]
    public void Constructor_NullHeaterService_ThrowsArgumentNullException()
    {
        var fanService = new Mock<IFanService>();
        var sensorService = new Mock<ISensorService>();

        Assert.Throws<ArgumentNullException>(() =>
            new HoldStrategy(null!, fanService.Object, sensorService.Object));
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var heaterService = new Mock<IHeaterService>(MockBehavior.Loose);
        var fanService = new Mock<IFanService>(MockBehavior.Loose);
        var sensorService = new Mock<ISensorService>(MockBehavior.Loose);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var strategy = new HoldStrategy(
            heaterService.Object,
            fanService.Object,
            sensorService.Object,
            (_, _) => Task.CompletedTask);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => strategy.ExecuteAsync(16.0, 16.0, 5, cts.Token));
    }
}
