namespace UglyClient.Services;

/// <summary>Contract for sensor temperature retrieval.</summary>
public interface ISensorService
{
    /// <summary>Retrieves the current temperature from a single sensor.</summary>
    /// <param name="sensorId">One-based sensor identifier.</param>
    /// <exception cref="DeviceServiceException">Thrown when the reading cannot be retrieved.</exception>
    Task<double> GetTemperatureAsync(int sensorId);

    /// <summary>Retrieves the arithmetic mean of all managed sensor readings.</summary>
    /// <exception cref="DeviceServiceException">Thrown when one or more readings cannot be retrieved.</exception>
    Task<double> GetAverageTemperatureAsync();
}
