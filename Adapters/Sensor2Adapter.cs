using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>Adapts the Sensor 2 HTTP endpoint (plain integer string) to <see cref="ISensor"/>.</summary>
/// <remarks>
/// Sensor 2 returns its reading as a plain integer string (e.g. <c>"21"</c>), widened to <see cref="double"/>.
/// </remarks>
public sealed class Sensor2Adapter : ISensor
{
    private readonly IHttpService _httpService;

    /// <summary>Fixed identifier for Sensor 2.</summary>
    public int SensorId => 2;

    /// <summary>Initialises a new instance of <see cref="Sensor2Adapter"/>.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpService"/> is <see langword="null"/>.</exception>
    public Sensor2Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    /// <inheritdoc />
    /// <remarks>Calls <c>GET api/Sensor/sensor2</c>. Returns an integer string parsed to <see cref="double"/>.</remarks>
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
