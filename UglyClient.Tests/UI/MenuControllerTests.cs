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

    [Fact]
    public async Task Option4_DisplayAll_CallsAllDeviceServices()
    {
        var fanMock = new Mock<IFanService>();
        var heaterMock = new Mock<IHeaterService>();
        var sensorMock = new Mock<ISensorService>();

        fanMock.Setup(f => f.GetAllFanStatesAsync())
            .ReturnsAsync(new[] { new FanDTO { Id = 1, IsOn = true }, new FanDTO { Id = 2, IsOn = false } });
        heaterMock.Setup(h => h.GetAllHeaterLevelsAsync())
            .ReturnsAsync(new[] { 2, 3 });
        sensorMock.Setup(s => s.GetTemperatureAsync(It.IsAny<int>())).ReturnsAsync(20.0);

        var output = new StringWriter();
        var controller = BuildController("4\n7\n", output, fanMock: fanMock, heaterMock: heaterMock, sensorMock: sensorMock);

        await controller.RunAsync();

        fanMock.Verify(f => f.GetAllFanStatesAsync(), Times.Once);
        heaterMock.Verify(h => h.GetAllHeaterLevelsAsync(), Times.Once);
        // DefaultConfig has SensorCount=2, so two individual sensor reads
        sensorMock.Verify(s => s.GetTemperatureAsync(It.IsAny<int>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Option5_CallsSimulationRunAsync()
    {
        var simMock = new Mock<ISimulationService>();
        simMock.Setup(s => s.RunAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var output = new StringWriter();
        var controller = BuildController("5\n7\n", output, simMock: simMock);

        await controller.RunAsync();

        simMock.Verify(s => s.RunAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Option6_CallsSimulationResetAsync()
    {
        var simMock = new Mock<ISimulationService>();
        var fanMock = new Mock<IFanService>();
        var heaterMock = new Mock<IHeaterService>();
        var sensorMock = new Mock<ISensorService>();

        simMock.Setup(s => s.ResetAsync()).Returns(Task.CompletedTask);
        fanMock.Setup(f => f.GetAllFanStatesAsync()).ReturnsAsync(Array.Empty<FanDTO>());
        heaterMock.Setup(h => h.GetAllHeaterLevelsAsync()).ReturnsAsync(Array.Empty<int>());

        var output = new StringWriter();
        var controller = BuildController("6\n7\n", output,
            fanMock: fanMock, heaterMock: heaterMock, sensorMock: sensorMock, simMock: simMock);

        await controller.RunAsync();

        simMock.Verify(s => s.ResetAsync(), Times.Once);
    }

    [Fact]
    public async Task Option1_OutOfRangeFanId_DoesNotCallServiceAndWritesError()
    {
        // Input validation must reject device IDs outside 1–FanCount before calling the service
        var fanMock = new Mock<IFanService>(MockBehavior.Strict);

        var output = new StringWriter();
        // DefaultConfig has FanCount=2; ID 99 is out of range
        var controller = BuildController("1\n99\n7\n", output, fanMock: fanMock);

        await controller.RunAsync();

        fanMock.VerifyNoOtherCalls();
        Assert.Contains("Invalid", output.ToString());
    }

    [Fact]
    public async Task Option2_OutOfRangeHeaterLevel_DoesNotCallServiceAndWritesError()
    {
        // Heater levels are bounded 0–5; level 9 must be rejected before calling the service
        var heaterMock = new Mock<IHeaterService>(MockBehavior.Strict);

        var output = new StringWriter();
        // Valid heater ID 1, but level 9 exceeds the maximum of 5
        var controller = BuildController("2\n1\n9\n7\n", output, heaterMock: heaterMock);

        await controller.RunAsync();

        heaterMock.VerifyNoOtherCalls();
        Assert.Contains("Invalid", output.ToString());
    }

    [Fact]
    public async Task Option1_InvalidFanState_DoesNotCallServiceAndWritesError()
    {
        // Gibberish input for on/off must be rejected; the service must never be called.
        var fanMock = new Mock<IFanService>(MockBehavior.Strict);

        var output = new StringWriter();
        // Fan ID 1 is valid; "xyz" is not a valid state
        var controller = BuildController("1\n1\nxyz\n7\n", output, fanMock: fanMock);

        await controller.RunAsync();

        fanMock.VerifyNoOtherCalls();
        Assert.Contains("Invalid", output.ToString());
    }

    [Fact]
    public async Task Option1_OffInput_TurnsOffFan()
    {
        // "off" is a valid state and must call SetFanStateAsync with isOn=false.
        var fanMock = new Mock<IFanService>();
        fanMock.Setup(f => f.SetFanStateAsync(It.IsAny<int>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var output = new StringWriter();
        var controller = BuildController("1\n1\noff\n7\n", output, fanMock: fanMock);

        await controller.RunAsync();

        fanMock.Verify(f => f.SetFanStateAsync(1, false), Times.Once);
    }
}
