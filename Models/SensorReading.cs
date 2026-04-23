namespace UglyClient.Models;

/// <summary>
/// Represents a normalised temperature reading from any sensor device.
/// The API may return temperatures as <c>string</c>, <c>int</c>, or <c>decimal</c> depending on
/// the sensor; <see cref="Temperature"/> is always stored as <c>double</c> after normalisation
/// by <c>SensorService</c>.
/// </summary>
public class SensorReading
{
    /// <summary>
    /// Gets or sets the unique identifier of the sensor that produced this reading.
    /// </summary>
    public int SensorId { get; set; }

    /// <summary>
    /// Gets or sets the temperature value recorded by the sensor, expressed in degrees Celsius
    /// and normalised to <c>double</c> regardless of the raw API response type.
    /// </summary>
    public double Temperature { get; set; }
}
