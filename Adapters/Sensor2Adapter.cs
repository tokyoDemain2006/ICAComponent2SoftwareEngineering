using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>
/// Adapter that wraps the Sensor 2 HTTP endpoint and converts its raw <see cref="int"/>
/// response into a <see cref="double"/> temperature value, conforming to <see cref="ISensor"/>.
/// </summary>
/// <remarks>
/// Sensor 2 returns its reading as a plain integer string (e.g. <c>"21"</c>).
/// This adapter parses that integer with <see cref="int.TryParse(string, out int)"/> and
/// widens it to <see cref="double"/> so that callers never need to know the underlying
/// representation.
/// </remarks>
public sealed class Sensor2Adapter : ISensor
{
    /// <summary>The shared HTTP service used to make requests to the sensor API.</summary>
    private readonly IHttpService _httpService;

    /// <summary>
    /// Gets the fixed identifier of Sensor 2.
    /// </summary>
    public int SensorId => 2;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor2Adapter"/>.
    /// </summary>
    /// <param name="httpService">
    /// The shared HTTP service used for sensor requests.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpService"/> is <see langword="null"/>.
    /// </exception>
    public Sensor2Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls <c>GET api/Sensor/sensor2</c>. The API returns the temperature as an
    /// integer string which is parsed to <see cref="int"/> and then widened to
    /// <see cref="double"/>.
    /// </remarks>
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
