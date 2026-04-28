namespace UglyClient.Services;

/// <summary>
/// Defines the contract for the shared HTTP communication layer used by all service classes.
/// Implementing classes are responsible for owning and configuring the underlying HTTP client,
/// including base URL and API key injection.
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// Sends an HTTP GET request to the specified endpoint and returns the response body as a string.
    /// </summary>
    /// <param name="endpoint">The relative endpoint path (e.g. "api/fans/1").</param>
    /// <returns>The response content as a raw string.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the request cannot be completed successfully.
    /// </exception>
    Task<string> GetAsync(string endpoint);

    /// <summary>
    /// Sends an HTTP POST request to the specified endpoint with the supplied JSON body,
    /// and returns the response body as a string.
    /// </summary>
    /// <param name="endpoint">The relative endpoint path (e.g. "api/fans/1/state").</param>
    /// <param name="body">The JSON-serialised request body to send.</param>
    /// <returns>The response content as a raw string.</returns>
    /// <exception cref="DeviceServiceException">
    /// Thrown when the request cannot be completed successfully.
    /// </exception>
    Task<string> PostAsync(string endpoint, string body);
}
