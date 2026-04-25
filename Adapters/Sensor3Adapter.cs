using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>
/// Adapter that wraps the Sensor 3 HTTP endpoint and converts its raw <see cref="decimal"/>
/// response into a <see cref="double"/> temperature value, conforming to <see cref="ISensor"/>.
/// </summary>
/// <remarks>
/// Sensor 3 returns its reading as a decimal string (e.g. <c>"21.75"</c>).
/// This adapter parses that decimal with <see cref="decimal.TryParse(string, out decimal)"/>
/// and casts it to <see cref="double"/> so that callers never need to know the underlying
/// representation.
/// </remarks>
public sealed class Sensor3Adapter : ISensor
{
    /// <summary>The shared HTTP service used to make requests to the sensor API.</summary>
    private readonly IHttpService _httpService;

    /// <summary>
    /// Gets the fixed identifier of Sensor 3.
    /// </summary>
    public int SensorId => 3;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor3Adapter"/>.
    /// </summary>
    /// <param name="httpService">
    /// The shared HTTP service used for sensor requests.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpService"/> is <see langword="null"/>.
    /// </exception>
    public Sensor3Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls <c>GET api/Sensor/sensor3</c>. The API returns the temperature as a
    /// decimal string which is parsed to <see cref="decimal"/> and then cast to
    /// <see cref="double"/>.
    /// </remarks>
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
