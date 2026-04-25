namespace UglyClient.Interfaces;

/// <summary>
/// Defines the contract for a temperature sensor.
/// Implementing this interface as part of the Adapter Pattern ensures that all
/// sensor adapters — regardless of their underlying HTTP response format — expose
/// a unified <see cref="GetTemperatureAsync"/> method that returns a <see cref="double"/>.
/// </summary>
public interface ISensor
{
    /// <summary>
    /// Gets the one-based identifier of the sensor represented by this adapter.
    /// </summary>
    int SensorId { get; }

    /// <summary>
    /// Asynchronously retrieves the current temperature reading from the sensor.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to the sensor temperature as a
    /// <see cref="double"/> (degrees Celsius).
    /// </returns>
    /// <exception cref="UglyClient.Services.DeviceServiceException">
    /// Thrown when the sensor reading cannot be retrieved successfully.
    /// </exception>
    Task<double> GetTemperatureAsync();
}
