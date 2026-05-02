namespace UglyClient.Services;

/// <summary>Concrete <see cref="IHeaterService"/> implementation using an injected <see cref="IHttpService"/>.</summary>
public class HeaterService : IHeaterService
{
    private readonly IHttpService _httpService;
    private readonly int _heaterCount;

    /// <summary>Initialises a new instance of <see cref="HeaterService"/>.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpService"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="heaterCount"/> is less than 1.</exception>
    public HeaterService(IHttpService httpService, int heaterCount = 3)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        if (heaterCount < 1)
            throw new ArgumentOutOfRangeException(nameof(heaterCount), "Heater count must be at least 1.");
        _heaterCount = heaterCount;
    }

    /// <inheritdoc/>
    public async Task SetHeaterLevelAsync(int heaterId, int level)
    {
        try
        {
            await _httpService.PostAsync($"api/heat/{heaterId}", level.ToString());
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException($"Unable to update heater {heaterId} right now.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetHeaterLevelAsync(int heaterId)
    {
        string body;

        try
        {
            body = await _httpService.GetAsync($"api/heat/{heaterId}/level");
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException($"Unable to load heater {heaterId} right now.", ex);
        }

        if (!int.TryParse(body, out int level))
        {
            throw new DeviceServiceException($"Unable to read the current level for heater {heaterId}.");
        }

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
