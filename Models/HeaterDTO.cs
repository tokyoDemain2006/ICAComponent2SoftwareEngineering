namespace UglyClient.Models;

/// <summary>
/// Data Transfer Object representing the state of a heater device as returned by the
/// <c>GET api/heat/{id}/level</c> endpoint.
/// </summary>
public class HeaterDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the heater.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the current heat level of the heater.
    /// The integer value corresponds to the power level reported by the API.
    /// </summary>
    public int Level { get; set; }
}
