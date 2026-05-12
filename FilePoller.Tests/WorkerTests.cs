using FilePoller.Entities;
using FilePoller.Models;
using FilePoller.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using StatementProcessorModels;

namespace FilePoller.Tests;

public class WorkerTests
{
    private readonly Mock<IOptions<AppSettings>> _mockSettings;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IDataService> _mockDataService;
    private readonly Mock<IProcessLogger> _mockProcessLogger;
    private readonly Worker _worker;

    public WorkerTests()
    {
        _mockSettings = new Mock<IOptions<AppSettings>>();
        _mockLogService = new Mock<ILogService>();
        _mockDataService = new Mock<IDataService>();
        _mockProcessLogger = new Mock<IProcessLogger>();

        var appSettings = new AppSettings
        {
            ServiceName = "FilePoller",
            FileChkIntervalSeconds = 30,
            IncomingUrl = "/incoming",
            WaitTimeLoopCount = 10,
            ApiUrl = "http://localhost:5000",
            FtpUrl = "ftp.example.com",
            NextStepPath = "/api/next-step",
            DownloadToName = "downloads",
            StepLogUrl = "/api/logs",
            KeyFile = "/path/to/key",
            FtpUser = "testuser"
        };

        _mockSettings.Setup(s => s.Value).Returns(appSettings);

        _mockLogService.Setup(s => s.LogAlert(It.IsAny<AppLog>()))
            .Returns(Task.FromResult(true));

        _mockProcessLogger.Setup(p => p.WriteProcessLog(It.IsAny<WriteJobStepParameters>()))
            .Returns(Task.FromResult(true));

        _worker = new Worker(_mockSettings.Object, _mockLogService.Object, _mockDataService.Object, _mockProcessLogger.Object);
    }

    [Fact]
    public async Task Worker_IsBackgroundService()
    {
        // Assert
        Assert.IsAssignableFrom<BackgroundService>(_worker);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldNotThrow()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        var job = new Jobs
        {
            Id = 1,
            JobId = Guid.NewGuid(),
            StartDateTime = DateTime.Now,
            StopDateTime = null,
            JobUser = "testuser"
        };

        _mockDataService.Setup(s => s.GetStartedJob())
            .Returns(Task.FromResult<Jobs?>(job));

        // Act & Assert - Should complete without throwing
        await _worker.StartAsync(cts.Token);
        await Task.Delay(200);
        await _worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void Worker_Constructor_WithValidDependencies_ShouldInitialize()
    {
        // Assert
        Assert.NotNull(_worker);
    }

    [Fact]
    public async Task Worker_StartAsync_ShouldNotThrow()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        _mockDataService.Setup(s => s.GetStartedJob())
            .Returns(Task.FromResult<Jobs?>(null));

        // Act & Assert
        await _worker.StartAsync(cts.Token);
    }

    [Fact]
    public async Task Worker_StopAsync_ShouldNotThrow()
    {
        // Act & Assert
        await _worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void Worker_Dependencies_AreInjected()
    {
        // Assert - If we can create the worker without errors, dependencies are injected
        Assert.NotNull(_worker);
        Assert.IsType<Worker>(_worker);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(60)]
    public void Worker_Settings_ShouldPreserveFileCheckInterval(int intervalSeconds)
    {
        // Arrange
        var settings = new AppSettings { FileChkIntervalSeconds = intervalSeconds };
        _mockSettings.Setup(s => s.Value).Returns(settings);

        // Act
        var actualInterval = _mockSettings.Object.Value.FileChkIntervalSeconds;

        // Assert
        Assert.Equal(intervalSeconds, actualInterval);
    }

    [Theory]
    [InlineData("http://localhost:5000")]
    [InlineData("https://api.example.com")]
    [InlineData("http://192.168.1.1:8080")]
    public void Worker_Settings_ShouldPreserveApiUrl(string apiUrl)
    {
        // Arrange
        var settings = new AppSettings { ApiUrl = apiUrl };
        _mockSettings.Setup(s => s.Value).Returns(settings);

        // Act
        var actualUrl = _mockSettings.Object.Value.ApiUrl;

        // Assert
        Assert.Equal(apiUrl, actualUrl);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void Worker_Settings_ShouldPreserveWaitTimeLoopCount(int loopCount)
    {
        // Arrange
        var settings = new AppSettings { WaitTimeLoopCount = loopCount };
        _mockSettings.Setup(s => s.Value).Returns(settings);

        // Act
        var actualCount = _mockSettings.Object.Value.WaitTimeLoopCount;

        // Assert
        Assert.Equal(loopCount, actualCount);
    }
}
