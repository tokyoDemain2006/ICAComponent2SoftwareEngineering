namespace UglyClient.Interfaces;

/// <summary>Contract for a temperature-control algorithm used during a simulation phase.</summary>
public interface ITemperatureControlStrategy
{
    /// <summary>Executes the temperature-control algorithm for the supplied phase settings.</summary>
    /// <param name="currentTemperature">Temperature observed at the start of the phase.</param>
    /// <param name="targetTemperature">Desired target temperature for the phase.</param>
    /// <param name="durationSeconds">Maximum number of one-second iterations to perform.</param>
    /// <param name="cancellationToken">Token used to cancel the phase while running.</param>
    /// <returns>The last temperature observed before the phase completed.</returns>
    Task<double> ExecuteAsync(
        double currentTemperature,
        double targetTemperature,
        int durationSeconds,
        CancellationToken cancellationToken = default);
}
