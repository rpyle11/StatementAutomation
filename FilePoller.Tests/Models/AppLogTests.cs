using FilePoller.Models;

namespace FilePoller.Tests.Models;

public class AppLogTests
{
    [Fact]
    public void AppLog_LogMsg_CanBeSet()
    {
        // Arrange
        var appLog = new AppLog { LogMsg = "Test message" };

        // Act & Assert
        Assert.Equal("Test message", appLog.LogMsg);
    }

    [Fact]
    public void AppLog_AppUser_CanBeSet()
    {
        // Arrange
        var appLog = new AppLog { AppUser = "testuser" };

        // Act & Assert
        Assert.Equal("testuser", appLog.AppUser);
    }

    [Fact]
    public void AppLog_MessageType_CanBeSet()
    {
        // Arrange
        var appLog = new AppLog { MessageType = AppLog.MessageTypeEnum.Error };

        // Act & Assert
        Assert.Equal(AppLog.MessageTypeEnum.Error, appLog.MessageType);
    }

    [Fact]
    public void AppLog_SendEmail_CanBeSet()
    {
        // Arrange
        var appLog = new AppLog { SendEmail = true };

        // Act & Assert
        Assert.True(appLog.SendEmail);
    }

    [Fact]
    public void AppLog_SendEmail_DefaultIsFalse()
    {
        // Arrange
        var appLog = new AppLog();

        // Act & Assert
        Assert.False(appLog.SendEmail);
    }

    [Fact]
    public void AppLog_MessageType_DefaultIsError()
    {
        // Arrange
        var appLog = new AppLog();

        // Act & Assert
        Assert.Equal(AppLog.MessageTypeEnum.Error, appLog.MessageType);
    }

    [Fact]
    public void AppLog_AllPropertiesCanBeSet()
    {
        // Arrange
        var appLog = new AppLog
        {
            LogMsg = "Test error message",
            AppUser = "testuser",
            MessageType = AppLog.MessageTypeEnum.Error,
            SendEmail = true
        };

        // Act & Assert
        Assert.Equal("Test error message", appLog.LogMsg);
        Assert.Equal("testuser", appLog.AppUser);
        Assert.Equal(AppLog.MessageTypeEnum.Error, appLog.MessageType);
        Assert.True(appLog.SendEmail);
    }

    [Fact]
    public void AppLog_WithNullLogMsg()
    {
        // Arrange & Act
        var appLog = new AppLog { LogMsg = null };

        // Assert
        Assert.Null(appLog.LogMsg);
    }

    [Fact]
    public void AppLog_WithNullAppUser()
    {
        // Arrange & Act
        var appLog = new AppLog { AppUser = null };

        // Assert
        Assert.Null(appLog.AppUser);
    }

    [Fact]
    public void AppLog_WithLongMessage()
    {
        // Arrange
        var longMessage = new string('x', 1000);
        var appLog = new AppLog { LogMsg = longMessage };

        // Act & Assert
        Assert.Equal(1000, appLog.LogMsg!.Length);
    }
}
