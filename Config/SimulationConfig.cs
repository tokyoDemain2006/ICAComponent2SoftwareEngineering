namespace UglyClient.Config;

/// <summary>Immutable configuration for the simulation client.</summary>
/// <param name="FanCount">Total number of fans in the simulation.</param>
/// <param name="HeaterCount">Total number of heaters in the simulation.</param>
/// <param name="SensorCount">Total number of temperature sensors in the simulation.</param>
/// <param name="BaseUrl">Base URL of the environment simulation API (including trailing slash).</param>
/// <param name="ApiKey">API key sent in the <c>X-Api-Key</c> request header.</param>
public record SimulationConfig(
    int FanCount,
    int HeaterCount,
    int SensorCount,
    string BaseUrl,
    string ApiKey
);
