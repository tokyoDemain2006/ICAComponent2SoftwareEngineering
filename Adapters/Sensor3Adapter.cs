using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>Adapts the Sensor 3 HTTP endpoint (decimal string) to <see cref="ISensor"/>.</summary>
/// <remarks>
/// Sensor 3 returns its reading as a decimal string (e.g. <c>"21.75"</c>), cast to <see cref="double"/>.
/// </remarks>
public sealed class Sensor3Adapter : ISensor
{
    private readonly IHttpService _httpService;

    /// <summary>Fixed identifier for Sensor 3.</summary>
    public int SensorId => 3;

    /// <summary>Initialises a new instance of <see cref="Sensor3Adapter"/>.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpService"/> is <see langword="null"/>.</exception>
    public Sensor3Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    /// <inheritdoc />
    /// <remarks>Calls <c>GET api/Sensor/sensor3</c>. Returns a decimal string cast to <see cref="double"/>.</remarks>
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
