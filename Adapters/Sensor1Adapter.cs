using UglyClient.Interfaces;

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
    /// <summary>The shared <see cref="HttpClient"/> used to make requests to the sensor API.</summary>
    private readonly HttpClient _client;

    /// <summary>
    /// Gets the fixed identifier of Sensor 1.
    /// </summary>
    public int SensorId => 1;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor1Adapter"/>.
    /// </summary>
    /// <param name="client">
    /// The <see cref="HttpClient"/> pre-configured with the base address and any
    /// required authentication headers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="client"/> is <see langword="null"/>.
    /// </exception>
    public Sensor1Adapter(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls <c>GET api/Sensor/sensor1</c>. The API returns the temperature as a
    /// plain <see cref="string"/> which is parsed to <see cref="double"/>.
    /// </remarks>
    public async Task<double> GetTemperatureAsync()
    {
        var response = await _client.GetAsync("api/Sensor/sensor1");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get temperature from Sensor 1: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();

        if (!double.TryParse(content, out double temperature))
        {
            throw new Exception($"Failed to parse Sensor 1 temperature as a double. Raw value: '{content}'");
        }

        return temperature;
    }
}
