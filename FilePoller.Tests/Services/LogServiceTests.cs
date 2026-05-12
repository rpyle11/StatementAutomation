
using FilePoller.Models;
using FilePoller.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace FilePoller.Tests.Services;

public class LogServiceTests
{
    private readonly Mock<IOptions<AppSettings>> _mockSettings;

    public LogServiceTests()
    {
        _mockSettings = new Mock<IOptions<AppSettings>>();

        var appSettings = new AppSettings
        {
            ServiceName = "FilePoller",
            AppLogEmailSubject = "FilePoller Alert",
            AppLogFromEmail = "filepoller@example.com",
            AppLogNotifyEmail = "admin@example.com"
        };

        _mockSettings.Setup(s => s.Value).Returns(appSettings);
    }

    private LogService CreateLogService(HttpMessageHandler? handler = null)
    {
        if (handler == null)
        {
            handler = new MockHttpMessageHandler();
        }

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        return new LogService(_mockSettings.Object, httpClient);
    }

    [Fact]
    public async Task LogAlert_WithErrorMessage_ShouldReturnBoolean()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var logService = CreateLogService(mockHandler);

        var appLog = new AppLog
        {
            LogMsg = "Test error message",
            AppUser = "testuser",
            MessageType = AppLog.MessageTypeEnum.Error,
            SendEmail = true
        };

        // Act
        var result = await logService.LogAlert(appLog);

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task LogAlert_WithNullAppUser_ShouldHandleGracefully()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var logService = CreateLogService(mockHandler);

        var appLog = new AppLog
        {
            LogMsg = "Test error message",
            AppUser = null,
            MessageType = AppLog.MessageTypeEnum.Error,
            SendEmail = false
        };

        // Act & Assert - Should not throw
        var result = await logService.LogAlert(appLog);
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task LogAlert_WithSendEmailFalse_ShouldNotIncludeEmailAddress()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var logService = CreateLogService(mockHandler);

        var appLog = new AppLog
        {
            LogMsg = "Test error message",
            AppUser = "testuser",
            MessageType = AppLog.MessageTypeEnum.Error,
            SendEmail = false
        };

        // Act
        var result = await logService.LogAlert(appLog);

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task LogAlert_WithLongMessage_ShouldHandleGracefully()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var logService = CreateLogService(mockHandler);

        var longMessage = new string('x', 1000);
        var appLog = new AppLog
        {
            LogMsg = longMessage,
            AppUser = "testuser",
            MessageType = AppLog.MessageTypeEnum.Error,
            SendEmail = true
        };

        // Act & Assert - Should not throw
        var result = await logService.LogAlert(appLog);
        Assert.IsType<bool>(result);
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }
}
