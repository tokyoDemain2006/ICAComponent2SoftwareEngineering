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
    /// Asynchronously retrieves the current temperature reading from the sensor.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to the sensor temperature as a
    /// <see cref="double"/> (degrees Celsius).
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the HTTP request fails or the response cannot be parsed into a
    /// valid temperature value.
    /// </exception>
    Task<double> GetTemperatureAsync();
}
