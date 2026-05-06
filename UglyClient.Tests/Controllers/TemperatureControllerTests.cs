using Moq;
using UglyClient.Controllers;
using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="TemperatureController"/>.
/// </summary>
public class TemperatureControllerTests
{
    [Fact]
    public void Constructor_NullFanService_ThrowsArgumentNullException()
    {
        var heaterService = new Mock<IHeaterService>();
        var sensorService = new Mock<ISensorService>();

        Assert.Throws<ArgumentNullException>(() =>
            new TemperatureController(null!, heaterService.Object, sensorService.Object));
    }

    [Fact]
    public void Constructor_NullHeaterService_ThrowsArgumentNullException()
    {
        var fanService = new Mock<IFanService>();
        var sensorService = new Mock<ISensorService>();

        Assert.Throws<ArgumentNullException>(() =>
            new TemperatureController(fanService.Object, null!, sensorService.Object));
    }

    [Fact]
    public void Constructor_NullSensorService_ThrowsArgumentNullException()
    {
        var fanService = new Mock<IFanService>();
        var heaterService = new Mock<IHeaterService>();

        Assert.Throws<ArgumentNullException>(() =>
            new TemperatureController(fanService.Object, heaterService.Object, null!));
    }

    [Fact]
    public async Task RunPhaseAsync_NullStrategy_ThrowsArgumentNullException()
    {
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);
        var controller = new TemperatureController(fanService.Object, heaterService.Object, sensorService.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            controller.RunPhaseAsync(null!, 18.0, 20.0, 5));
    }

    [Fact]
    public async Task RunPhaseAsync_DelegatesToProvidedStrategyWithSuppliedArguments()
    {
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);
        var strategy = new Mock<ITemperatureControlStrategy>(MockBehavior.Strict);
        using var cancellationSource = new CancellationTokenSource();

        strategy
            .Setup(service => service.ExecuteAsync(17.2, 20.0, 30, cancellationSource.Token))
            .ReturnsAsync(19.4)
            .Verifiable();

        var controller = new TemperatureController(fanService.Object, heaterService.Object, sensorService.Object);

        var result = await controller.RunPhaseAsync(
            strategy.Object,
            17.2,
            20.0,
            30,
            cancellationSource.Token);

        Assert.Equal(19.4, result, precision: 5);
        strategy.Verify();
    }

    [Fact]
    public async Task RunFullCycleAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var fanService = new Mock<IFanService>(MockBehavior.Loose);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Loose);
        var sensorService = new Mock<ISensorService>(MockBehavior.Loose);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var controller = new TemperatureController(fanService.Object, heaterService.Object, sensorService.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.RunFullCycleAsync(cts.Token));
    }
}
