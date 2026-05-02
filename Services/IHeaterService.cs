namespace UglyClient.Services;

/// <summary>Contract for all heater-related operations against the simulation API.</summary>
public interface IHeaterService
{
    /// <summary>Sets the heat level of a single heater.</summary>
    /// <param name="heaterId">One-based heater identifier.</param>
    /// <param name="level">Desired heat level (0–5, where 0 is off).</param>
    /// <exception cref="DeviceServiceException">Thrown when the update fails.</exception>
    Task SetHeaterLevelAsync(int heaterId, int level);

    /// <summary>Retrieves the current heat level of a single heater.</summary>
    /// <param name="heaterId">One-based heater identifier.</param>
    /// <returns>Current heat level (0–5).</returns>
    /// <exception cref="DeviceServiceException">Thrown when the level cannot be retrieved.</exception>
    Task<int> GetHeaterLevelAsync(int heaterId);

    /// <summary>Sets every managed heater to the same level.</summary>
    /// <param name="level">Heat level to apply to all heaters (0–5).</param>
    /// <exception cref="DeviceServiceException">Thrown when any individual heater update fails.</exception>
    Task SetAllHeatersAsync(int level);

    /// <summary>Retrieves the current heat level of every managed heater.</summary>
    /// <exception cref="DeviceServiceException">Thrown when any individual heater level cannot be retrieved.</exception>
    Task<IEnumerable<int>> GetAllHeaterLevelsAsync();
}
