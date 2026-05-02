using UglyClient.UI;

namespace UglyClient.Tests.UI;

public class DeviceDisplayFormatterTests
{
    [Fact]
    public void FormatFanState_WhenOn_ReturnsOnString()
    {
        var result = DeviceDisplayFormatter.FormatFanState(1, isOn: true);
        Assert.Equal("  Fan 1: On", result);
    }

    [Fact]
    public void FormatFanState_WhenOff_ReturnsOffString()
    {
        var result = DeviceDisplayFormatter.FormatFanState(2, isOn: false);
        Assert.Equal("  Fan 2: Off", result);
    }

    [Fact]
    public void FormatHeaterLevel_ReturnsExpectedString()
    {
        var result = DeviceDisplayFormatter.FormatHeaterLevel(3, level: 4);
        Assert.Equal("  Heater 3: Level 4", result);
    }

    [Fact]
    public void FormatSensorReading_ReturnsFormattedTemperature()
    {
        var result = DeviceDisplayFormatter.FormatSensorReading(2, temp: 21.567);
        Assert.Equal("  Sensor 2: Temperature 21.6 (Deg)", result);
    }

    [Fact]
    public void FormatMenu_ContainsAllSevenOptions()
    {
        var menu = DeviceDisplayFormatter.FormatMenu();
        Assert.Contains("1. Control Fan", menu);
        Assert.Contains("2. Control Heater", menu);
        Assert.Contains("3. Read Temperature", menu);
        Assert.Contains("4. Display State of All Devices", menu);
        Assert.Contains("5. Control Simulation", menu);
        Assert.Contains("6. Reset Simulation", menu);
        Assert.Contains("7. Exit", menu);
    }

    [Fact]
    public void FormatSensorReading_ZeroDegrees_ReturnsCorrectString()
    {
        var result = DeviceDisplayFormatter.FormatSensorReading(1, temp: 0.0);
        Assert.Equal("  Sensor 1: Temperature 0.0 (Deg)", result);
    }
}
