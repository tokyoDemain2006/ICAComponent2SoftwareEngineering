using UglyClient.Models;

namespace UglyClient.Services;

/// <summary>Contract for all fan-related operations against the simulation API.</summary>
public interface IFanService
{
    /// <summary>Turns a single fan on or off.</summary>
    /// <param name="fanId">One-based fan identifier.</param>
    /// <param name="isOn"><c>true</c> to switch on; <c>false</c> to switch off.</param>
    /// <exception cref="DeviceServiceException">Thrown when the update fails.</exception>
    Task SetFanStateAsync(int fanId, bool isOn);

    /// <summary>Retrieves the current on/off state of a single fan.</summary>
    /// <param name="fanId">One-based fan identifier.</param>
    /// <exception cref="DeviceServiceException">Thrown when the state cannot be retrieved.</exception>
    Task<FanDTO> GetFanStateAsync(int fanId);

    /// <summary>Sets every managed fan to the same on/off state.</summary>
    /// <param name="isOn"><c>true</c> to switch all fans on; <c>false</c> to switch all off.</param>
    /// <exception cref="DeviceServiceException">Thrown when any individual fan update fails.</exception>
    Task SetAllFansAsync(bool isOn);

    /// <summary>Retrieves the current on/off state of every managed fan.</summary>
    /// <exception cref="DeviceServiceException">Thrown when any individual fan state cannot be retrieved.</exception>
    Task<IEnumerable<FanDTO>> GetAllFanStatesAsync();
}
