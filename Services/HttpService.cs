using System.Text;

namespace UglyClient.Services;

/// <summary>Shared HTTP service that owns the <see cref="HttpClient"/> and routes all API calls.</summary>
public sealed class HttpService : IHttpService, IDisposable
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string JsonMediaType = "application/json";
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>Constructs and configures an <see cref="HttpClient"/> with the supplied base URL and API key.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="baseUrl"/> or <paramref name="apiKey"/> is null or whitespace.</exception>
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
        _ownsHttpClient = true;
    }

    /// <summary>Wraps an existing <see cref="HttpClient"/> without taking ownership of its lifetime.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public HttpService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = false;
    }

    /// <inheritdoc/>
    public async Task<string> GetAsync(string endpoint)
    {
        return await SendAsync(() => new HttpRequestMessage(HttpMethod.Get, endpoint));
    }

    /// <inheritdoc/>
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

    /// <summary>Releases the underlying <see cref="HttpClient"/> if owned by this instance.</summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
