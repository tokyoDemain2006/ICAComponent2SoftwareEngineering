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
    Task SetFanStateAsync(int fanId, bool isOn);

    /// <summary>
    /// Sends a GET request to retrieve the current on/off state of a single fan.
    /// </summary>
    Task<FanDTO> GetFanStateAsync(int fanId);

    /// <summary>
    /// Sets every managed fan to the same on/off state by calling
    /// <see cref="SetFanStateAsync"/> for each fan in sequence.
    /// </summary>
    Task SetAllFansAsync(bool isOn);

    /// <summary>
    /// Retrieves the current on/off state of every managed fan.
    /// </summary>
    Task<IEnumerable<FanDTO>> GetAllFanStatesAsync();
}
