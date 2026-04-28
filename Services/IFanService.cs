using UglyClient.Models;

namespace UglyClient.Services;

/// <summary>
/// Defines the contract for all fan-related operations against the environment simulation API.
/// Implementations must encapsulate every HTTP call and all fan-related error handling so that
/// no <see cref="System.Net.Http.HttpClient"/> usage for fans exists outside this service.
/// </summary>
public interface IFanService
{
    /// <summary>
    /// Sends a POST request to turn a single fan on or off.
    /// </summary>
    /// <param name="fanId">The one-based identifier of the fan to control.</param>
    /// <param name="isOn"><c>true</c> to switch the fan on; <c>false</c> to switch it off.</param>
    /// <returns>A <see cref="Task"/> that completes when the API acknowledges the command.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the fan update cannot be completed successfully.
    /// </exception>
    Task SetFanStateAsync(int fanId, bool isOn);

    /// <summary>
    /// Sends a GET request to retrieve the current on/off state of a single fan.
    /// </summary>
    /// <param name="fanId">The one-based identifier of the fan to query.</param>
    /// <returns>
    /// A <see cref="Task{FanDTO}"/> whose result is the <see cref="FanDTO"/>
    /// describing the fan's current state.
    /// </returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the fan state cannot be retrieved successfully.
    /// </exception>
    Task<FanDTO> GetFanStateAsync(int fanId);

    /// <summary>
    /// Sets every managed fan to the same on/off state by calling
    /// <see cref="SetFanStateAsync"/> for each fan in sequence.
    /// </summary>
    /// <param name="isOn"><c>true</c> to switch all fans on; <c>false</c> to switch them all off.</param>
    /// <returns>A <see cref="Task"/> that completes when all fans have been updated.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when any individual fan update cannot be completed successfully.
    /// </exception>
    Task SetAllFansAsync(bool isOn);

    /// <summary>
    /// Retrieves the current on/off state of every managed fan.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{T}"/> whose result is an <see cref="IEnumerable{FanDTO}"/>
    /// containing one <see cref="FanDTO"/> per fan, ordered by ascending fan ID.
    /// </returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when any individual fan state cannot be retrieved successfully.
    /// </exception>
    Task<IEnumerable<FanDTO>> GetAllFanStatesAsync();
}
