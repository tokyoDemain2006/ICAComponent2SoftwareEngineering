namespace UglyClient.Services;

/// <summary>
/// Defines the contract for simulation-wide operations against the environment API.
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Resets the simulation to its default state.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the reset request succeeds.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the reset request cannot be completed successfully.
    /// </exception>
    Task ResetAsync();
}
