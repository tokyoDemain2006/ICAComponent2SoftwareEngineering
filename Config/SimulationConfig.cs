namespace UglyClient.Config;

/// <summary>
/// Holds all configuration values for the simulation client, eliminating magic numbers
/// and hardcoded strings from the rest of the codebase.
/// </summary>
/// <remarks>
/// A single instance is constructed in <c>Program.Main</c> and passed via constructor
/// injection to every service that requires it.
/// </remarks>
/// <param name="FanCount">Total number of fans managed by the simulation.</param>
/// <param name="HeaterCount">Total number of heaters managed by the simulation.</param>
/// <param name="SensorCount">Total number of temperature sensors managed by the simulation.</param>
/// <param name="BaseUrl">Base URL of the environment simulation API (including trailing slash).</param>
/// <param name="ApiKey">API key sent in the <c>X-Api-Key</c> request header.</param>
public record SimulationConfig(
    int FanCount,
    int HeaterCount,
    int SensorCount,
    string BaseUrl,
    string ApiKey
);
