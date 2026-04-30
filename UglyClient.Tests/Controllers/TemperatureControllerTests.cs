using Moq;
using UglyClient.Controllers;
using UglyClient.Interfaces;
using UglyClient.Services;
using UglyClient.Strategies;

namespace UglyClient.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="TemperatureController"/>.
/// </summary>
public class TemperatureControllerTests
{
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
    public async Task RunFullCycleAsync_RunsExpectedStrategiesInOrder()
    {
        var fanService = new Mock<IFanService>(MockBehavior.Strict);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Strict);
        var sensorService = new Mock<ISensorService>(MockBehavior.Strict);
        using var cancellationSource = new CancellationTokenSource();

        sensorService.Setup(service => service.GetAverageTemperatureAsync()).ReturnsAsync(18.5).Verifiable();

        var controller = new RecordingTemperatureController(
            fanService.Object,
            heaterService.Object,
            sensorService.Object,
            [19.8, 16.1, 16.0, 18.0, 18.0]);

        await controller.RunFullCycleAsync(cancellationSource.Token);

        sensorService.Verify(service => service.GetAverageTemperatureAsync(), Times.Once);
        Assert.Collection(
            controller.PhaseCalls,
            phase =>
            {
                Assert.Equal(typeof(HeatUpStrategy), phase.StrategyType);
                Assert.Equal(18.5, phase.CurrentTemperature, precision: 5);
                Assert.Equal(20.0, phase.TargetTemperature, precision: 5);
                Assert.Equal(30, phase.DurationSeconds);
                Assert.Equal(cancellationSource.Token, phase.CancellationToken);
            },
            phase =>
            {
                Assert.Equal(typeof(CoolDownStrategy), phase.StrategyType);
                Assert.Equal(19.8, phase.CurrentTemperature, precision: 5);
                Assert.Equal(16.0, phase.TargetTemperature, precision: 5);
                Assert.Equal(10, phase.DurationSeconds);
                Assert.Equal(cancellationSource.Token, phase.CancellationToken);
            },
            phase =>
            {
                Assert.Equal(typeof(HoldStrategy), phase.StrategyType);
                Assert.Equal(16.1, phase.CurrentTemperature, precision: 5);
                Assert.Equal(16.0, phase.TargetTemperature, precision: 5);
                Assert.Equal(10, phase.DurationSeconds);
                Assert.Equal(cancellationSource.Token, phase.CancellationToken);
            },
            phase =>
            {
                Assert.Equal(typeof(HeatUpStrategy), phase.StrategyType);
                Assert.Equal(16.0, phase.CurrentTemperature, precision: 5);
                Assert.Equal(18.0, phase.TargetTemperature, precision: 5);
                Assert.Equal(20, phase.DurationSeconds);
                Assert.Equal(cancellationSource.Token, phase.CancellationToken);
            },
            phase =>
            {
                Assert.Equal(typeof(HoldStrategy), phase.StrategyType);
                Assert.Equal(18.0, phase.CurrentTemperature, precision: 5);
                Assert.Equal(18.0, phase.TargetTemperature, precision: 5);
                Assert.Equal(int.MaxValue, phase.DurationSeconds);
                Assert.Equal(cancellationSource.Token, phase.CancellationToken);
            });
    }

    private sealed class RecordingTemperatureController : TemperatureController
    {
        private readonly Queue<double> _phaseResults;

        public RecordingTemperatureController(
            IFanService fanService,
            IHeaterService heaterService,
            ISensorService sensorService,
            IEnumerable<double> phaseResults)
            : base(fanService, heaterService, sensorService)
        {
            _phaseResults = new Queue<double>(phaseResults);
        }

        public List<PhaseCall> PhaseCalls { get; } = [];

        public override Task<double> RunPhaseAsync(
            ITemperatureControlStrategy strategy,
            double currentTemperature,
            double targetTemperature,
            int durationSeconds,
            CancellationToken cancellationToken = default)
        {
            PhaseCalls.Add(new PhaseCall(
                strategy.GetType(),
                currentTemperature,
                targetTemperature,
                durationSeconds,
                cancellationToken));

            return Task.FromResult(_phaseResults.Dequeue());
        }
    }

    private sealed record PhaseCall(
        Type StrategyType,
        double CurrentTemperature,
        double TargetTemperature,
        int DurationSeconds,
        CancellationToken CancellationToken);
}
