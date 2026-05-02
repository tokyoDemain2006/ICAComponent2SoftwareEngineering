using UglyClient.Interfaces;

namespace UglyClient.Services;

/// <summary>Facade implementation of <see cref="ISensorService"/> that delegates to injected sensor adapters.</summary>
public class SensorService : ISensorService
{
    private readonly IReadOnlyDictionary<int, ISensor> _sensorsById;

    /// <summary>Initialises a new instance of <see cref="SensorService"/>.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sensors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the sequence is empty, contains a <see langword="null"/> adapter,
    /// or contains duplicate sensor identifiers.
    /// </exception>
    public SensorService(IEnumerable<ISensor> sensors)
    {
        ArgumentNullException.ThrowIfNull(sensors);

        var sensorsById = new Dictionary<int, ISensor>();

        foreach (var sensor in sensors)
        {
            if (sensor is null)
            {
                throw new ArgumentException("Sensor collection must not contain null adapters.", nameof(sensors));
            }

            if (!sensorsById.TryAdd(sensor.SensorId, sensor))
            {
                throw new ArgumentException(
                    $"Duplicate sensor ID detected: {sensor.SensorId}.",
                    nameof(sensors));
            }
        }

        if (sensorsById.Count == 0)
        {
            throw new ArgumentException("Sensor collection must contain at least one adapter.", nameof(sensors));
        }

        _sensorsById = sensorsById;
    }

    /// <inheritdoc />
    public Task<double> GetTemperatureAsync(int sensorId)
    {
        if (!_sensorsById.TryGetValue(sensorId, out var sensor))
        {
            throw new DeviceServiceException($"Sensor {sensorId} is not available.");
        }

        return sensor.GetTemperatureAsync();
    }

    /// <inheritdoc />
    public async Task<double> GetAverageTemperatureAsync()
    {
        double total = 0;
        int count = 0;

        foreach (var sensor in _sensorsById.Values)
        {
            total += await sensor.GetTemperatureAsync();
            count++;
        }

        return total / count;
    }
}
