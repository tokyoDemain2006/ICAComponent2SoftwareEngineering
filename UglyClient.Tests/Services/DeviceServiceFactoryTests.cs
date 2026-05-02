using UglyClient.Config;
using UglyClient.Controllers;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="DeviceServiceFactory"/>.
/// </summary>
public class DeviceServiceFactoryTests
{
    private static SimulationConfig ValidConfig => new(
        FanCount: 3,
        HeaterCount: 3,
        SensorCount: 3,
        BaseUrl: "http://localhost/",
        ApiKey: "test-key");

    [Fact]
    public void Create_NullConfig_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DeviceServiceFactory.Create(null!));
    }

    [Fact]
    public void Create_ValidConfig_ReturnsFanService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.NotNull(result.FanService);
    }

    [Fact]
    public void Create_ValidConfig_ReturnsHeaterService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.NotNull(result.HeaterService);
    }

    [Fact]
    public void Create_ValidConfig_ReturnsSensorService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.NotNull(result.SensorService);
    }

    [Fact]
    public void Create_ValidConfig_ReturnsSimulationService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.NotNull(result.SimulationService);
    }

    [Fact]
    public void Create_ValidConfig_ReturnsTemperatureController()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.NotNull(result.TemperatureController);
    }

    [Fact]
    public void Create_ValidConfig_FanServiceImplementsIFanService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.IsAssignableFrom<IFanService>(result.FanService);
    }

    [Fact]
    public void Create_ValidConfig_HeaterServiceImplementsIHeaterService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.IsAssignableFrom<IHeaterService>(result.HeaterService);
    }

    [Fact]
    public void Create_ValidConfig_SensorServiceImplementsISensorService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.IsAssignableFrom<ISensorService>(result.SensorService);
    }

    [Fact]
    public void Create_ValidConfig_SimulationServiceImplementsISimulationService()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.IsAssignableFrom<ISimulationService>(result.SimulationService);
    }

    [Fact]
    public void Create_ValidConfig_TemperatureControllerIsCorrectType()
    {
        var result = DeviceServiceFactory.Create(ValidConfig);

        Assert.IsType<TemperatureController>(result.TemperatureController);
    }
}
