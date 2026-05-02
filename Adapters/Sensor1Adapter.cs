using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>Adapts the Sensor 1 HTTP endpoint (plain text <see cref="double"/>) to <see cref="ISensor"/>.</summary>
/// <remarks>
/// Sensor 1 returns its reading as a plain text string (e.g. <c>"21.5"</c>).
/// </remarks>
public sealed class Sensor1Adapter : ISensor
{
    private readonly IHttpService _httpService;

    /// <summary>Fixed identifier for Sensor 1.</summary>
    public int SensorId => 1;

    /// <summary>Initialises a new instance of <see cref="Sensor1Adapter"/>.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpService"/> is <see langword="null"/>.</exception>
    public Sensor1Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    /// <inheritdoc />
    /// <remarks>Calls <c>GET api/Sensor/sensor1</c>. Returns a plain text string parsed to <see cref="double"/>.</remarks>
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
