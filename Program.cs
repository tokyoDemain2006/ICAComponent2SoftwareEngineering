using System;
using System.Threading.Tasks;

class Program
{
    private const int DeviceCount = 3;

    static async Task Main(string[] args)
    {
        using var service = new HttpSimulationService("https://localhost:44351/", "u007-key");

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
                    await HandleFanControl(service);
                    break;
                case "2":
                    await HandleHeaterControl(service);
                    break;
                case "3":
                    await HandleReadTemperature(service);
                    break;
                case "4":
                    await DisplayAllDeviceState(service);
                    break;
                case "5":
                    await RunTemperatureControlLoop(service);
                    break;
                case "6":
                    await ResetAndDisplayState(service);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

            Console.WriteLine();
        }
    }

    private static async Task HandleFanControl(HttpSimulationService service)
    {
        Console.Write("Enter Fan Number: ");
        if (!int.TryParse(Console.ReadLine(), out int fanId))
        {
            Console.WriteLine("Invalid Fan Number.");
            return;
        }

        Console.Write("Turn Fan On or Off? (on/off): ");
        var stateInput = Console.ReadLine();
        bool isOn = stateInput?.ToLower() == "on";

        try
        {
            await service.SetFanState(fanId, isOn);
            Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task HandleHeaterControl(HttpSimulationService service)
    {
        Console.Write("Enter Heater Number: ");
        if (!int.TryParse(Console.ReadLine(), out int heaterId))
        {
            Console.WriteLine("Invalid Heater Number.");
            return;
        }

        Console.Write("Set Heater Level (0-5): ");
        if (!int.TryParse(Console.ReadLine(), out int level) || level < 0 || level > 5)
        {
            Console.WriteLine("Invalid Heater Level. Please enter a value between 0 and 5.");
            return;
        }

        try
        {
            await service.SetHeaterLevel(heaterId, level);
            Console.WriteLine($"Heater {heaterId} level set to {level}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task HandleReadTemperature(HttpSimulationService service)
    {
        Console.Write("Enter Sensor Number: ");
        if (!int.TryParse(Console.ReadLine(), out int sensorId))
        {
            Console.WriteLine("Invalid Sensor Number.");
            return;
        }

        try
        {
            double temperature = await service.GetSensorTemperature(sensorId);
            Console.WriteLine($"Sensor {sensorId} Temperature: {temperature:F1}°C");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task DisplayAllDeviceState(HttpSimulationService service)
    {
        Console.WriteLine("Fetching the state of all devices...");

        try
        {
            Console.WriteLine("Fetching fan states individually...");
            for (int i = 1; i <= DeviceCount; i++)
            {
                var fan = await service.GetFanState(i);
                Console.WriteLine($"  Fan {fan.Id}: {(fan.IsOn ? "On" : "Off")}");
            }

            Console.WriteLine("Fetching heater levels individually...");
            for (int i = 1; i <= DeviceCount; i++)
            {
                int level = await service.GetHeaterLevel(i);
                Console.WriteLine($"  Heater {i}: Level {level}");
            }

            Console.WriteLine("Fetching sensor temperatures individually...");
            var sensor1Temp = await service.GetSensor1Temperature();
            Console.WriteLine($"  Sensor 1: Temperature {sensor1Temp} (Deg)");

            var sensor2Temp = await service.GetSensor2Temperature();
            Console.WriteLine($"  Sensor 2: Temperature {sensor2Temp} (Deg)");

            var sensor3Temp = await service.GetSensor3Temperature();
            Console.WriteLine($"  Sensor 3: Temperature {sensor3Temp} (Deg)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching device states: {ex.Message}");
        }
    }

    private static async Task ResetAndDisplayState(HttpSimulationService service)
    {
        Console.WriteLine("Resetting client state...");

        try
        {
            await service.Reset();
            Console.WriteLine("Client state has been successfully reset.");
            await DisplayAllDeviceState(service);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while resetting client state: {ex.Message}");
        }
    }

    private static async Task RunTemperatureControlLoop(HttpSimulationService service)
    {
        Console.WriteLine("Starting temperature control algorithm...");
        double currentTemperature = await GetAverageTemperature(service);

        while (true)
        {
            currentTemperature = await AdjustTemperature(service, currentTemperature, 20.0, 30);
            currentTemperature = await AdjustTemperature(service, currentTemperature, 16.0, 10);
            currentTemperature = await HoldTemperature(service, currentTemperature, 16.0, 10);
            currentTemperature = await AdjustTemperature(service, currentTemperature, 18.0, 20);
            currentTemperature = await HoldTemperature(service, currentTemperature, 18.0, int.MaxValue);
        }
    }

    private static async Task<double> AdjustTemperature(HttpSimulationService service, double currentTemperature, double targetTemperature, int durationSeconds)
    {
        Console.WriteLine($"Adjusting temperature to {targetTemperature}°C over {durationSeconds} seconds...");

        for (int i = 0; i < durationSeconds; i++)
        {
            if (Math.Abs(currentTemperature - targetTemperature) <= 0.1)
            {
                break;
            }

            if (currentTemperature < targetTemperature)
            {
                await SetAllHeaters(service, 3);
                await SetAllFans(service, false);
            }
            else
            {
                await SetAllHeaters(service, 0);
                await SetAllFans(service, true);
            }

            await Task.Delay(1000);
            currentTemperature = await GetAverageTemperature(service);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }

    private static async Task<double> HoldTemperature(HttpSimulationService service, double currentTemperature, double targetTemperature, int durationSeconds)
    {
        Console.WriteLine($"Holding temperature at {targetTemperature}°C for {durationSeconds} seconds...");

        for (int i = 0; i < durationSeconds; i++)
        {
            if (currentTemperature < targetTemperature)
            {
                await SetAllHeaters(service, 1);
                await SetAllFans(service, false);
            }
            else if (currentTemperature > targetTemperature)
            {
                await SetAllHeaters(service, 0);
                await SetAllFans(service, true);
            }

            await Task.Delay(1000);
            currentTemperature = await GetAverageTemperature(service);
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }

    private static async Task<double> GetAverageTemperature(HttpSimulationService service)
    {
        var sensor1 = double.Parse(await service.GetSensor1Temperature());
        var sensor2 = await service.GetSensor2Temperature();
        var sensor3 = (double)await service.GetSensor3Temperature();

        return (sensor1 + sensor2 + sensor3) / 3;
    }

    private static async Task SetAllHeaters(HttpSimulationService service, int level)
    {
        for (int i = 1; i <= DeviceCount; i++)
        {
            await service.SetHeaterLevel(i, level);
        }
    }

    private static async Task SetAllFans(HttpSimulationService service, bool state)
    {
        for (int i = 1; i <= DeviceCount; i++)
        {
            await service.SetFanState(i, state);
        }
    }
}
