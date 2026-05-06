using UglyClient.Config;
using UglyClient.Services;

namespace UglyClient.UI;

/// <summary>
/// Owns the interactive menu loop for the simulation client.
/// Reads user input from an injected <see cref="TextReader"/> and writes output to an injected
/// <see cref="TextWriter"/> so the loop is fully testable without touching the real console.
/// </summary>
public sealed class MenuController
{
    private readonly IFanService _fanService;
    private readonly IHeaterService _heaterService;
    private readonly ISensorService _sensorService;
    private readonly ISimulationService _simulationService;
    private readonly SimulationConfig _config;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public MenuController(
        IFanService fanService,
        IHeaterService heaterService,
        ISensorService sensorService,
        ISimulationService simulationService,
        SimulationConfig config,
        TextReader? input = null,
        TextWriter? output = null)
    {
        _fanService = fanService;
        _heaterService = heaterService;
        _sensorService = sensorService;
        _simulationService = simulationService;
        _config = config;
        _input = input ?? Console.In;
        _output = output ?? Console.Out;
    }

    public async Task RunAsync()
    {
        bool running = true;

        while (running)
        {
            _output.Write(DeviceDisplayFormatter.FormatMenu());

            var choice = _input.ReadLine();

            switch (choice)
            {
                case "1":
                    await HandleFanAsync();
                    break;
                case "2":
                    await HandleHeaterAsync();
                    break;
                case "3":
                    await HandleSensorAsync();
                    break;
                case "4":
                    await HandleDisplayAllAsync();
                    break;
                case "5":
                    await HandleSimulationAsync();
                    break;
                case "6":
                    await HandleResetAsync();
                    break;
                case "7":
                    _output.WriteLine("Exiting. Goodbye!");
                    running = false;
                    break;
                default:
                    _output.WriteLine("Invalid option. Please try again.");
                    break;
            }

            if (running)
                _output.WriteLine();
        }
    }

    private async Task HandleFanAsync()
    {
        _output.Write("Enter Fan Number: ");
        if (!int.TryParse(_input.ReadLine(), out int fanId) || !IsValidDeviceId(fanId, _config.FanCount))
        {
            _output.WriteLine($"Invalid Fan Number. Please enter a value between 1 and {_config.FanCount}.");
            return;
        }

        _output.Write("Turn Fan On or Off? (on/off): ");
        var stateInput = _input.ReadLine()?.ToLower();
        if (stateInput != "on" && stateInput != "off")
        {
            _output.WriteLine("Invalid Fan State. Please enter 'on' or 'off'.");
            return;
        }

        bool isOn = stateInput == "on";

        try
        {
            await _fanService.SetFanStateAsync(fanId, isOn);
            _output.WriteLine(DeviceDisplayFormatter.FormatFanState(fanId, isOn));
        }
        catch (DeviceServiceException ex)
        {
            _output.WriteLine(ex.Message);
        }
    }

    private async Task HandleHeaterAsync()
    {
        _output.Write("Enter Heater Number: ");
        if (!int.TryParse(_input.ReadLine(), out int heaterId) || !IsValidDeviceId(heaterId, _config.HeaterCount))
        {
            _output.WriteLine($"Invalid Heater Number. Please enter a value between 1 and {_config.HeaterCount}.");
            return;
        }

        _output.Write("Set Heater Level (0-5): ");
        if (!int.TryParse(_input.ReadLine(), out int level) || level < 0 || level > 5)
        {
            _output.WriteLine("Invalid Heater Level. Please enter a value between 0 and 5.");
            return;
        }

        try
        {
            await _heaterService.SetHeaterLevelAsync(heaterId, level);
            _output.WriteLine(DeviceDisplayFormatter.FormatHeaterLevel(heaterId, level));
        }
        catch (DeviceServiceException ex)
        {
            _output.WriteLine(ex.Message);
        }
    }

    private async Task HandleSensorAsync()
    {
        _output.Write("Enter Sensor Number: ");
        if (!int.TryParse(_input.ReadLine(), out int sensorId) || !IsValidDeviceId(sensorId, _config.SensorCount))
        {
            _output.WriteLine($"Invalid Sensor Number. Please enter a value between 1 and {_config.SensorCount}.");
            return;
        }

        try
        {
            double temperature = await _sensorService.GetTemperatureAsync(sensorId);
            _output.WriteLine(DeviceDisplayFormatter.FormatSensorReading(sensorId, temperature));
        }
        catch (DeviceServiceException ex)
        {
            _output.WriteLine(ex.Message);
        }
    }

    private async Task HandleDisplayAllAsync()
    {
        _output.WriteLine("Fetching the state of all devices...");

        try
        {
            _output.WriteLine("Fetching fan states individually...");
            foreach (var fan in await _fanService.GetAllFanStatesAsync())
                _output.WriteLine(DeviceDisplayFormatter.FormatFanState(fan.Id, fan.IsOn));

            _output.WriteLine("Fetching heater levels individually...");
            int heaterId = 1;
            foreach (var heaterLevel in await _heaterService.GetAllHeaterLevelsAsync())
                _output.WriteLine(DeviceDisplayFormatter.FormatHeaterLevel(heaterId++, heaterLevel));

            _output.WriteLine("Fetching sensor temperatures individually...");
            for (int sensorId = 1; sensorId <= _config.SensorCount; sensorId++)
            {
                double temperature = await _sensorService.GetTemperatureAsync(sensorId);
                _output.WriteLine(DeviceDisplayFormatter.FormatSensorReading(sensorId, temperature));
            }
        }
        catch (DeviceServiceException ex)
        {
            _output.WriteLine(ex.Message);
        }
    }

    private async Task HandleSimulationAsync()
    {
        _output.WriteLine("Starting temperature control algorithm...");

        try
        {
            await _simulationService.RunAsync(CancellationToken.None);
        }
        catch (DeviceServiceException ex)
        {
            _output.WriteLine(ex.Message);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Temperature control stopped.");
        }
    }

    private async Task HandleResetAsync()
    {
        _output.WriteLine("Resetting client state...");

        try
        {
            await _simulationService.ResetAsync();
            _output.WriteLine("Client state has been successfully reset.");
            await HandleDisplayAllAsync();
        }
        catch (DeviceServiceException ex)
        {
            _output.WriteLine(ex.Message);
        }
    }

    private static bool IsValidDeviceId(int deviceId, int deviceCount)
        => deviceId >= 1 && deviceId <= deviceCount;
}
