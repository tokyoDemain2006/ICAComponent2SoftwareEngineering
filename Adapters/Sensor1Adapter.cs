using UglyClient.Interfaces;
using UglyClient.Services;

namespace UglyClient.Adapters;

/// <summary>
/// Adapter that wraps the Sensor 1 HTTP endpoint and converts its raw <see cref="string"/>
/// response into a <see cref="double"/> temperature value, conforming to <see cref="ISensor"/>.
/// </summary>
/// <remarks>
/// Sensor 1 returns its reading as a plain text string (e.g. <c>"21.5"</c>).
/// This adapter parses that string with <see cref="double.Parse(string)"/> so that
/// callers never need to know the underlying representation.
/// </remarks>
public sealed class Sensor1Adapter : ISensor
{
    /// <summary>The shared HTTP service used to make requests to the sensor API.</summary>
    private readonly IHttpService _httpService;

    /// <summary>
    /// Gets the fixed identifier of Sensor 1.
    /// </summary>
    public int SensorId => 1;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor1Adapter"/>.
    /// </summary>
    /// <param name="httpService">
    /// The shared HTTP service used for sensor requests.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpService"/> is <see langword="null"/>.
    /// </exception>
    public Sensor1Adapter(IHttpService httpService)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls <c>GET api/Sensor/sensor1</c>. The API returns the temperature as a
    /// plain <see cref="string"/> which is parsed to <see cref="double"/>.
    /// </remarks>
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
