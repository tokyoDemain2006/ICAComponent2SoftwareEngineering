using Moq;
using UglyClient.Controllers;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SimulationService"/>.
/// </summary>
public class SimulationServiceTests
{
    private static TemperatureController CreateMockController()
    {
        var fanService = new Mock<IFanService>(MockBehavior.Loose);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Loose);
        var sensorService = new Mock<ISensorService>(MockBehavior.Loose);
        return new Mock<TemperatureController>(fanService.Object, heaterService.Object, sensorService.Object).Object;
    }

    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SimulationService(null!, CreateMockController()));
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
        var simulationService = new SimulationService(httpService.Object, CreateMockController());

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
        var simulationService = new SimulationService(httpService.Object, CreateMockController());

        await Assert.ThrowsAsync<DeviceServiceException>(() => simulationService.ResetAsync());
    }

    [Fact]
    public async Task ResetAsync_WhenHttpFails_ErrorMessageIndicatesReset()
    {
        // The service wraps low-level errors with a context-specific message so callers
        // know the reset operation failed, not some unrelated call.
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService
            .Setup(service => service.PostAsync("api/Envo/reset", string.Empty))
            .ThrowsAsync(new DeviceServiceException("transport failure"));
        var simulationService = new SimulationService(httpService.Object, CreateMockController());

        var ex = await Assert.ThrowsAsync<DeviceServiceException>(() => simulationService.ResetAsync());

        Assert.Contains("reset", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_DelegatesToTemperatureController()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Loose);
        var fanService = new Mock<IFanService>(MockBehavior.Loose);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Loose);
        var sensorService = new Mock<ISensorService>(MockBehavior.Loose);
        var controllerMock = new Mock<TemperatureController>(fanService.Object, heaterService.Object, sensorService.Object);
        controllerMock
            .Setup(c => c.RunFullCycleAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var simulationService = new SimulationService(httpService.Object, controllerMock.Object);
        await simulationService.RunAsync(CancellationToken.None);

        controllerMock.Verify(c => c.RunFullCycleAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RunAsync_PassesCancellationTokenToController()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Loose);
        var fanService = new Mock<IFanService>(MockBehavior.Loose);
        var heaterService = new Mock<IHeaterService>(MockBehavior.Loose);
        var sensorService = new Mock<ISensorService>(MockBehavior.Loose);
        var controllerMock = new Mock<TemperatureController>(fanService.Object, heaterService.Object, sensorService.Object);
        using var cts = new CancellationTokenSource();
        controllerMock
            .Setup(c => c.RunFullCycleAsync(cts.Token))
            .Returns(Task.CompletedTask);

        var simulationService = new SimulationService(httpService.Object, controllerMock.Object);
        await simulationService.RunAsync(cts.Token);

        controllerMock.Verify(c => c.RunFullCycleAsync(cts.Token), Times.Once);
    }
}
