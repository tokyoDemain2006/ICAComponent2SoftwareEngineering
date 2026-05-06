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
    private readonly IHttpService _httpService;

    private readonly int _fanCount;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initialises a new instance of <see cref="FanService"/> with the specified
    /// <see cref="IHttpService"/> and optional fan count.
    /// </summary>
    public FanService(IHttpService httpService, int fanCount = 3)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        if (fanCount < 1)
            throw new ArgumentOutOfRangeException(nameof(fanCount), "Fan count must be at least 1.");
        _fanCount = fanCount;
    }

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

    public async Task SetAllFansAsync(bool isOn)
    {
        for (int i = 1; i <= _fanCount; i++)
            await SetFanStateAsync(i, isOn);
    }

    public async Task<IEnumerable<FanDTO>> GetAllFanStatesAsync()
    {
        var fans = new List<FanDTO>(_fanCount);

        for (int i = 1; i <= _fanCount; i++)
            fans.Add(await GetFanStateAsync(i));

        return fans;
    }
}
