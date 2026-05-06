using Moq;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FanService"/>.
/// </summary>
public class FanServiceTests
{
    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FanService(null!));
    }

    [Fact]
    public void Constructor_ZeroFanCount_ThrowsArgumentOutOfRangeException()
    {
        var httpService = new Mock<IHttpService>();
        Assert.Throws<ArgumentOutOfRangeException>(() => new FanService(httpService.Object, 0));
    }

    [Fact]
    public async Task SetFanStateAsync_TurnOn_CallsExpectedEndpoint()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.PostAsync("api/fans/2", "true")).ReturnsAsync(string.Empty);
        var fanService = new FanService(httpService.Object);

        await fanService.SetFanStateAsync(2, true);

        httpService.VerifyAll();
    }

    [Fact]
    public async Task SetFanStateAsync_TurnOff_CallsExpectedEndpointWithFalse()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.PostAsync("api/fans/1", "false")).ReturnsAsync(string.Empty);
        var fanService = new FanService(httpService.Object);

        await fanService.SetFanStateAsync(1, false);

        httpService.VerifyAll();
    }

    [Fact]
    public async Task SetFanStateAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.PostAsync("api/fans/1", "true"))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var fanService = new FanService(httpService.Object);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => fanService.SetFanStateAsync(1, true));

        Assert.True(exception.Message.Contains("fan 1", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetFanStateAsync_ValidJson_ReturnsFanState()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.GetAsync("api/fans/3/state"))
            .ReturnsAsync("""{"id":3,"isOn":true}""");
        var fanService = new FanService(httpService.Object);

        var fan = await fanService.GetFanStateAsync(3);

        Assert.Equal(3, fan.Id);
        Assert.True(fan.IsOn);
    }

    [Fact]
    public async Task GetFanStateAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.GetAsync("api/fans/4/state"))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var fanService = new FanService(httpService.Object);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => fanService.GetFanStateAsync(4));

        Assert.True(exception.Message.Contains("fan 4", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetFanStateAsync_WhenJsonIsInvalid_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.GetAsync("api/fans/1/state"))
            .ReturnsAsync("{not-json}");
        var fanService = new FanService(httpService.Object);

        await Assert.ThrowsAsync<DeviceServiceException>(() => fanService.GetFanStateAsync(1));
    }

    [Fact]
    public async Task SetAllFansAsync_CallsEachFanEndpoint()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.PostAsync("api/fans/1", "false")).ReturnsAsync(string.Empty);
        httpService.Setup(service => service.PostAsync("api/fans/2", "false")).ReturnsAsync(string.Empty);
        var fanService = new FanService(httpService.Object, fanCount: 2);

        await fanService.SetAllFansAsync(false);

        httpService.VerifyAll();
    }

    [Fact]
    public async Task GetAllFanStatesAsync_ReturnsStateForEachFan()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/fans/1/state")).ReturnsAsync("""{"id":1,"isOn":true}""");
        httpService.Setup(service => service.GetAsync("api/fans/2/state")).ReturnsAsync("""{"id":2,"isOn":false}""");
        var fanService = new FanService(httpService.Object, fanCount: 2);

        var fans = (await fanService.GetAllFanStatesAsync()).ToList();

        Assert.Equal(2, fans.Count);
        Assert.Equal(1, fans[0].Id);
        Assert.Equal(2, fans[1].Id);
    }
}
