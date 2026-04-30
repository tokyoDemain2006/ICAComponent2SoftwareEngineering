namespace UglyClient.Services;

/// <summary>
/// Defines the contract for simulation-wide operations against the environment API.
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Runs the end-to-end temperature-control cycle.
    /// </summary>
    /// <param name="ct">The token used to cancel the cycle cleanly.</param>
    /// <returns>A <see cref="Task"/> that completes when the cycle finishes or is cancelled.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when a device operation fails during the cycle.
    /// </exception>
    Task RunAsync(CancellationToken ct);

    /// <summary>
    /// Resets the simulation to its default state.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the reset request succeeds.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the reset request cannot be completed successfully.
    /// </exception>
    Task ResetAsync();
}
