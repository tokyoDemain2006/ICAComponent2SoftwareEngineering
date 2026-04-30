using System;
using System.Threading.Tasks;
using UglyClient.Adapters;
using UglyClient.Config;
using UglyClient.Controllers;
using UglyClient.Interfaces;
using UglyClient.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new SimulationConfig(
            FanCount: 3,
            HeaterCount: 3,
            SensorCount: 3,
            BaseUrl: "https://localhost:44351/",
            ApiKey: "u007-key"
        );

        using var httpService = new HttpService(config.BaseUrl, config.ApiKey);

        // Adapter Pattern: each sensor adapter wraps its own HTTP call and converts
        // the raw response type to double, conforming to ISensor.
        ISensor sensor1Adapter = new Sensor1Adapter(httpService);
        ISensor sensor2Adapter = new Sensor2Adapter(httpService);
        ISensor sensor3Adapter = new Sensor3Adapter(httpService);
        ISensor[] sensorAdapters = [sensor1Adapter, sensor2Adapter, sensor3Adapter];
        ISensorService sensorService = new SensorService(sensorAdapters);

        // Facade / Service layer: FanService encapsulates all fan-related HTTP calls.
        IFanService fanService = new FanService(httpService, config.FanCount);

        // Facade / Service layer: HeaterService encapsulates all heater-related HTTP calls.
        IHeaterService heaterService = new HeaterService(httpService, config.HeaterCount);
        ISimulationService simulationService = new SimulationService(httpService);
        var temperatureController = new TemperatureController(fanService, heaterService, sensorService);

        while (true)
        {
            Console.WriteLine("Simulation Control:");
            Console.WriteLine("1. Control Fan");
            Console.WriteLine("2. Control Heater");
            Console.WriteLine("3. Read Temperature");
            Console.WriteLine("4. Display State of All Devices");
            Console.WriteLine("5. Control Simulation");
            Console.WriteLine("6. Reset Simulation");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            switch (input)
            {
                case "1":

                    Console.Write("Enter Fan Number: ");
                    if (int.TryParse(Console.ReadLine(), out int fanId) && IsValidDeviceId(fanId, config.FanCount))
                    {
                        Console.Write("Turn Fan On or Off? (on/off): ");
                        var stateInput = Console.ReadLine();
                        bool isOn = stateInput?.ToLower() == "on";

                        try
                        {
                            await fanService.SetFanStateAsync(fanId, isOn);
                            Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.");
                        }
                        catch (DeviceServiceException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Fan Number. Please enter a value between 1 and {config.FanCount}.");
                    }
                    break;
                case "2":

                    Console.Write("Enter Heater Number: ");
                    if (int.TryParse(Console.ReadLine(), out int heaterId) && IsValidDeviceId(heaterId, config.HeaterCount))
                    {
                        Console.Write("Set Heater Level (0-5): ");
                        if (int.TryParse(Console.ReadLine(), out int level) && level >= 0 && level <= 5)
                        {
                            try
                            {
                                await heaterService.SetHeaterLevelAsync(heaterId, level);
                                Console.WriteLine($"Heater {heaterId} level set to {level}.");
                            }
                            catch (DeviceServiceException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Heater Level. Please enter a value between 0 and 5.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Heater Number. Please enter a value between 1 and {config.HeaterCount}.");
                    }
                    break;
                case "3":

                    Console.Write("Enter Sensor Number: ");
                    if (int.TryParse(Console.ReadLine(), out int sensorId) && IsValidDeviceId(sensorId, config.SensorCount))
                    {
                        try
                        {
                            double temperature = await sensorService.GetTemperatureAsync(sensorId);
                            Console.WriteLine($"Sensor {sensorId} Temperature: {temperature:F1}°C");
                        }
                        catch (DeviceServiceException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Sensor Number. Please enter a value between 1 and {config.SensorCount}.");
                    }
                    break;
                case "4":
                    Console.WriteLine("Fetching the state of all devices...");

                    try
                    {
                        await DisplayAllDeviceStatesAsync(fanService, heaterService, sensorService, config.SensorCount);
                    }
                    catch (DeviceServiceException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "5":
                    try
                    {
                        await RunTemperatureControlAsync(temperatureController);
                    }
                    catch (DeviceServiceException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "6":
                    Console.WriteLine("Resetting client state...");

                    try
                    {
                        await simulationService.ResetAsync();
                        Console.WriteLine("Client state has been successfully reset.");
                        Console.WriteLine("Fetching the state of all devices...");
                        await DisplayAllDeviceStatesAsync(fanService, heaterService, sensorService, config.SensorCount);
                    }
                    catch (DeviceServiceException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

            Console.WriteLine(); // Add a blank line for better readability
        }
    }

    /// <summary>
    /// Runs the temperature-control cycle until the user requests cancellation.
    /// </summary>
    /// <param name="temperatureController">The controller that orchestrates the strategy sequence.</param>
    static async Task RunTemperatureControlAsync(TemperatureController temperatureController)
    {
        ArgumentNullException.ThrowIfNull(temperatureController);

        Console.WriteLine("Starting temperature control algorithm...");

        if (Console.IsInputRedirected)
        {
            Console.WriteLine("Temperature control requires interactive console input for clean cancellation.");
            return;
        }

        using var cancellationSource = new CancellationTokenSource();
        Console.WriteLine("Press Enter to stop temperature control.");

        var controlTask = temperatureController.RunFullCycleAsync(cancellationSource.Token);

        while (!controlTask.IsCompleted)
        {
            if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Enter)
            {
                cancellationSource.Cancel();
                break;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        try
        {
            await controlTask;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Temperature control stopped.");
        }
    }

    /// <summary>
    /// Writes the current state of all fans, heaters, and sensors to the console.
    /// </summary>
    /// <param name="fanService">The fan facade used to retrieve fan states.</param>
    /// <param name="heaterService">The heater facade used to retrieve heater levels.</param>
    /// <param name="sensorService">The sensor facade used to retrieve temperatures.</param>
    /// <param name="sensorCount">The total number of configured sensors to display.</param>
    static async Task DisplayAllDeviceStatesAsync(IFanService fanService, IHeaterService heaterService, ISensorService sensorService, int sensorCount)
    {
        await DisplayFanStatesAsync(fanService);
        await DisplayHeaterLevelsAsync(heaterService);
        await DisplaySensorTemperaturesAsync(sensorService, sensorCount);
    }

    /// <summary>
    /// Writes the current state of every configured fan to the console.
    /// </summary>
    /// <param name="fanService">The fan facade used to retrieve fan states.</param>
    static async Task DisplayFanStatesAsync(IFanService fanService)
    {
        Console.WriteLine("Fetching fan states individually...");

        foreach (var fan in await fanService.GetAllFanStatesAsync())
        {
            Console.WriteLine($"  Fan {fan.Id}: {(fan.IsOn ? "On" : "Off")}");
        }
    }

    /// <summary>
    /// Writes the current level of every configured heater to the console.
    /// </summary>
    /// <param name="heaterService">The heater facade used to retrieve heater levels.</param>
    static async Task DisplayHeaterLevelsAsync(IHeaterService heaterService)
    {
        Console.WriteLine("Fetching heater levels individually...");

        int heaterId = 1;
        foreach (var heaterLevel in await heaterService.GetAllHeaterLevelsAsync())
        {
            Console.WriteLine($"  Heater {heaterId++}: Level {heaterLevel}");
        }
    }

    /// <summary>
    /// Writes the current temperature for every configured sensor to the console.
    /// </summary>
    /// <param name="sensorService">The sensor facade used to retrieve temperatures.</param>
    /// <param name="sensorCount">The total number of configured sensors to display.</param>
    static async Task DisplaySensorTemperaturesAsync(ISensorService sensorService, int sensorCount)
    {
        Console.WriteLine("Fetching sensor temperatures individually...");

        for (int sensorId = 1; sensorId <= sensorCount; sensorId++)
        {
            double temperature = await sensorService.GetTemperatureAsync(sensorId);
            Console.WriteLine($"  Sensor {sensorId}: Temperature {temperature:F1} (Deg)");
        }
    }

    /// <summary>
    /// Determines whether a one-based device identifier is within the configured range.
    /// </summary>
    /// <param name="deviceId">The one-based device identifier to validate.</param>
    /// <param name="deviceCount">The total number of configured devices of that type.</param>
    /// <returns><see langword="true"/> when the identifier is valid; otherwise <see langword="false"/>.</returns>
    static bool IsValidDeviceId(int deviceId, int deviceCount)
    {
        return deviceId >= 1 && deviceId <= deviceCount;
    }


}
