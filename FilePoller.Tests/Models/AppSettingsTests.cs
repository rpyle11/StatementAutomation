using FilePoller.Models;

namespace FilePoller.Tests.Models;

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_ServiceName_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { ServiceName = "TestService" };

        // Act & Assert
        Assert.Equal("TestService", settings.ServiceName);
    }

    [Fact]
    public void AppSettings_FileChkIntervalSeconds_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { FileChkIntervalSeconds = 30 };

        // Act & Assert
        Assert.Equal(30, settings.FileChkIntervalSeconds);
    }

    [Fact]
    public void AppSettings_IncomingUrl_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { IncomingUrl = "/incoming" };

        // Act & Assert
        Assert.Equal("/incoming", settings.IncomingUrl);
    }

    [Fact]
    public void AppSettings_WaitTimeLoopCount_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { WaitTimeLoopCount = 10 };

        // Act & Assert
        Assert.Equal(10, settings.WaitTimeLoopCount);
    }

    [Fact]
    public void AppSettings_ApiUrl_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { ApiUrl = "http://localhost:5000" };

        // Act & Assert
        Assert.Equal("http://localhost:5000", settings.ApiUrl);
    }

    [Fact]
    public void AppSettings_FtpUrl_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { FtpUrl = "ftp.example.com" };

        // Act & Assert
        Assert.Equal("ftp.example.com", settings.FtpUrl);
    }

    [Fact]
    public void AppSettings_NextStepPath_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { NextStepPath = "/api/next-step" };

        // Act & Assert
        Assert.Equal("/api/next-step", settings.NextStepPath);
    }

    [Fact]
    public void AppSettings_DownloadToName_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { DownloadToName = "downloads" };

        // Act & Assert
        Assert.Equal("downloads", settings.DownloadToName);
    }

    [Fact]
    public void AppSettings_StepLogUrl_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { StepLogUrl = "/api/logs" };

        // Act & Assert
        Assert.Equal("/api/logs", settings.StepLogUrl);
    }

    [Fact]
    public void AppSettings_KeyFile_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { KeyFile = "/path/to/key" };

        // Act & Assert
        Assert.Equal("/path/to/key", settings.KeyFile);
    }

    [Fact]
    public void AppSettings_FtpUser_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings { FtpUser = "testuser" };

        // Act & Assert
        Assert.Equal("testuser", settings.FtpUser);
    }

    [Fact]
    public void AppSettings_AllPropertiesCanBeSet()
    {
        // Arrange
        var settings = new AppSettings
        {
            ServiceName = "FilePoller",
            AppLogEmailSubject = "Alert",
            AppLogFromEmail = "from@example.com",
            AppLogNotifyEmail = "notify@example.com",
            FileChkIntervalSeconds = 30,
            IncomingUrl = "/incoming",
            WaitTimeLoopCount = 10,
            ApiUrl = "http://localhost:5000",
            FtpUrl = "ftp.example.com",
            NextStepPath = "/api/next",
            DownloadToName = "downloads",
            StepLogUrl = "/api/logs",
            KeyFile = "/key",
            FtpUser = "user"
        };

        // Act & Assert
        Assert.Equal("FilePoller", settings.ServiceName);
        Assert.Equal("Alert", settings.AppLogEmailSubject);
        Assert.Equal("from@example.com", settings.AppLogFromEmail);
        Assert.Equal("notify@example.com", settings.AppLogNotifyEmail);
        Assert.Equal(30, settings.FileChkIntervalSeconds);
        Assert.Equal("/incoming", settings.IncomingUrl);
        Assert.Equal(10, settings.WaitTimeLoopCount);
        Assert.Equal("http://localhost:5000", settings.ApiUrl);
        Assert.Equal("ftp.example.com", settings.FtpUrl);
        Assert.Equal("/api/next", settings.NextStepPath);
        Assert.Equal("downloads", settings.DownloadToName);
        Assert.Equal("/api/logs", settings.StepLogUrl);
        Assert.Equal("/key", settings.KeyFile);
        Assert.Equal("user", settings.FtpUser);
    }
}
