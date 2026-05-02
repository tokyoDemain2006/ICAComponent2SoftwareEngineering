using Moq;
using UglyClient.Config;
using UglyClient.Services;
using UglyClient.Models;
using UglyClient.UI;

namespace UglyClient.Tests.UI;

public class MenuControllerTests
{
    private static SimulationConfig DefaultConfig => new(FanCount: 2, HeaterCount: 2, SensorCount: 2, BaseUrl: "http://localhost/", ApiKey: "test");

    private static MenuController BuildController(
        string inputLines,
        StringWriter output,
        Mock<IFanService>? fanMock = null,
        Mock<IHeaterService>? heaterMock = null,
        Mock<ISensorService>? sensorMock = null,
        Mock<ISimulationService>? simMock = null,
        SimulationConfig? config = null)
    {
        return new MenuController(
            fanMock?.Object ?? new Mock<IFanService>().Object,
            heaterMock?.Object ?? new Mock<IHeaterService>().Object,
            sensorMock?.Object ?? new Mock<ISensorService>().Object,
            simMock?.Object ?? new Mock<ISimulationService>().Object,
            config ?? DefaultConfig,
            new StringReader(inputLines),
            output);
    }

    [Fact]
    public async Task Option1_CallsSetFanStateAsync()
    {
        var fanMock = new Mock<IFanService>();
        fanMock.Setup(f => f.SetFanStateAsync(It.IsAny<int>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var output = new StringWriter();
        // Input: choose 1, fan id 1, "on", then 7 to exit
        var controller = BuildController("1\n1\non\n7\n", output, fanMock: fanMock);

        await controller.RunAsync();

        fanMock.Verify(f => f.SetFanStateAsync(1, true), Times.Once);
    }

    [Fact]
    public async Task Option2_CallsSetHeaterLevelAsync()
    {
        var heaterMock = new Mock<IHeaterService>();
        heaterMock.Setup(h => h.SetHeaterLevelAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);

        var output = new StringWriter();
        // Input: choose 2, heater id 1, level 3, then 7 to exit
        var controller = BuildController("2\n1\n3\n7\n", output, heaterMock: heaterMock);

        await controller.RunAsync();

        heaterMock.Verify(h => h.SetHeaterLevelAsync(1, 3), Times.Once);
    }

    [Fact]
    public async Task Option3_CallsGetTemperatureAsync()
    {
        var sensorMock = new Mock<ISensorService>();
        sensorMock.Setup(s => s.GetTemperatureAsync(1)).ReturnsAsync(22.5);

        var output = new StringWriter();
        // Input: choose 3, sensor id 1, then 7 to exit
        var controller = BuildController("3\n1\n7\n", output, sensorMock: sensorMock);

        await controller.RunAsync();

        sensorMock.Verify(s => s.GetTemperatureAsync(1), Times.Once);
    }

    [Fact]
    public async Task Option7_ExitsLoopCleanly()
    {
        var output = new StringWriter();
        var controller = BuildController("7\n", output);

        // Should complete without hanging or throwing
        await controller.RunAsync();

        Assert.Contains("Exiting", output.ToString());
    }

    [Fact]
    public async Task InvalidInput_DoesNotCrash_WritesErrorMessage()
    {
        var output = new StringWriter();
        // Send invalid option, then exit
        var controller = BuildController("xyz\n7\n", output);

        await controller.RunAsync();

        Assert.Contains("Invalid option", output.ToString());
    }

    [Fact]
    public async Task Option1_DeviceServiceException_IsWrittenToOutput()
    {
        var fanMock = new Mock<IFanService>();
        fanMock.Setup(f => f.SetFanStateAsync(It.IsAny<int>(), It.IsAny<bool>()))
               .ThrowsAsync(new DeviceServiceException("Fan API unreachable"));

        var output = new StringWriter();
        // Input: choose 1, fan id 1, "on", then 7 to exit
        var controller = BuildController("1\n1\non\n7\n", output, fanMock: fanMock);

        await controller.RunAsync();

        Assert.Contains("Fan API unreachable", output.ToString());
    }
}
