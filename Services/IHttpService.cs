namespace UglyClient.Services;

/// <summary>Contract for the shared HTTP communication layer used by all service classes.</summary>
public interface IHttpService
{
    /// <summary>Sends a GET request and returns the response body as a string.</summary>
    /// <param name="endpoint">Relative endpoint path (e.g. <c>api/fans/1</c>).</param>
    /// <exception cref="DeviceServiceException">Thrown when the request fails.</exception>
    Task<string> GetAsync(string endpoint);

    /// <summary>Sends a POST request with <paramref name="body"/> and returns the response body.</summary>
    /// <param name="endpoint">Relative endpoint path (e.g. <c>api/fans/1/state</c>).</param>
    /// <param name="body">JSON-serialised request body.</param>
    /// <exception cref="DeviceServiceException">Thrown when the request fails.</exception>
    Task<string> PostAsync(string endpoint, string body);
}
