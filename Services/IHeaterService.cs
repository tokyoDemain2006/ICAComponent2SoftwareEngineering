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
    /// <param name="heaterId">The one-based identifier of the heater to control.</param>
    /// <param name="level">The desired heat level (0–5, where 0 is off).</param>
    /// <returns>A <see cref="Task"/> that completes when the API acknowledges the command.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the heater update cannot be completed successfully.
    /// </exception>
    Task SetHeaterLevelAsync(int heaterId, int level);

    /// <summary>
    /// Sends a GET request to retrieve the current heat level of a single heater.
    /// </summary>
    /// <param name="heaterId">The one-based identifier of the heater to query.</param>
    /// <returns>
    /// A <see cref="Task{T}"/> whose result is the current heat level (0–5) as an
    /// <see cref="int"/>.
    /// </returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the heater level cannot be retrieved successfully.
    /// </exception>
    Task<int> GetHeaterLevelAsync(int heaterId);

    /// <summary>
    /// Sets every managed heater to the same level by calling
    /// <see cref="SetHeaterLevelAsync"/> for each heater in sequence.
    /// </summary>
    /// <param name="level">The heat level to apply to all heaters (0–5).</param>
    /// <returns>A <see cref="Task"/> that completes when all heaters have been updated.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when any individual heater update cannot be completed successfully.
    /// </exception>
    Task SetAllHeatersAsync(int level);

    /// <summary>
    /// Retrieves the current heat level of every managed heater.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{T}"/> whose result is an <see cref="IEnumerable{T}"/> of
    /// <see cref="int"/> values, one per heater, ordered by ascending heater ID.
    /// </returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when any individual heater level cannot be retrieved successfully.
    /// </exception>
    Task<IEnumerable<int>> GetAllHeaterLevelsAsync();
}
