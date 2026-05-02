namespace UglyClient.Models;

/// <summary>DTO for fan state returned by <c>GET api/fans/{id}/state</c>.</summary>
public class FanDTO
{
    /// <summary>Fan identifier.</summary>
    public int Id { get; set; }

    /// <summary><c>true</c> if the fan is on; <c>false</c> if off.</summary>
    public bool IsOn { get; set; }
}
