namespace UglyClient.UI;

/// <summary>
/// Provides static methods that format device state and menu text into console-ready strings.
/// All methods return a string; no <see cref="System.Console"/> calls are made inside this class.
/// </summary>
public static class DeviceDisplayFormatter
{

    public static string FormatFanState(int id, bool isOn)
        => $"  Fan {id}: {(isOn ? "On" : "Off")}";


    public static string FormatHeaterLevel(int id, int level)
        => $"  Heater {id}: Level {level}";

    public static string FormatSensorReading(int id, double temp)
        => $"  Sensor {id}: Temperature {temp:F1} (Deg)";

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
