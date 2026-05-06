using UglyClient.Controllers;

namespace UglyClient.Services;

/// <summary>
/// Concrete implementation of <see cref="ISimulationService"/> for simulation-wide API actions.
/// </summary>
public sealed class SimulationService : ISimulationService
{
    private readonly IHttpService _httpService;

    private readonly TemperatureController _temperatureController;

    /// <summary>
    /// Initialises a new instance of <see cref="SimulationService"/>.
    /// </summary>
    public SimulationService(IHttpService httpService, TemperatureController temperatureController)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        _temperatureController = temperatureController ?? throw new ArgumentNullException(nameof(temperatureController));
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await _temperatureController.RunFullCycleAsync(ct);
    }

    public async Task ResetAsync()
    {
        try
        {
            await _httpService.PostAsync("api/Envo/reset", string.Empty);
        }
        catch (DeviceServiceException ex)
        {
            throw new DeviceServiceException("Unable to reset the simulation right now.", ex);
        }
    }
}
