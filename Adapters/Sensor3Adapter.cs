using UglyClient.Interfaces;

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
    /// <summary>The shared <see cref="HttpClient"/> used to make requests to the sensor API.</summary>
    private readonly HttpClient _client;

    /// <summary>
    /// Initialises a new instance of <see cref="Sensor3Adapter"/>.
    /// </summary>
    /// <param name="client">
    /// The <see cref="HttpClient"/> pre-configured with the base address and any
    /// required authentication headers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="client"/> is <see langword="null"/>.
    /// </exception>
    public Sensor3Adapter(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls <c>GET api/Sensor/sensor3</c>. The API returns the temperature as a
    /// decimal string which is parsed to <see cref="decimal"/> and then cast to
    /// <see cref="double"/>.
    /// </remarks>
    public async Task<double> GetTemperatureAsync()
    {
        var response = await _client.GetAsync("api/Sensor/sensor3");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get temperature from Sensor 3: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();

        if (!decimal.TryParse(content, out decimal temperature))
        {
            throw new Exception($"Failed to parse Sensor 3 temperature as a decimal. Raw value: '{content}'");
        }

        return (double)temperature;
    }
}
