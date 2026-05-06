using UglyClient.Interfaces;
using UglyClient.Services;
using UglyClient.Strategies;

namespace UglyClient.Controllers;

/// <summary>
/// Orchestrates the end-to-end temperature-control cycle by selecting and executing the
/// appropriate strategy for each phase.
/// </summary>
public class TemperatureController
{
    private readonly IFanService _fanService;

    private readonly IHeaterService _heaterService;

    private readonly ISensorService _sensorService;

    /// <summary>
    /// Initialises a new instance of <see cref="TemperatureController"/>.
    /// </summary>
    public TemperatureController(
        IFanService fanService,
        IHeaterService heaterService,
        ISensorService sensorService)
    {
        _fanService = fanService ?? throw new ArgumentNullException(nameof(fanService));
        _heaterService = heaterService ?? throw new ArgumentNullException(nameof(heaterService));
        _sensorService = sensorService ?? throw new ArgumentNullException(nameof(sensorService));
    }

    /// <summary>
    /// Runs a single control phase by delegating to the supplied strategy.
    /// </summary>
    public async Task<double> RunPhaseAsync(
        ITemperatureControlStrategy strategy,
        double currentTemperature,
        double targetTemperature,
        int durationSeconds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentOutOfRangeException.ThrowIfNegative(durationSeconds);
        cancellationToken.ThrowIfCancellationRequested();

        return await strategy.ExecuteAsync(
            currentTemperature,
            targetTemperature,
            durationSeconds,
            cancellationToken);
    }

    /// <summary>
    /// Runs the fixed multi-phase simulation cycle until the final hold phase is cancelled.
    /// </summary>
    public async Task RunFullCycleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        double currentTemperature = await _sensorService.GetAverageTemperatureAsync();
        cancellationToken.ThrowIfCancellationRequested();

        currentTemperature = await RunPhaseAsync(
            new HeatUpStrategy(_heaterService, _fanService, _sensorService),
            currentTemperature,
            20.0,
            30,
            cancellationToken);

        currentTemperature = await RunPhaseAsync(
            new CoolDownStrategy(_heaterService, _fanService, _sensorService),
            currentTemperature,
            16.0,
            10,
            cancellationToken);

        currentTemperature = await RunPhaseAsync(
            new HoldStrategy(_heaterService, _fanService, _sensorService),
            currentTemperature,
            16.0,
            10,
            cancellationToken);

        currentTemperature = await RunPhaseAsync(
            new HeatUpStrategy(_heaterService, _fanService, _sensorService),
            currentTemperature,
            18.0,
            20,
            cancellationToken);

        await RunPhaseAsync(
            new HoldStrategy(_heaterService, _fanService, _sensorService),
            currentTemperature,
            18.0,
            int.MaxValue,
            cancellationToken);
    }
}
