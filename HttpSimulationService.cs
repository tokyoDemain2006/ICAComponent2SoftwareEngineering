using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class HttpSimulationService : IDisposable
{
    private readonly HttpClient _client;

    public HttpSimulationService(string baseAddress, string apiKey)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };

        _client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    public async Task<FanDto> GetFanState(int fanId)
    {
        var response = await _client.GetAsync($"api/fans/{fanId}/state");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get fan {fanId} state: {response.ReasonPhrase}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var fan = JsonSerializer.Deserialize<FanDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (fan is null)
        {
            throw new Exception($"Failed to parse fan {fanId} state.");
        }

        return fan;
    }

    public async Task<int> GetHeaterLevel(int heaterId)
    {
        var response = await _client.GetAsync($"api/heat/{heaterId}/level");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get heater {heaterId} level: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        if (!int.TryParse(content, out int level))
        {
            throw new Exception($"Failed to parse heater {heaterId} level.");
        }

        return level;
    }

    public async Task<string> GetSensor1Temperature()
    {
        var response = await _client.GetAsync("api/Sensor/sensor1");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get temperature from Sensor 1: {response.ReasonPhrase}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<int> GetSensor2Temperature()
    {
        var response = await _client.GetAsync("api/Sensor/sensor2");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get temperature from Sensor 2: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        if (!int.TryParse(content, out int temp))
        {
            throw new Exception("Failed to parse Sensor 2 temperature as an integer.");
        }

        return temp;
    }

    public async Task<decimal> GetSensor3Temperature()
    {
        var response = await _client.GetAsync("api/Sensor/sensor3");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get temperature from Sensor 3: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        if (!decimal.TryParse(content, out decimal temp))
        {
            throw new Exception("Failed to parse Sensor 3 temperature as a decimal.");
        }

        return temp;
    }

    public async Task<double> GetSensorTemperature(int sensorId)
    {
        var response = await _client.GetAsync($"api/sensor/{sensorId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get temperature from sensor {sensorId}: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        if (!double.TryParse(content, out double temp))
        {
            throw new Exception($"Failed to parse temperature from sensor {sensorId}.");
        }

        return temp;
    }

    public async Task SetHeaterLevel(int heaterId, int level)
    {
        var response = await _client.PostAsync(
            $"api/heat/{heaterId}",
            new StringContent(level.ToString(), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set heater level {heaterId}: {response.ReasonPhrase}");
        }
    }

    public async Task SetFanState(int fanId, bool isOn)
    {
        var response = await _client.PostAsync(
            $"api/fans/{fanId}",
            new StringContent(isOn.ToString().ToLower(), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set fan state for fan {fanId}: {response.ReasonPhrase}");
        }
    }

    public async Task Reset()
    {
        var response = await _client.PostAsync("api/Envo/reset", null);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to reset client state: {response.ReasonPhrase}");
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

public class FanDto
{
    public int Id { get; set; }
    public bool IsOn { get; set; }
}
