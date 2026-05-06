using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>
/// Adapter that wraps the Sensor 2 HTTP endpoint and converts its raw <see cref="int"/>
/// response into a <see cref="double"/> temperature value, conforming to <see cref="ISensor"/>.
/// </summary>
public sealed class Sensor2Adapter : ISensor
{
    private readonly IHttpService _httpService;

    /// <summary>
    /// Gets the fixed identifier of Sensor 2.
    /// </summary>
    public int SensorId => 2;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor2Adapter"/>.
    /// </summary>
    public Sensor2Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    public async Task<double> GetTemperatureAsync()
    {
        string content;

        try
        {
            content = await _httpService.GetAsync("api/Sensor/sensor2");
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException("Unable to load sensor 2 right now.", ex);
        }

        if (!int.TryParse(content, out int temperature))
        {
            throw new DeviceServiceException("Unable to read the current temperature from sensor 2.");
        }

        return temperature;
    }
}
