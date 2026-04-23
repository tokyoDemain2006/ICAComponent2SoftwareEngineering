namespace UglyClient.Services;

/// <summary>
/// Defines the facade contract for sensor-related temperature retrieval.
/// Implementations hide adapter selection and aggregation details behind a small
/// domain-focused API that always returns temperatures as <see cref="double"/>.
/// </summary>
public interface ISensorService
{
    /// <summary>
    /// Retrieves the current temperature from the sensor identified by <paramref name="sensorId"/>.
    /// </summary>
    /// <param name="sensorId">The one-based identifier of the sensor to query.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to the current sensor temperature as a
    /// normalised <see cref="double"/>.
    /// </returns>
    Task<double> GetTemperatureAsync(int sensorId);

    /// <summary>
    /// Retrieves the current temperature from every managed sensor and returns the arithmetic mean.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to the average temperature across all sensors
    /// as a normalised <see cref="double"/>.
    /// </returns>
    Task<double> GetAverageTemperatureAsync();
}
