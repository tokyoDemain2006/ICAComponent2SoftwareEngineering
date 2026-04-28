using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Strategies;

/// <summary>
/// Raises the environment temperature by enabling heaters and disabling fans until the target is reached
/// or the allotted duration expires.
/// </summary>
public class HeatUpStrategy : ITemperatureControlStrategy
{
    /// <summary>
    /// The heater level applied while heating the environment.
    /// </summary>
    private const int HeatingLevel = 3;

    /// <summary>
    /// The tolerance used to stop once the target temperature has effectively been reached.
    /// </summary>
    private const double TemperatureTolerance = 0.1;

    /// <summary>
    /// The delay between successive temperature polls.
    /// </summary>
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The heater facade used to apply heat.
    /// </summary>
    private readonly IHeaterService _heaterService;

    /// <summary>
    /// The fan facade used to reduce cooling while heating.
    /// </summary>
    private readonly IFanService _fanService;

    /// <summary>
    /// The sensor facade used to poll the current average temperature.
    /// </summary>
    private readonly ISensorService _sensorService;

    /// <summary>
    /// The delay operation used between polls.
    /// </summary>
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;

    /// <summary>
    /// Initialises a new instance of <see cref="HeatUpStrategy"/>.
    /// </summary>
    /// <param name="heaterService">The heater facade used to control all heaters.</param>
    /// <param name="fanService">The fan facade used to control all fans.</param>
    /// <param name="sensorService">The sensor facade used to read temperatures.</param>
    /// <param name="delayAsync">The delay operation used between temperature polls.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="heaterService"/>, <paramref name="fanService"/>, or
    /// <paramref name="sensorService"/> is <see langword="null"/>.
    /// </exception>
    public HeatUpStrategy(
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

    /// <inheritdoc />
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

            if (HasReachedTarget(currentTemperature, targetTemperature))
            {
                break;
            }

            await _heaterService.SetAllHeatersAsync(HeatingLevel);
            await _fanService.SetAllFansAsync(false);
            await _delayAsync(PollInterval, cancellationToken);
            currentTemperature = await _sensorService.GetAverageTemperatureAsync();
        }

        return currentTemperature;
    }

    /// <summary>
    /// Determines whether the heat-up phase can stop because the target has been reached.
    /// </summary>
    /// <param name="currentTemperature">The latest observed temperature.</param>
    /// <param name="targetTemperature">The target temperature for the phase.</param>
    /// <returns>
    /// <see langword="true"/> when the temperature is within tolerance of the target or already above it;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private static bool HasReachedTarget(double currentTemperature, double targetTemperature)
    {
        return currentTemperature >= targetTemperature ||
               Math.Abs(currentTemperature - targetTemperature) <= TemperatureTolerance;
    }
}
