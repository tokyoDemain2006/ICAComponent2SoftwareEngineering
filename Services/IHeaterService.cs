namespace UglyClient.Services;

/// <summary>
/// Defines the contract for all heater-related operations against the environment simulation API.
/// Implementations must encapsulate every HTTP call and all heater-related error handling so that
/// no <see cref="System.Net.Http.HttpClient"/> usage for heaters exists outside this service.
/// </summary>
public interface IHeaterService
{
    /// <summary>
    /// Sends a POST request to set the heat level of a single heater.
    /// </summary>
    Task SetHeaterLevelAsync(int heaterId, int level);

    /// <summary>
    /// Sends a GET request to retrieve the current heat level of a single heater.
    /// </summary>
    Task<int> GetHeaterLevelAsync(int heaterId);

    /// <summary>
    /// Sets every managed heater to the same level by calling
    /// <see cref="SetHeaterLevelAsync"/> for each heater in sequence.
    /// </summary>
    Task SetAllHeatersAsync(int level);

    /// <summary>
    /// Retrieves the current heat level of every managed heater.
    /// </summary>
    Task<IEnumerable<int>> GetAllHeaterLevelsAsync();
}
