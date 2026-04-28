using System.Text.Json;
using UglyClient.Models;

namespace UglyClient.Services;

/// <summary>
/// Concrete implementation of <see cref="IFanService"/> that communicates with the environment
/// simulation API via an injected <see cref="IHttpService"/>.
/// All fan-related HTTP calls and their error handling are encapsulated here; no other class
/// should make raw HTTP calls for fan operations.
/// </summary>
public class FanService : IFanService
{
    /// <summary>
    /// The shared HTTP service used to communicate with the simulation API.
    /// </summary>
    private readonly IHttpService _httpService;

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
    /// <see cref="IHttpService"/> and optional fan count.
    /// </summary>
    /// <param name="httpService">
    /// The shared HTTP service used for fan requests. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="fanCount">
    /// The total number of fans in the simulation. Defaults to <c>3</c> when not supplied.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpService"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fanCount"/> is less than 1.</exception>
    public FanService(IHttpService httpService, int fanCount = 3)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        if (fanCount < 1)
            throw new ArgumentOutOfRangeException(nameof(fanCount), "Fan count must be at least 1.");
        _fanCount = fanCount;
    }

    /// <inheritdoc/>
    public async Task SetFanStateAsync(int fanId, bool isOn)
    {
        try
        {
            await _httpService.PostAsync($"api/fans/{fanId}", isOn.ToString().ToLowerInvariant());
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException($"Unable to update fan {fanId} right now.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<FanDTO> GetFanStateAsync(int fanId)
    {
        string json;

        try
        {
            json = await _httpService.GetAsync($"api/fans/{fanId}/state");
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException($"Unable to load fan {fanId} right now.", ex);
        }

        try
        {
            var fan = JsonSerializer.Deserialize<FanDTO>(json, JsonOptions);
            return fan ?? throw new DeviceServiceException($"Unable to read the current state for fan {fanId}.");
        }
        catch (JsonException ex)
        {
            throw new DeviceServiceException($"Unable to read the current state for fan {fanId}.", ex);
        }
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
