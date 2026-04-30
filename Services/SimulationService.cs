using UglyClient.Controllers;

namespace UglyClient.Services;

/// <summary>
/// Concrete implementation of <see cref="ISimulationService"/> for simulation-wide API actions.
/// </summary>
public sealed class SimulationService : ISimulationService
{
    /// <summary>
    /// The shared HTTP service used to communicate with the simulation API.
    /// </summary>
    private readonly IHttpService _httpService;

    /// <summary>
    /// The controller that orchestrates the temperature-control phase cycle.
    /// </summary>
    private readonly TemperatureController _temperatureController;

    /// <summary>
    /// Initialises a new instance of <see cref="SimulationService"/>.
    /// </summary>
    /// <param name="httpService">The shared HTTP service used for simulation requests.</param>
    /// <param name="temperatureController">The controller that orchestrates the temperature-control cycle.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpService"/> or <paramref name="temperatureController"/> is <see langword="null"/>.
    /// </exception>
    public SimulationService(IHttpService httpService, TemperatureController temperatureController)
    {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        _temperatureController = temperatureController ?? throw new ArgumentNullException(nameof(temperatureController));
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        await _temperatureController.RunFullCycleAsync(ct);
    }

    /// <inheritdoc />
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
