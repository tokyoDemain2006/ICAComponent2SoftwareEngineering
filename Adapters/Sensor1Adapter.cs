using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>
/// Adapter that wraps the Sensor 1 HTTP endpoint and converts its raw <see cref="string"/>
/// response into a <see cref="double"/> temperature value, conforming to <see cref="ISensor"/>.
/// </summary>
public sealed class Sensor1Adapter : ISensor
{
    private readonly IHttpService _httpService;

    /// <summary>
    /// Gets the fixed identifier of Sensor 1.
    /// </summary>
    public int SensorId => 1;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor1Adapter"/>.
    /// </summary>
    public Sensor1Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    public async Task<double> GetTemperatureAsync()
    {
        string content;

        try
        {
            content = await _httpService.GetAsync("api/Sensor/sensor1");
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException("Unable to load sensor 1 right now.", ex);
        }

        if (!double.TryParse(content, out double temperature))
        {
            throw new DeviceServiceException("Unable to read the current temperature from sensor 1.");
        }

        return temperature;
    }
}
