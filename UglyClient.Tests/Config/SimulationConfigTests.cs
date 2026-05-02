using UglyClient.Config;

namespace UglyClient.Tests.Config;

public class SimulationConfigTests
{
    [Fact]
    public void Constructor_StoresAllProperties()
    {
        var config = new SimulationConfig(
            FanCount: 4,
            HeaterCount: 5,
            SensorCount: 3,
            BaseUrl: "https://api.example.com/",
            ApiKey: "secret-key");

        Assert.Equal(4, config.FanCount);
        Assert.Equal(5, config.HeaterCount);
        Assert.Equal(3, config.SensorCount);
        Assert.Equal("https://api.example.com/", config.BaseUrl);
        Assert.Equal("secret-key", config.ApiKey);
    }

    [Fact]
    public void Equality_TwoInstancesWithSameValues_AreEqual()
    {
        var a = new SimulationConfig(3, 3, 3, "http://localhost/", "key");
        var b = new SimulationConfig(3, 3, 3, "http://localhost/", "key");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentFanCount_AreNotEqual()
    {
        var a = new SimulationConfig(3, 3, 3, "http://localhost/", "key");
        var b = new SimulationConfig(2, 3, 3, "http://localhost/", "key");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void With_ChangingFanCount_PreservesOtherProperties()
    {
        var original = new SimulationConfig(3, 3, 3, "http://localhost/", "key");

        var modified = original with { FanCount = 6 };

        Assert.Equal(6, modified.FanCount);
        Assert.Equal(original.HeaterCount, modified.HeaterCount);
        Assert.Equal(original.SensorCount, modified.SensorCount);
        Assert.Equal(original.BaseUrl, modified.BaseUrl);
        Assert.Equal(original.ApiKey, modified.ApiKey);
    }

    [Fact]
    public void Equality_DifferentApiKey_AreNotEqual()
    {
        var a = new SimulationConfig(3, 3, 3, "http://localhost/", "key-a");
        var b = new SimulationConfig(3, 3, 3, "http://localhost/", "key-b");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void With_ChangingBaseUrl_UpdatesUrl()
    {
        var original = new SimulationConfig(3, 3, 3, "http://old/", "key");

        var modified = original with { BaseUrl = "http://new/" };

        Assert.Equal("http://new/", modified.BaseUrl);
        Assert.NotEqual(original, modified);
    }
}
