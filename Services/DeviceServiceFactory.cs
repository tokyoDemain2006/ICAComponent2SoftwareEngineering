using UglyClient.Adapters;
using UglyClient.Config;
using UglyClient.Controllers;
using UglyClient.Interfaces;

namespace UglyClient.Services;

/// <summary>
/// Holds the fully-wired service instances produced by <see cref="DeviceServiceFactory.Create"/>.
/// All properties expose interfaces so callers never depend on concrete service types.
/// </summary>
public record DeviceServices(
    IFanService FanService,
    IHeaterService HeaterService,
    ISensorService SensorService,
    ISimulationService SimulationService,
    TemperatureController TemperatureController);

/// <summary>
/// Centralises the construction of all concrete service types so that <c>Program.cs</c>
/// never directly instantiates a service class.
/// </summary>
public static class DeviceServiceFactory
{
    /// <summary>
    /// Builds and wires every service required by the simulation client from the supplied config.
    /// </summary>
    public static DeviceServices Create(SimulationConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var httpService = new HttpService(config.BaseUrl, config.ApiKey);

        ISensor[] sensorAdapters =
        [
            new Sensor1Adapter(httpService),
            new Sensor2Adapter(httpService),
            new Sensor3Adapter(httpService),
        ];

        ISensorService sensorService = new SensorService(sensorAdapters);
        IFanService fanService = new FanService(httpService, config.FanCount);
        IHeaterService heaterService = new HeaterService(httpService, config.HeaterCount);
        var temperatureController = new TemperatureController(fanService, heaterService, sensorService);
        ISimulationService simulationService = new SimulationService(httpService, temperatureController);

        return new DeviceServices(fanService, heaterService, sensorService, simulationService, temperatureController);
    }
}
