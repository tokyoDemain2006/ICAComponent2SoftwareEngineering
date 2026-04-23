using System.Text;

namespace UglyClient.Services;

/// <summary>
/// Concrete implementation of <see cref="IHeaterService"/> that communicates with the environment
/// simulation API via an injected <see cref="HttpClient"/>.
/// All heater-related HTTP calls and their error handling are encapsulated here; no other class
/// should make raw HTTP calls for heater operations.
/// </summary>
public class HeaterService : IHeaterService
{
    /// <summary>
    /// The HTTP client used to communicate with the simulation API.
    /// Must have its <see cref="HttpClient.BaseAddress"/> set before being injected.
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// The total number of heaters managed by the simulation.
    /// Used by <see cref="SetAllHeatersAsync"/> and <see cref="GetAllHeaterLevelsAsync"/> to
    /// iterate over all heater IDs (1 … <see cref="_heaterCount"/>).
    /// </summary>
    private readonly int _heaterCount;

    /// <summary>
    /// Initialises a new instance of <see cref="HeaterService"/> with the specified
    /// <see cref="HttpClient"/> and optional heater count.
    /// </summary>
    /// <param name="client">
    /// The pre-configured <see cref="HttpClient"/> (with <c>BaseAddress</c> and any required
    /// headers already set). Must not be <c>null</c>.
    /// </param>
    /// <param name="heaterCount">
    /// The total number of heaters in the simulation. Defaults to <c>3</c> when not supplied.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="heaterCount"/> is less than 1.</exception>
    public HeaterService(HttpClient client, int heaterCount = 3)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        if (heaterCount < 1)
            throw new ArgumentOutOfRangeException(nameof(heaterCount), "Heater count must be at least 1.");
        _heaterCount = heaterCount;
    }

    /// <inheritdoc/>
    public async Task SetHeaterLevelAsync(int heaterId, int level)
    {
        var body = new StringContent(level.ToString(), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"api/heat/{heaterId}", body);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to set heater level for heater {heaterId}: {response.ReasonPhrase}");
    }

    /// <inheritdoc/>
    public async Task<int> GetHeaterLevelAsync(int heaterId)
    {
        var response = await _client.GetAsync($"api/heat/{heaterId}/level");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to get heater level for heater {heaterId}: {response.ReasonPhrase}");

        var body = await response.Content.ReadAsStringAsync();

        if (!int.TryParse(body, out int level))
            throw new Exception($"Heater {heaterId} returned an unreadable level response: '{body}'");

        return level;
    }

    /// <inheritdoc/>
    public async Task SetAllHeatersAsync(int level)
    {
        for (int i = 1; i <= _heaterCount; i++)
            await SetHeaterLevelAsync(i, level);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<int>> GetAllHeaterLevelsAsync()
    {
        var levels = new List<int>(_heaterCount);

        for (int i = 1; i <= _heaterCount; i++)
            levels.Add(await GetHeaterLevelAsync(i));

        return levels;
    }
}
