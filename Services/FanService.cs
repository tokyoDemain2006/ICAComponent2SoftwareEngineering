using System.Text;
using System.Text.Json;
using UglyClient.Models;

namespace UglyClient.Services;

/// <summary>
/// Concrete implementation of <see cref="IFanService"/> that communicates with the environment
/// simulation API via an injected <see cref="HttpClient"/>.
/// All fan-related HTTP calls and their error handling are encapsulated here; no other class
/// should make raw HTTP calls for fan operations.
/// </summary>
public class FanService : IFanService
{
    /// <summary>
    /// The HTTP client used to communicate with the simulation API.
    /// Must have its <see cref="HttpClient.BaseAddress"/> set before being injected.
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// The total number of fans managed by the simulation.
    /// Used by <see cref="SetAllFansAsync"/> and <see cref="GetAllFanStatesAsync"/> to
    /// iterate over all fan IDs (1 … <see cref="_fanCount"/>).
    /// </summary>
    private readonly int _fanCount;

    /// <summary>
    /// Shared <see cref="JsonSerializerOptions"/> configured for case-insensitive property
    /// name matching, compatible with the simulation API's JSON responses.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initialises a new instance of <see cref="FanService"/> with the specified
    /// <see cref="HttpClient"/> and optional fan count.
    /// </summary>
    /// <param name="client">
    /// The pre-configured <see cref="HttpClient"/> (with <c>BaseAddress</c> and any required
    /// headers already set). Must not be <c>null</c>.
    /// </param>
    /// <param name="fanCount">
    /// The total number of fans in the simulation. Defaults to <c>3</c> when not supplied.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fanCount"/> is less than 1.</exception>
    public FanService(HttpClient client, int fanCount = 3)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        if (fanCount < 1)
            throw new ArgumentOutOfRangeException(nameof(fanCount), "Fan count must be at least 1.");
        _fanCount = fanCount;
    }

    /// <inheritdoc/>
    public async Task SetFanStateAsync(int fanId, bool isOn)
    {
        var body = new StringContent(isOn.ToString().ToLower(), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"api/fans/{fanId}", body);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to set fan state for fan {fanId}: {response.ReasonPhrase}");
    }

    /// <inheritdoc/>
    public async Task<FanDTO> GetFanStateAsync(int fanId)
    {
        var response = await _client.GetAsync($"api/fans/{fanId}/state");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to get fan state for fan {fanId}: {response.ReasonPhrase}");

        var json = await response.Content.ReadAsStringAsync();
        var fan = JsonSerializer.Deserialize<FanDTO>(json, JsonOptions);

        return fan ?? throw new Exception($"Fan {fanId} returned a null or unreadable response.");
    }

    /// <inheritdoc/>
    public async Task SetAllFansAsync(bool isOn)
    {
        for (int i = 1; i <= _fanCount; i++)
            await SetFanStateAsync(i, isOn);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FanDTO>> GetAllFanStatesAsync()
    {
        var fans = new List<FanDTO>(_fanCount);

        for (int i = 1; i <= _fanCount; i++)
            fans.Add(await GetFanStateAsync(i));

        return fans;
    }
}
