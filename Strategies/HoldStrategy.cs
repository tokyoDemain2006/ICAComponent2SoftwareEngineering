using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Strategies;

/// <summary>
/// Maintains the environment temperature near a target by applying small corrective changes and
/// polling the average temperature once per second.
/// </summary>
public class HoldStrategy : ITemperatureControlStrategy
{
    private const int MinimalHeatingLevel = 1;

    private const double TemperatureTolerance = 0.1;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    private readonly IHeaterService _heaterService;

    private readonly IFanService _fanService;

    private readonly ISensorService _sensorService;

    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;

    /// <summary>
    /// Initialises a new instance of <see cref="HoldStrategy"/>.
    /// </summary>
    public HoldStrategy(
        IHeaterService heaterService,
        IFanService fanService,
        ISensorService sensorService,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        _heaterService = heaterService ?? throw new ArgumentNullException(nameof(heaterService));
        _fanService = fanService ?? throw new ArgumentNullException(nameof(fanService));
        _sensorService = sensorService ?? throw new ArgumentNullException(nameof(sensorService));
        _delayAsync = delayAsync ?? Task.Delay;
    }

    public async Task<double> ExecuteAsync(
        double currentTemperature,
        double targetTemperature,
        int durationSeconds,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(durationSeconds);

        for (int iteration = 0; iteration < durationSeconds; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (targetTemperature - currentTemperature > TemperatureTolerance)
            {
                await _heaterService.SetAllHeatersAsync(MinimalHeatingLevel);
                await _fanService.SetAllFansAsync(false);
            }
            else if (currentTemperature - targetTemperature > TemperatureTolerance)
            {
                await _heaterService.SetAllHeatersAsync(0);
                await _fanService.SetAllFansAsync(true);
            }

            await _delayAsync(PollInterval, cancellationToken);
            currentTemperature = await _sensorService.GetAverageTemperatureAsync();
        }

        return currentTemperature;
    }
}
