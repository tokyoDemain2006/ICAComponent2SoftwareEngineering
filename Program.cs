using UglyClient.Config;
using UglyClient.Services;
using UglyClient.UI;

var config = new SimulationConfig(
    FanCount: 3,
    HeaterCount: 3,
    SensorCount: 3,
    BaseUrl: "https://localhost:44351/",
    ApiKey: "u007-key"
);

var services = DeviceServiceFactory.Create(config);

var menuController = new MenuController(
    services.FanService,
    services.HeaterService,
    services.SensorService,
    services.SimulationService,
    config);

await menuController.RunAsync();
