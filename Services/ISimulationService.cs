namespace UglyClient.Services;

/// <summary>
/// Defines the contract for simulation-wide operations against the environment API.
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Runs the end-to-end temperature-control cycle.
    /// </summary>
    Task RunAsync(CancellationToken ct);

    /// <summary>
    /// Resets the simulation to its default state.
    /// </summary>
    Task ResetAsync();
}
