namespace UglyClient.Interfaces;

/// <summary>Contract for a temperature sensor adapter.</summary>
public interface ISensor
{
    /// <summary>One-based sensor identifier.</summary>
    int SensorId { get; }

    /// <summary>Returns the current sensor temperature in degrees Celsius.</summary>
    /// <exception cref="UglyClient.Services.DeviceServiceException">
    /// Thrown when the reading cannot be retrieved.
    /// </exception>
    Task<double> GetTemperatureAsync();
}
