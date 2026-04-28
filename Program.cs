using System;
using System.Net.Http;
using System.Threading.Tasks;
using UglyClient.Adapters;
using UglyClient.Config;
using UglyClient.Interfaces;
using UglyClient.Services;
using UglyClient.Strategies;

class Program
{
    /// <summary>
    /// The tolerance used to treat two temperature readings as effectively equal.
    /// </summary>
    private const double TemperatureTolerance = 0.1;

    static async Task Main(string[] args)
    {
        var config = new SimulationConfig(
            FanCount: 3,
            HeaterCount: 3,
            SensorCount: 3,
            BaseUrl: "https://localhost:44351/",
            ApiKey: "u007-key"
        );

        // https://envrosym.azurewebsites.net/
        var client = new HttpClient { BaseAddress = new Uri(config.BaseUrl) };
        // var client = new HttpClient { BaseAddress = new Uri("https://envrosym.azurewebsites.net/") };

        // Must match the API key in ApiKeyManager (dictionary key)
        // MUST match a DICTIONARY KEY on the server:
        client.DefaultRequestHeaders.Add("X-Api-Key", config.ApiKey);

        // Adapter Pattern: each sensor adapter wraps its own HTTP call and converts
        // the raw response type to double, conforming to ISensor.
        ISensor sensor1Adapter = new Sensor1Adapter(client);
        ISensor sensor2Adapter = new Sensor2Adapter(client);
        ISensor sensor3Adapter = new Sensor3Adapter(client);
        ISensor[] sensorAdapters = [sensor1Adapter, sensor2Adapter, sensor3Adapter];
        ISensorService sensorService = new SensorService(sensorAdapters);

        // Facade / Service layer: FanService encapsulates all fan-related HTTP calls.
        IFanService fanService = new FanService(client, config.FanCount);

        // Facade / Service layer: HeaterService encapsulates all heater-related HTTP calls.
        IHeaterService heaterService = new HeaterService(client, config.HeaterCount);
        ITemperatureControlStrategy heatUpStrategy = new HeatUpStrategy(heaterService, fanService, sensorService);
        ITemperatureControlStrategy coolDownStrategy = new CoolDownStrategy(heaterService, fanService, sensorService);
        ITemperatureControlStrategy holdStrategy = new HoldStrategy(heaterService, fanService, sensorService);

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
                    if (int.TryParse(Console.ReadLine(), out int fanId))
                    {
                        Console.Write("Turn Fan On or Off? (on/off): ");
                        var stateInput = Console.ReadLine();
                        bool isOn = stateInput?.ToLower() == "on";

                        try
                        {
                            await fanService.SetFanStateAsync(fanId, isOn);
                            Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid Fan Number.");
                    }
                    break;
                case "2":

                    Console.Write("Enter Heater Number: ");
                    if (int.TryParse(Console.ReadLine(), out int heaterId))
                    {
                        Console.Write("Set Heater Level (0-5): ");
                        if (int.TryParse(Console.ReadLine(), out int level) && level >= 0 && level <= 5)
                        {
                            try
                            {
                                await heaterService.SetHeaterLevelAsync(heaterId, level);
                                Console.WriteLine($"Heater {heaterId} level set to {level}.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Heater Level. Please enter a value between 0 and 5.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid Heater Number.");
                    }
                    break;
                case "3":

                    Console.Write("Enter Sensor Number: ");
                    if (int.TryParse(Console.ReadLine(), out int sensorId))
                    {
                        try
                        {
                            double temperature = await sensorService.GetTemperatureAsync(sensorId);
                            Console.WriteLine($"Sensor {sensorId} Temperature: {temperature:F1}°C");
                        }
                        catch (KeyNotFoundException)
                        {
                            Console.WriteLine($"Sensor {sensorId} not found. Valid sensors are 1, 2, or 3.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid Sensor Number.");
                    }
                    break;
                case "4":
                    Console.WriteLine("Fetching the state of all devices...");

                    try
                    {
                        await DisplayAllDeviceStatesAsync(fanService, heaterService, sensorService, config.SensorCount);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching device states: {ex.Message}");
                    }
                    break;
                case "5":
                    await RunTemperatureControlLoop(sensorService, heatUpStrategy, coolDownStrategy, holdStrategy);
                    break;
                case "6":
                    // await Reset(client);
                    Console.WriteLine("Resetting client state...");

                    try
                    {
                        // Send a POST request to the reset endpoint
                        var response = await client.PostAsync("api/Envo/reset", null);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Client state has been successfully reset.");
                            Console.WriteLine("Fetching the state of all devices...");

                            try
                            {
                                await DisplayAllDeviceStatesAsync(fanService, heaterService, sensorService, config.SensorCount);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error fetching device states: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to reset client state: {response.ReasonPhrase}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while resetting client state: {ex.Message}");
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
    /// Runs the interactive temperature-control loop using the heater, fan, and sensor facades.
    /// </summary>
    /// <param name="sensorService">The sensor facade used to read current temperatures.</param>
    /// <param name="heatUpStrategy">The strategy used when the target temperature is above the current temperature.</param>
    /// <param name="coolDownStrategy">The strategy used when the target temperature is below the current temperature.</param>
    /// <param name="holdStrategy">The strategy used to maintain a target temperature.</param>
    static async Task RunTemperatureControlLoop(
        ISensorService sensorService,
        ITemperatureControlStrategy heatUpStrategy,
        ITemperatureControlStrategy coolDownStrategy,
        ITemperatureControlStrategy holdStrategy)
    {
        Console.WriteLine("Starting temperature control algorithm...");

        double currentTemperature = await sensorService.GetAverageTemperatureAsync();

        // Prompt user for the final target temperature in Phase 4
        Console.Write("Enter the final target temperature for Phase 4: ");
        if (!double.TryParse(Console.ReadLine(), out double finalTargetTemperature))
        {
            Console.WriteLine("Invalid input. Please enter a valid numeric temperature.");
            return;
        }

        while (true)
        {
            // Phase 1: Gradually increase to 20°C over 30 seconds
            Console.WriteLine("Heating to 20.0°C over 30 seconds...");
            currentTemperature = await ExecuteAdjustmentPhaseAsync(currentTemperature, 20.0, 30, heatUpStrategy, coolDownStrategy);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");

            // Phase 2: Rapidly cool to 16°C
            Console.WriteLine("Cooling to 16.0°C over 10 seconds...");
            currentTemperature = await ExecuteAdjustmentPhaseAsync(currentTemperature, 16.0, 10, heatUpStrategy, coolDownStrategy);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");

            // Phase 3: Hold at 16°C for 10 seconds
            Console.WriteLine("Holding temperature at 16.0°C for 10 seconds...");
            currentTemperature = await holdStrategy.ExecuteAsync(currentTemperature, 16.0, 10);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");

            // Phase 4: Gradually adjust to the user-defined target temperature and maintain
            Console.WriteLine($"Adjusting temperature to {finalTargetTemperature:F1}°C over 20 seconds...");
            currentTemperature = await ExecuteAdjustmentPhaseAsync(currentTemperature, finalTargetTemperature, 20, heatUpStrategy, coolDownStrategy);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
            Console.WriteLine($"Holding temperature at {finalTargetTemperature:F1}°C until exit...");
            currentTemperature = await holdStrategy.ExecuteAsync(currentTemperature, finalTargetTemperature, int.MaxValue);
        }
    }

    /// <summary>
    /// Executes the appropriate adjustment strategy for the requested target temperature.
    /// </summary>
    /// <param name="currentTemperature">The current average temperature before the adjustment loop starts.</param>
    /// <param name="targetTemperature">The target average temperature.</param>
    /// <param name="durationSeconds">The maximum number of one-second adjustment iterations to run.</param>
    /// <param name="heatUpStrategy">The strategy used when heating is required.</param>
    /// <param name="coolDownStrategy">The strategy used when cooling is required.</param>
    /// <returns>The final average temperature observed when the phase ends.</returns>
    static async Task<double> ExecuteAdjustmentPhaseAsync(
        double currentTemperature,
        double targetTemperature,
        int durationSeconds,
        ITemperatureControlStrategy heatUpStrategy,
        ITemperatureControlStrategy coolDownStrategy)
    {
        if (Math.Abs(currentTemperature - targetTemperature) <= TemperatureTolerance)
        {
            return currentTemperature;
        }

        var selectedStrategy = currentTemperature < targetTemperature ? heatUpStrategy : coolDownStrategy;
        return await selectedStrategy.ExecuteAsync(currentTemperature, targetTemperature, durationSeconds);
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

        Console.WriteLine("Fetching sensor temperatures individually...");

        try
        {
            await DisplaySensorTemperaturesAsync(sensorService, sensorCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching sensor data: {ex.Message}");
        }
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
        for (int sensorId = 1; sensorId <= sensorCount; sensorId++)
        {
            double temperature = await sensorService.GetTemperatureAsync(sensorId);
            Console.WriteLine($"  Sensor {sensorId}: Temperature {temperature:F1} (Deg)");
        }
    }


}
