namespace UglyClient.Interfaces;

/// <summary>
/// Defines the contract for a temperature-control algorithm used during a simulation phase.
/// </summary>
public interface ITemperatureControlStrategy
{
    /// <summary>
    /// Executes the temperature-control algorithm for the supplied phase settings.
    /// </summary>
    /// <param name="currentTemperature">The temperature observed at the start of the phase.</param>
    /// <param name="targetTemperature">The desired target temperature for the phase.</param>
    /// <param name="durationSeconds">The maximum number of one-second iterations to perform.</param>
    /// <param name="cancellationToken">The token used to cancel the phase while it is running.</param>
    /// <returns>The last temperature observed before the phase completed.</returns>
    Task<double> ExecuteAsync(
        double currentTemperature,
        double targetTemperature,
        int durationSeconds,
        CancellationToken cancellationToken = default);
}
