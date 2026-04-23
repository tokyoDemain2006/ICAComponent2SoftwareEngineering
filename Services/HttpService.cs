using System.Text;

namespace UglyClient.Services;

/// <summary>
/// Shared HTTP communication service that owns and configures the underlying <see cref="HttpClient"/>.
/// This is the single place in the codebase where <see cref="HttpClient"/> is constructed.
/// All service classes (<c>FanService</c>, <c>HeaterService</c>, <c>SensorService</c>,
/// <c>SimulationService</c>) receive an instance of this class via constructor injection and
/// never create their own <see cref="HttpClient"/>.
/// </summary>
public sealed class HttpService : IHttpService, IDisposable
{
    /// <summary>
    /// The name of the HTTP request header used for API key authentication.
    /// </summary>
    private const string ApiKeyHeaderName = "X-Api-Key";

    /// <summary>
    /// The media type sent in the Content-Type header for POST requests containing a JSON body.
    /// </summary>
    private const string JsonMediaType = "application/json";

    /// <summary>
    /// The underlying <see cref="HttpClient"/> owned exclusively by this service.
    /// It is configured once in the constructor and reused for the lifetime of this instance.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initialises a new instance of <see cref="HttpService"/>, constructing and configuring
    /// the underlying <see cref="HttpClient"/> with the supplied base URL and API key.
    /// </summary>
    /// <param name="baseUrl">
    /// The base URL of the API (e.g. <c>https://envrosym.azurewebsites.net/</c>).
    /// All relative endpoint paths supplied to <see cref="GetAsync"/> and
    /// <see cref="PostAsync"/> are resolved against this URL.
    /// </param>
    /// <param name="apiKey">
    /// The API key injected into every outgoing request via the <c>X-Api-Key</c> header.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="baseUrl"/> or <paramref name="apiKey"/> is null or whitespace.
    /// </exception>
    public HttpService(string baseUrl, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL must not be null or empty.", nameof(baseUrl));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key must not be null or empty.", nameof(apiKey));

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        _httpClient.DefaultRequestHeaders.Add(ApiKeyHeaderName, apiKey);
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified endpoint and returns the response body as a string.
    /// </summary>
    /// <param name="endpoint">The relative endpoint path (e.g. <c>api/fans/1</c>).</param>
    /// <returns>The response content as a raw string.</returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP response indicates a failure status code.
    /// </exception>
    public async Task<string> GetAsync(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Sends an HTTP POST request to the specified endpoint with the supplied JSON body
    /// and returns the response body as a string.
    /// </summary>
    /// <param name="endpoint">The relative endpoint path (e.g. <c>api/fans/1/state</c>).</param>
    /// <param name="body">The JSON-serialised request body to send.</param>
    /// <returns>The response content as a raw string.</returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP response indicates a failure status code.
    /// </exception>
    public async Task<string> PostAsync(string endpoint, string body)
    {
        var content = new StringContent(body, Encoding.UTF8, JsonMediaType);
        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Releases the underlying <see cref="HttpClient"/> and any associated resources.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
