namespace UglyClient.Interfaces;

/// <summary>
/// Defines the contract for a temperature-control algorithm used during a simulation phase.
/// </summary>
public interface ITemperatureControlStrategy
{
    /// <summary>
    /// Executes the temperature-control algorithm for the supplied phase settings.
    /// </summary>
    Task<double> ExecuteAsync(
        double currentTemperature,
        double targetTemperature,
        int durationSeconds,
        CancellationToken cancellationToken = default);
}
