using UglyClient.Interfaces;

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
    /// <summary>The shared <see cref="HttpClient"/> used to make requests to the sensor API.</summary>
    private readonly HttpClient _client;

    /// <summary>
    /// Gets the fixed identifier of Sensor 2.
    /// </summary>
    public int SensorId => 2;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor2Adapter"/>.
    /// </summary>
    /// <param name="client">
    /// The <see cref="HttpClient"/> pre-configured with the base address and any
    /// required authentication headers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="client"/> is <see langword="null"/>.
    /// </exception>
    public Sensor2Adapter(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls <c>GET api/Sensor/sensor2</c>. The API returns the temperature as an
    /// integer string which is parsed to <see cref="int"/> and then widened to
    /// <see cref="double"/>.
    /// </remarks>
    public async Task<double> GetTemperatureAsync()
    {
        var response = await _client.GetAsync("api/Sensor/sensor2");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get temperature from Sensor 2: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();

        if (!int.TryParse(content, out int temperature))
        {
            throw new Exception($"Failed to parse Sensor 2 temperature as an integer. Raw value: '{content}'");
        }

        return (double)temperature;
    }
}
