namespace UglyClient.Models;

/// <summary>
/// Data Transfer Object representing the state of a fan device as returned by the
/// <c>GET api/fans/{id}/state</c> endpoint.
/// </summary>
public class FanDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the fan.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the fan is currently switched on.
    /// <c>true</c> means the fan is on; <c>false</c> means the fan is off.
    /// </summary>
    public bool IsOn { get; set; }
}
