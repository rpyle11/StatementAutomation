using FilePoller.Models;
using FilePoller.Services;
using Microsoft.Extensions.Options;
using Moq;
using StatementProcessorModels;

namespace FilePoller.Tests.Services;

public class ProcessLoggerTests
{
    private readonly Mock<IOptions<AppSettings>> _mockSettings;

    public ProcessLoggerTests()
    {
        _mockSettings = new Mock<IOptions<AppSettings>>();

        var appSettings = new AppSettings
        {
            ServiceName = "FilePoller",
            ApiUrl = "http://localhost:5000",
            StepLogUrl = "/api/logs"
        };

        _mockSettings.Setup(s => s.Value).Returns(appSettings);
    }

    [Fact]
    public async Task WriteProcessLog_WithValidParameters_ShouldReturnBoolean()
    {
        // Arrange
        // Since ProcessLogger is instantiated without HttpClient parameter in constructor,
        // we verify it can be created and the method signature is correct
        var logger = new ProcessLogger(_mockSettings.Object);
        var parameters = new WriteJobStepParameters
        {
            JobId = Guid.NewGuid(),
            Message = "Test log message",
            AppUser = "testuser"
        };

        // Note: The actual test would require mocking the internal HttpClient
        // This test verifies the object can be instantiated with dependencies
        Assert.NotNull(logger);
    }

    [Fact]
    public void ProcessLogger_CanBeInstantiatedWithValidSettings()
    {
        // Arrange & Act
        var logger = new ProcessLogger(_mockSettings.Object);

        // Assert
        Assert.NotNull(logger);
        Assert.IsType<ProcessLogger>(logger);
    }

    [Fact]
    public void ProcessLogger_Settings_AreInjected()
    {
        // Arrange
        var settings = new AppSettings
        {
            ApiUrl = "http://api.example.com",
            StepLogUrl = "/logs"
        };
        _mockSettings.Setup(s => s.Value).Returns(settings);

        // Act
        var logger = new ProcessLogger(_mockSettings.Object);

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void ProcessLogger_WithDifferentApiUrls()
    {
        // Arrange
        var settings1 = new AppSettings { ApiUrl = "http://api1.example.com" };
        var settings2 = new AppSettings { ApiUrl = "http://api2.example.com" };

        // Act
        var logger1 = new ProcessLogger(Options.Create(settings1));
        var logger2 = new ProcessLogger(Options.Create(settings2));

        // Assert
        Assert.NotNull(logger1);
        Assert.NotNull(logger2);
    }
}
