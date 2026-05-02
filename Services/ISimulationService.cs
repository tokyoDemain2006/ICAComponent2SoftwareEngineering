namespace UglyClient.Services;

/// <summary>Contract for simulation-wide operations against the environment API.</summary>
public interface ISimulationService
{
    /// <summary>Runs the end-to-end temperature-control cycle.</summary>
    /// <param name="ct">Token used to cancel the cycle cleanly.</param>
    /// <exception cref="DeviceServiceException">Thrown when a device operation fails during the cycle.</exception>
    Task RunAsync(CancellationToken ct);

    /// <summary>Resets the simulation to its default state.</summary>
    /// <exception cref="DeviceServiceException">Thrown when the reset request fails.</exception>
    Task ResetAsync();
}
