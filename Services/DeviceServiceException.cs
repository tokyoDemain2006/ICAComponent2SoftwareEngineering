namespace UglyClient.Services;

/// <summary>
/// Represents a service-layer failure while communicating with or interpreting responses from
/// the simulation API.
/// </summary>
public sealed class DeviceServiceException : Exception
{
    /// <summary>
    /// Initialises a new instance of <see cref="DeviceServiceException"/>.
    /// </summary>
    public DeviceServiceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
