using Moq;
using UglyClient.Controllers;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SimulationService"/>.
/// </summary>
public class SimulationServiceTests
{
    private static TemperatureController CreateController()
    {
        var fanService = new Mock<IFanService>(MockBehavior.Loose);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Loose);
        var sensorService = new Mock<ISensorService>(MockBehavior.Loose);
        return new TemperatureController(fanService.Object, heaterService.Object, sensorService.Object);
    }

    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SimulationService(null!, CreateController()));
    }

    [Fact]
    public void Constructor_NullController_ThrowsArgumentNullException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Loose);
        Assert.Throws<ArgumentNullException>(() => new SimulationService(httpService.Object, null!));
    }

    [Fact]
    public async Task ResetAsync_CallsResetEndpoint()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.PostAsync("api/Envo/reset", string.Empty)).ReturnsAsync(string.Empty);
        var simulationService = new SimulationService(httpService.Object, CreateController());

        await simulationService.ResetAsync();

        httpService.VerifyAll();
    }

    [Fact]
    public async Task ResetAsync_WhenHttpFails_ThrowsDeviceServiceException()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.PostAsync("api/Envo/reset", string.Empty))
            .ThrowsAsync(new DeviceServiceException("The simulation service is unavailable right now."));
        var simulationService = new SimulationService(httpService.Object, CreateController());

        await Assert.ThrowsAsync<DeviceServiceException>(() => simulationService.ResetAsync());
    }

    [Fact]
    public async Task ResetAsync_WhenHttpFails_ErrorMessageIndicatesReset()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.PostAsync("api/Envo/reset", string.Empty))
            .ThrowsAsync(new DeviceServiceException("transport failure"));
        var simulationService = new SimulationService(httpService.Object, CreateController());

        var ex = await Assert.ThrowsAsync<DeviceServiceException>(() => simulationService.ResetAsync());

        Assert.Contains("reset", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
