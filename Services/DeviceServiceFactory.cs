using UglyClient.Adapters;
using UglyClient.Config;
using UglyClient.Controllers;
using UglyClient.Interfaces;

namespace UglyClient.Services;

/// <summary>
/// Holds the fully-wired service instances produced by <see cref="DeviceServiceFactory.Create"/>.
/// All properties expose interfaces so callers never depend on concrete service types.
/// </summary>
/// <param name="FanService">The fan control service.</param>
/// <param name="HeaterService">The heater control service.</param>
/// <param name="SensorService">The sensor read service.</param>
/// <param name="SimulationService">The simulation-lifecycle service.</param>
/// <param name="TemperatureController">The orchestrator that runs the multi-phase temperature cycle.</param>
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
    /// <param name="config">The simulation configuration. Must not be <see langword="null"/>.</param>
    /// <returns>A <see cref="DeviceServices"/> record whose properties are all interface-typed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
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
