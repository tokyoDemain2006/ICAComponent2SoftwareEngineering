using Moq;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="HeaterService"/>.
/// </summary>
public class HeaterServiceTests
{
    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new HeaterService(null!));
    }

    [Fact]
    public void Constructor_ZeroHeaterCount_ThrowsArgumentOutOfRangeException()
    {
        var httpService = new Mock<IHttpService>();
        Assert.Throws<ArgumentOutOfRangeException>(() => new HeaterService(httpService.Object, 0));
    }

    [Fact]
    public async Task SetHeaterLevelAsync_CallsExpectedEndpoint()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.PostAsync("api/heat/2", "5")).ReturnsAsync(string.Empty);
        var heaterService = new HeaterService(httpService.Object);

        await heaterService.SetHeaterLevelAsync(2, 5);

        httpService.VerifyAll();
    }

    [Fact]
    public async Task SetHeaterLevelAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.PostAsync("api/heat/1", "3"))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var heaterService = new HeaterService(httpService.Object);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => heaterService.SetHeaterLevelAsync(1, 3));

        Assert.True(exception.Message.Contains("heater 1", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetHeaterLevelAsync_ValidIntegerResponse_ReturnsLevel()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/heat/3/level")).ReturnsAsync("4");
        var heaterService = new HeaterService(httpService.Object);

        var level = await heaterService.GetHeaterLevelAsync(3);

        Assert.Equal(4, level);
    }

    [Fact]
    public async Task GetHeaterLevelAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.GetAsync("api/heat/2/level"))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var heaterService = new HeaterService(httpService.Object);

        var exception = await Assert.ThrowsAsync<DeviceServiceException>(() => heaterService.GetHeaterLevelAsync(2));

        Assert.True(exception.Message.Contains("heater 2", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetHeaterLevelAsync_WhenResponseIsInvalid_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/heat/1/level")).ReturnsAsync("not-a-number");
        var heaterService = new HeaterService(httpService.Object);

        await Assert.ThrowsAsync<DeviceServiceException>(() => heaterService.GetHeaterLevelAsync(1));
    }

    [Fact]
    public async Task SetAllHeatersAsync_CallsEachHeaterEndpoint()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.PostAsync("api/heat/1", "0")).ReturnsAsync(string.Empty);
        httpService.Setup(service => service.PostAsync("api/heat/2", "0")).ReturnsAsync(string.Empty);
        var heaterService = new HeaterService(httpService.Object, heaterCount: 2);

        await heaterService.SetAllHeatersAsync(0);

        httpService.VerifyAll();
    }

    [Fact]
    public async Task GetAllHeaterLevelsAsync_ReturnsLevelForEachHeater()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.GetAsync("api/heat/1/level")).ReturnsAsync("2");
        httpService.Setup(service => service.GetAsync("api/heat/2/level")).ReturnsAsync("5");
        var heaterService = new HeaterService(httpService.Object, heaterCount: 2);

        var levels = (await heaterService.GetAllHeaterLevelsAsync()).ToList();

        Assert.Equal([2, 5], levels);
    }
}
