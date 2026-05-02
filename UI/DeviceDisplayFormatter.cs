namespace UglyClient.UI;

/// <summary>
/// Provides static methods that format device state and menu text into console-ready strings.
/// All methods return a string; no <see cref="System.Console"/> calls are made inside this class.
/// </summary>
public static class DeviceDisplayFormatter
{
    /// <summary>
    /// Formats the current on/off state of a single fan.
    /// </summary>
    /// <param name="id">The one-based fan identifier.</param>
    /// <param name="isOn"><c>true</c> if the fan is on; <c>false</c> if it is off.</param>
    /// <returns>A human-readable string describing the fan state.</returns>
    public static string FormatFanState(int id, bool isOn)
        => $"  Fan {id}: {(isOn ? "On" : "Off")}";

    /// <summary>
    /// Formats the current heat level of a single heater.
    /// </summary>
    /// <param name="id">The one-based heater identifier.</param>
    /// <param name="level">The current heat level (0–5).</param>
    /// <returns>A human-readable string describing the heater level.</returns>
    public static string FormatHeaterLevel(int id, int level)
        => $"  Heater {id}: Level {level}";

    /// <summary>
    /// Formats a temperature reading from a single sensor.
    /// </summary>
    /// <param name="id">The one-based sensor identifier.</param>
    /// <param name="temp">The temperature value returned by the sensor.</param>
    /// <returns>A human-readable string describing the sensor temperature.</returns>
    public static string FormatSensorReading(int id, double temp)
        => $"  Sensor {id}: Temperature {temp:F1} (Deg)";

    /// <summary>
    /// Returns the full 7-option menu string shown to the user before each prompt.
    /// </summary>
    /// <returns>A multi-line menu string ending without a trailing newline.</returns>
    public static string FormatMenu() =>
        "Simulation Control:\n" +
        "1. Control Fan\n" +
        "2. Control Heater\n" +
        "3. Read Temperature\n" +
        "4. Display State of All Devices\n" +
        "5. Control Simulation\n" +
        "6. Reset Simulation\n" +
        "7. Exit\n" +
        "Select an option: ";
}
