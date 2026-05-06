using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Strategies;

/// <summary>
/// Raises the environment temperature by enabling heaters and disabling fans until the target is reached
/// or the allotted duration expires.
/// </summary>
public class HeatUpStrategy : ITemperatureControlStrategy
{
    private const int HeatingLevel = 3;

    private const double TemperatureTolerance = 0.1;

    private readonly IHeaterService _heaterService;

    private readonly IFanService _fanService;

    private readonly ISensorService _sensorService;

    /// <summary>
    /// Initialises a new instance of <see cref="HeatUpStrategy"/>.
    /// </summary>
    public HeatUpStrategy(
        IHeaterService heaterService,
        IFanService fanService,
        ISensorService sensorService)
    {
        _heaterService = heaterService ?? throw new ArgumentNullException(nameof(heaterService));
        _fanService = fanService ?? throw new ArgumentNullException(nameof(fanService));
        _sensorService = sensorService ?? throw new ArgumentNullException(nameof(sensorService));
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

            if (HasReachedTarget(currentTemperature, targetTemperature))
            {
                break;
            }

            await _heaterService.SetAllHeatersAsync(HeatingLevel);
            await _fanService.SetAllFansAsync(false);
            await Task.Delay(1000, cancellationToken);
            currentTemperature = await _sensorService.GetAverageTemperatureAsync();
        }

        return currentTemperature;
    }

    private static bool HasReachedTarget(double currentTemperature, double targetTemperature)
    {
        return currentTemperature >= targetTemperature ||
               Math.Abs(currentTemperature - targetTemperature) <= TemperatureTolerance;
    }
}
