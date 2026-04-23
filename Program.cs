using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UglyClient.Adapters;
using UglyClient.Config;
using UglyClient.Interfaces;
using UglyClient.Models;

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
                            await SetFanState(client, fanId, isOn);
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
                                await SetHeaterLevel(client, heaterId, level);
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
                            ISensor? selectedAdapter = sensorId switch
                            {
                                1 => sensor1Adapter,
                                2 => sensor2Adapter,
                                3 => sensor3Adapter,
                                _ => null
                            };

                            if (selectedAdapter is null)
                            {
                                Console.WriteLine($"Sensor {sensorId} not found. Valid sensors are 1, 2, or 3.");
                            }
                            else
                            {
                                double temperature = await selectedAdapter.GetTemperatureAsync();
                                Console.WriteLine($"Sensor {sensorId} Temperature: {temperature:F1}°C");
                            }
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
                        Console.WriteLine("Fetching fan states individually...");
                        for (int i = 1; i <= config.FanCount; i++)
                        {
                            var fanResponse = await client.GetAsync($"api/fans/{i}/state");
                            if (fanResponse.IsSuccessStatusCode)
                            {
                                var fanJson = await fanResponse.Content.ReadAsStringAsync();
                                var fan = JsonSerializer.Deserialize<FanDTO>(fanJson, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                                Console.WriteLine($"  Fan {fan.Id}: {(fan.IsOn ? "On" : "Off")}");
                            }
                            else
                            {
                                Console.WriteLine($"  Fan {i}: Failed to fetch state.");
                            }
                        }
                        Console.WriteLine("Fetching heater levels individually...");
                        for (int i = 1; i <= config.HeaterCount; i++)
                        {
                            var heaterResponse = await client.GetAsync($"api/heat/{i}/level");
                            if (heaterResponse.IsSuccessStatusCode)
                            {
                                var levelString = await heaterResponse.Content.ReadAsStringAsync();
                                if (int.TryParse(levelString, out int level))
                                {
                                    Console.WriteLine($"  Heater {i}: Level {level}");
                                }
                                else
                                {
                                    Console.WriteLine($"  Heater {i}: Failed to parse level.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"  Heater {i}: Failed to fetch level.");
                            }
                        }
                        Console.WriteLine("Fetching sensor temperatures individually...");
                        try
                        {
                            var s1Temp = await sensor1Adapter.GetTemperatureAsync();
                            Console.WriteLine($"  Sensor 1: Temperature {s1Temp:F1} (Deg)");

                            var s2Temp = await sensor2Adapter.GetTemperatureAsync();
                            Console.WriteLine($"  Sensor 2: Temperature {s2Temp:F1} (Deg)");

                            var s3Temp = await sensor3Adapter.GetTemperatureAsync();
                            Console.WriteLine($"  Sensor 3: Temperature {s3Temp:F1} (Deg)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error fetching sensor data: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching device states: {ex.Message}");
                    }
                    break;
                case "5":
                    //await RunTemperatureControlLoop(client);
                    Console.WriteLine("Starting temperature control algorithm...");
                    Console.Write("Provide a final Temp Value: ");
                    double currentTemperature = await GetAverageTemperature(sensorAdapters);
                    while (true)
                    {
                        // Phase 1: Gradually increase to 20°C over 30 seconds
                        currentTemperature = await AdjustTemperature(client, sensorAdapters, currentTemperature, 20.0, 30);

                        // Phase 2: Rapidly cool to 16°C
                        currentTemperature = await AdjustTemperature(client, sensorAdapters, currentTemperature, 16.0, 10);

                        // Phase 3: Hold at 16°C for 10 seconds
                        currentTemperature = await HoldTemperature(client, sensorAdapters, currentTemperature, 16.0, 10);

                        // Phase 4: Gradually return to 18°C and maintain
                        currentTemperature = await AdjustTemperature(client, sensorAdapters, currentTemperature, 18.0, 20);
                        currentTemperature = await HoldTemperature(client, sensorAdapters, currentTemperature, 18.0, int.MaxValue); // Maintain until exit
                    }
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
                                // Get individual fan states
                                Console.WriteLine("Fetching fan states individually...");
                                for (int i = 1; i <= config.FanCount; i++)
                                {
                                    var fanResponse = await client.GetAsync($"api/fans/{i}/state");
                                    if (fanResponse.IsSuccessStatusCode)
                                    {
                                        var fanJson = await fanResponse.Content.ReadAsStringAsync();
                                        var fan = JsonSerializer.Deserialize<FanDTO>(fanJson, new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });
                                        Console.WriteLine($"  Fan {fan.Id}: {(fan.IsOn ? "On" : "Off")}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"  Fan {i}: Failed to fetch state.");
                                    }
                                }

                                // Get individual heater levels
                                Console.WriteLine("Fetching heater levels individually...");
                                for (int i = 1; i <= config.HeaterCount; i++)
                                {
                                    var heaterResponse = await client.GetAsync($"api/heat/{i}/level");
                                    if (heaterResponse.IsSuccessStatusCode)
                                    {
                                        var levelString = await heaterResponse.Content.ReadAsStringAsync();
                                        if (int.TryParse(levelString, out int level))
                                        {
                                            Console.WriteLine($"  Heater {i}: Level {level}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"  Heater {i}: Failed to parse level.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"  Heater {i}: Failed to fetch level.");
                                    }
                                }

                                // Get individual sensor temperatures
                                Console.WriteLine("Fetching sensor temperatures individually...");
                                try
                                {
                                    var s1Temp = await sensor1Adapter.GetTemperatureAsync();
                                    Console.WriteLine($"  Sensor 1: Temperature {s1Temp:F1} (Deg)");

                                    var s2Temp = await sensor2Adapter.GetTemperatureAsync();
                                    Console.WriteLine($"  Sensor 2: Temperature {s2Temp:F1} (Deg)");

                                    var s3Temp = await sensor3Adapter.GetTemperatureAsync();
                                    Console.WriteLine($"  Sensor 3: Temperature {s3Temp:F1} (Deg)");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error fetching sensor data: {ex.Message}");
                                }
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

    private static async Task Reset(HttpClient client, SimulationConfig config, IEnumerable<ISensor> sensors)
    {
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
                    // Get individual fan states
                    Console.WriteLine("Fetching fan states individually...");
                    for (int i = 1; i <= config.FanCount; i++)
                    {
                        var fanResponse = await client.GetAsync($"api/fans/{i}/state");
                        if (fanResponse.IsSuccessStatusCode)
                        {
                            var fanJson = await fanResponse.Content.ReadAsStringAsync();
                            var fan = JsonSerializer.Deserialize<FanDTO>(fanJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            Console.WriteLine($"  Fan {fan.Id}: {(fan.IsOn ? "On" : "Off")}");
                        }
                        else
                        {
                            Console.WriteLine($"  Fan {i}: Failed to fetch state.");
                        }
                    }

                    // Get individual heater levels
                    Console.WriteLine("Fetching heater levels individually...");
                    for (int i = 1; i <= config.HeaterCount; i++)
                    {
                        var heaterResponse = await client.GetAsync($"api/heat/{i}/level");
                        if (heaterResponse.IsSuccessStatusCode)
                        {
                            var levelString = await heaterResponse.Content.ReadAsStringAsync();
                            if (int.TryParse(levelString, out int level))
                            {
                                Console.WriteLine($"  Heater {i}: Level {level}");
                            }
                            else
                            {
                                Console.WriteLine($"  Heater {i}: Failed to parse level.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"  Heater {i}: Failed to fetch level.");
                        }
                    }

                    // Get individual sensor temperatures
                    Console.WriteLine("Fetching sensor temperatures individually...");
                    try
                    {
                        int sensorIndex = 1;
                        foreach (var sensor in sensors)
                        {
                            var temp = await sensor.GetTemperatureAsync();
                            Console.WriteLine($"  Sensor {sensorIndex}: Temperature {temp:F1} (Deg)");
                            sensorIndex++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching sensor data: {ex.Message}");
                    }
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
    }

    static async Task RunTemperatureControlLoop(HttpClient client, IEnumerable<ISensor> sensors)
    {
        Console.WriteLine("Starting temperature control algorithm...");

        double currentTemperature = await GetAverageTemperature(sensors);

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
            currentTemperature = await AdjustTemperature(client, sensors, currentTemperature, 20.0, 30);

            // Phase 2: Rapidly cool to 16°C
            currentTemperature = await AdjustTemperature(client, sensors, currentTemperature, 16.0, 10);

            // Phase 3: Hold at 16°C for 10 seconds
            currentTemperature = await HoldTemperature(client, sensors, currentTemperature, 16.0, 10);

            // Phase 4: Gradually adjust to the user-defined target temperature and maintain
            currentTemperature = await AdjustTemperature(client, sensors, currentTemperature, finalTargetTemperature, 20);
            currentTemperature = await HoldTemperature(client, sensors, currentTemperature, finalTargetTemperature, int.MaxValue); // Maintain until exit
        }
    }

    static async Task<double> AdjustTemperature(HttpClient client, IEnumerable<ISensor> sensors, double currentTemperature, double targetTemperature, int durationSeconds)
    {
        Console.WriteLine($"Adjusting temperature to {targetTemperature}°C over {durationSeconds} seconds...");
        int intervalMs = 1000; // 1-second intervals
        int iterations = durationSeconds;

        for (int i = 0; i < iterations; i++)
        {
            if (Math.Abs(currentTemperature - targetTemperature) <= 0.1) break;

            if (currentTemperature < targetTemperature)
            {
                // Turn on heaters and reduce fan activity
                await SetAllHeaters(client, 3); // Set heaters to level 3
                await SetAllFans(client, false); // Turn off fans
            }
            else
            {
                // Turn off heaters and increase fan activity
                await SetAllHeaters(client, 0); // Turn off heaters
                await SetAllFans(client, true); // Turn on fans
            }

            // Wait for a second and fetch the updated temperature
            await Task.Delay(intervalMs);
            currentTemperature = await GetAverageTemperature(sensors);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }

    static async Task<double> HoldTemperature(HttpClient client, IEnumerable<ISensor> sensors, double currentTemperature, double targetTemperature, int durationSeconds)
    {
        Console.WriteLine($"Holding temperature at {targetTemperature}°C for {durationSeconds} seconds...");
        int intervalMs = 1000; // 1-second intervals

        for (int i = 0; i < durationSeconds; i++)
        {
            if (currentTemperature < targetTemperature)
            {
                // Turn on heaters slightly and reduce fans
                await SetAllHeaters(client, 1); // Minimal heating
                await SetAllFans(client, false); // Reduce cooling
            }
            else if (currentTemperature > targetTemperature)
            {
                // Turn off heaters and increase fans
                await SetAllHeaters(client, 0); // Turn off heating
                await SetAllFans(client, true); // Activate cooling
            }

            // Wait for a second and fetch the updated temperature
            await Task.Delay(intervalMs);
            currentTemperature = await GetAverageTemperature(sensors);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }

    /// <summary>
    /// Calculates the average temperature across all provided sensor adapters.
    /// </summary>
    /// <param name="sensors">The collection of <see cref="ISensor"/> adapters to query.</param>
    /// <returns>The mean temperature as a <see cref="double"/>.</returns>
    static async Task<double> GetAverageTemperature(IEnumerable<ISensor> sensors)
    {
        double total = 0;
        int count = 0;

        foreach (var sensor in sensors)
        {
            total += await sensor.GetTemperatureAsync();
            count++;
        }

        return count > 0 ? total / count : 0;
    }

    static async Task SetAllHeaters(HttpClient client, int level)
    {
        for (int i = 1; i <= 3; i++) // Assuming 3 heaters
        {
            await SetHeaterLevel(client, i, level);
        }
    }

    static async Task SetAllFans(HttpClient client, bool state)
    {
        for (int i = 1; i <= 3; i++) // Assuming 3 fans
        {
            await SetFanState(client, i, state);
        }
    }

 

    static async Task SetHeaterLevel(HttpClient client, int heaterId, int level)
    {
        var response = await client.PostAsync($"api/heat/{heaterId}",
            new StringContent(level.ToString(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set heater level {heaterId}: {response.ReasonPhrase}");
        }
    }

    static async Task SetFanState(HttpClient client, int fanId, bool isOn)
    {
        var response = await client.PostAsync($"api/fans/{fanId}",
            new StringContent(isOn.ToString().ToLower(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set fan state for fan {fanId}: {response.ReasonPhrase}");
        }
    }


}
