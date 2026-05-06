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
    private const string ApiKeyHeaderName = "X-Api-Key";

    private const string JsonMediaType = "application/json";

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initialises a new instance of <see cref="HttpService"/>, constructing and configuring
    /// the underlying <see cref="HttpClient"/> with the supplied base URL and API key.
    /// </summary>
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
    /// Initialises a new instance of <see cref="HttpService"/> around an existing
    /// <see cref="HttpClient"/>.
    /// </summary>
    public HttpService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified endpoint and returns the response body as a string.
    /// </summary>
    public async Task<string> GetAsync(string endpoint)
    {
        return await SendAsync(() => new HttpRequestMessage(HttpMethod.Get, endpoint));
    }

    /// <summary>
    /// Sends an HTTP POST request to the specified endpoint with the supplied JSON body
    /// and returns the response body as a string.
    /// </summary>
    public async Task<string> PostAsync(string endpoint, string body)
    {
        return await SendAsync(() => new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(body, Encoding.UTF8, JsonMediaType)
        });
    }

    private async Task<string> SendAsync(Func<HttpRequestMessage> createRequest)
    {
        try
        {
            using var request = createRequest();
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new DeviceServiceException("The simulation service is unavailable right now.");
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (DeviceServiceException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new DeviceServiceException("Unable to reach the simulation service right now.", ex);
        }
    }

    /// <summary>
    /// Releases the underlying <see cref="HttpClient"/> and any associated resources.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
