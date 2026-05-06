using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>
/// Adapter that wraps the Sensor 3 HTTP endpoint and converts its raw <see cref="decimal"/>
/// response into a <see cref="double"/> temperature value, conforming to <see cref="ISensor"/>.
/// </summary>
public sealed class Sensor3Adapter : ISensor
{
    private readonly IHttpService _httpService;

    /// <summary>
    /// Gets the fixed identifier of Sensor 3.
    /// </summary>
    public int SensorId => 3;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor3Adapter"/>.
    /// </summary>
    public Sensor3Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    public async Task<double> GetTemperatureAsync()
    {
        string content;

        try
        {
            content = await _httpService.GetAsync("api/Sensor/sensor3");
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException("Unable to load sensor 3 right now.", ex);
        }

        if (!decimal.TryParse(content, out decimal temperature))
        {
            throw new DeviceServiceException("Unable to read the current temperature from sensor 3.");
        }

        return (double)temperature;
    }
}
