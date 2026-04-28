using Moq;
using UglyClient.Services;

namespace UglyClient.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SimulationService"/>.
/// </summary>
public class SimulationServiceTests
{
    [Fact]
    public void Constructor_NullHttpService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SimulationService(null!));
    }

    [Fact]
    public async Task ResetAsync_CallsResetEndpoint()
    {
        var httpService = new Mock<IHttpService>(MockBehavior.Strict);
        httpService.Setup(service => service.PostAsync("api/Envo/reset", string.Empty)).ReturnsAsync(string.Empty);
        var simulationService = new SimulationService(httpService.Object);

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
        var simulationService = new SimulationService(httpService.Object);

        await Assert.ThrowsAsync<DeviceServiceException>(() => simulationService.ResetAsync());
    }
}
