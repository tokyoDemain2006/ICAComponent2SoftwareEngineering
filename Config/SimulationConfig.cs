namespace UglyClient.Config;

/// <summary>
/// Holds all configuration values for the simulation client, eliminating magic numbers
/// and hardcoded strings from the rest of the codebase.
/// </summary>
public record SimulationConfig(
    int FanCount,
    int HeaterCount,
    int SensorCount,
    string BaseUrl,
    string ApiKey
);
