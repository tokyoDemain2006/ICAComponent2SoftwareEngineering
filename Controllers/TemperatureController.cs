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
    /// <summary>
    /// The fan facade used by phase strategies.
    /// </summary>
    private readonly IFanService _fanService;

    /// <summary>
    /// The heater facade used by phase strategies.
    /// </summary>
    private readonly IHeaterService _heaterService;

    /// <summary>
    /// The sensor facade used to read temperatures and construct phase strategies.
    /// </summary>
    private readonly ISensorService _sensorService;

    /// <summary>
    /// Initialises a new instance of <see cref="TemperatureController"/>.
    /// </summary>
    /// <param name="fanService">The fan facade used during each temperature-control phase.</param>
    /// <param name="heaterService">The heater facade used during each temperature-control phase.</param>
    /// <param name="sensorService">The sensor facade used to read the current average temperature.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fanService"/>, <paramref name="heaterService"/>, or
    /// <paramref name="sensorService"/> is <see langword="null"/>.
    /// </exception>
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
    /// <param name="strategy">The phase strategy to execute.</param>
    /// <param name="currentTemperature">The average temperature observed at the start of the phase.</param>
    /// <param name="targetTemperature">The target temperature for the phase.</param>
    /// <param name="durationSeconds">The maximum number of one-second iterations the phase may run.</param>
    /// <param name="cancellationToken">The token used to cancel the phase cleanly.</param>
    /// <returns>The last temperature returned by the strategy.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="durationSeconds"/> is negative.</exception>
    public virtual async Task<double> RunPhaseAsync(
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
    /// <param name="cancellationToken">The token used to cancel the cycle cleanly.</param>
    /// <returns>A <see cref="Task"/> that completes when the cycle finishes or is cancelled.</returns>
    public virtual async Task RunFullCycleAsync(CancellationToken cancellationToken = default)
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
