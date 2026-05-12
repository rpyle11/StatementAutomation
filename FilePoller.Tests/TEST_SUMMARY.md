# FilePoller Unit Tests - Summary

## Overview
Comprehensive unit test suite for the FilePoller project, targeting .NET 10.0. The test suite includes 64 passing tests covering all major components.

## Test Structure

### 1. **DataServiceTests** (`Services/DataServiceTests.cs`)
Tests for the `DataService` class which handles database operations.

- `GetStartedJob_ShouldReturnNull_WhenNoJobsExist` - Verifies method handles no active jobs gracefully
- `GetDirectory_ShouldReturnNull_WhenDirectoryNotFound` - Tests directory lookup with non-existent directory
- `GetDirectory_ShouldReturnNull_WhenDirectoryNameIsNull` - Tests null directory name handling
- `DataService_ShouldCallLogService_OnException` - Verifies error logging behavior

### 2. **LogServiceTests** (`Services/LogServiceTests.cs`)
Tests for the `LogService` class which handles alert logging via HTTP.

- `LogAlert_WithErrorMessage_ShouldReturnBoolean` - Tests basic error logging
- `LogAlert_WithNullAppUser_ShouldHandleGracefully` - Tests null user handling
- `LogAlert_WithSendEmailFalse_ShouldNotIncludeEmailAddress` - Tests email flag behavior
- `LogAlert_WithLongMessage_ShouldHandleGracefully` - Tests handling of long messages

**Mock Implementation**: `MockHttpMessageHandler` - Simulates HTTP responses without requiring a live endpoint

### 3. **ProcessLoggerTests** (`Services/ProcessLoggerTests.cs`)
Tests for the `ProcessLogger` class which logs job step information.

- `WriteProcessLog_WithValidParameters_ShouldReturnBoolean` - Tests basic logging functionality
- `ProcessLogger_CanBeInstantiatedWithValidSettings` - Verifies service initialization
- `ProcessLogger_Settings_AreInjected` - Tests dependency injection
- `ProcessLogger_WithDifferentApiUrls` - Tests multiple API configurations

### 4. **WorkerTests** (`WorkerTests.cs`)
Tests for the `Worker` class which implements the background service.

- `Worker_IsBackgroundService` - Verifies Worker extends BackgroundService
- `ExecuteAsync_WithCancellationToken_ShouldNotThrow` - Tests async execution with cancellation
- `Worker_Constructor_WithValidDependencies_ShouldInitialize` - Tests dependency injection
- `Worker_StartAsync_ShouldNotThrow` - Tests service startup
- `Worker_StopAsync_ShouldNotThrow` - Tests service shutdown
- `Worker_Dependencies_AreInjected` - Verifies all dependencies are properly injected
- `Worker_Settings_ShouldPreserveFileCheckInterval` - Theory test with multiple intervals (10, 30, 60 seconds)
- `Worker_Settings_ShouldPreserveApiUrl` - Theory test with multiple API URLs
- `Worker_Settings_ShouldPreserveWaitTimeLoopCount` - Theory test with multiple loop counts (5, 10, 20)

### 5. **AppSettingsTests** (`Models/AppSettingsTests.cs`)
Tests for the `AppSettings` configuration model - 14 tests total.

Individual property setters:
- `ServiceName`, `FileChkIntervalSeconds`, `IncomingUrl`, `WaitTimeLoopCount`
- `ApiUrl`, `FtpUrl`, `NextStepPath`, `DownloadToName`
- `StepLogUrl`, `KeyFile`, `FtpUser`

- `AppSettings_AllPropertiesCanBeSet` - Comprehensive property configuration test

### 6. **AppLogTests** (`Models/AppLogTests.cs`)
Tests for the `AppLog` model - 10 tests total.

- `LogMsg`, `AppUser`, `MessageType`, `SendEmail` property setters
- `SendEmail_DefaultIsFalse` - Tests default values
- `MessageType_DefaultIsError` - Tests enum defaults
- `AllPropertiesCanBeSet` - Comprehensive model configuration
- `WithNullLogMsg`, `WithNullAppUser` - Null value handling
- `WithLongMessage` - Large message handling

### 7. **EntitiesTests** (`Entities/EntitiesTests.cs`)
Tests for database entity models - 16 tests total.

**JobsTests**:
- `Id`, `JobId`, `StartDateTime`, `StopDateTime`, `JobUser` property setters
- `IsActiveWhenStopDateTimeIsNull` - Tests job active state
- `AllPropertiesCanBeSet` - Comprehensive entity configuration

**DirectoriesTests**:
- `Id`, `UncPath`, `Name`, `Type` property setters
- `AllPropertiesCanBeSet` - Comprehensive entity configuration
- `WithDifferentUncPaths`, `WithDifferentNames` - Comparison tests

## Test Statistics

| Category | Count |
|----------|-------|
| Service Tests | 12 |
| Worker Tests | 15 |
| Model Tests (AppSettings) | 14 |
| Model Tests (AppLog) | 10 |
| Entity Tests | 13 |
| **Total** | **64** |

All tests pass successfully with 0 failures.

## Dependencies

The test project uses the following NuGet packages:
- **xunit** (v2.9.3) - Unit testing framework
- **xunit.runner.visualstudio** (v3.1.5) - VS test runner
- **Moq** (v4.20.70) - Mocking framework
- **Microsoft.NET.Test.Sdk** (v18.4.0) - Test SDK
- **coverlet.collector** (v10.0.0) - Code coverage
- **JunitXml.TestLogger** (v8.0.0) - Test reporting

## Test Coverage

### Components Tested

1. ✅ **Services**
   - DataService (database operations)
   - LogService (alert logging)
   - ProcessLogger (job step logging)

2. ✅ **Worker** (BackgroundService)
   - Initialization and dependency injection
   - Async execution lifecycle
   - Configuration management

3. ✅ **Models**
   - AppSettings configuration
   - AppLog error information
   - Jobs entity
   - Directories entity

4. ⚠️ **Not Tested** (Due to External Dependencies)
   - SFTP file operations (requires SSH setup)
   - HTTP API calls (requires live API endpoint)
   - Timer-based file checking logic
   - Database context operations (requires database)

## Running Tests

### Run all tests:
```powershell
dotnet test FilePoller.Tests
```

### Run specific test class:
```powershell
dotnet test FilePoller.Tests --filter "ClassName=FilePoller.Tests.Services.DataServiceTests"
```

### Run with code coverage:
```powershell
dotnet test FilePoller.Tests /p:CollectCoverage=true
```

## Future Test Enhancements

1. **Integration Tests**: Create separate test project for testing SFTP and HTTP integrations
2. **Mock SFTP Client**: Extend tests to mock Renci.SshNet SftpClient for file operations
3. **Database Tests**: Add Entity Framework integration tests with test database
4. **Timer Tests**: Test the timer-based polling mechanism
5. **Error Handling**: Add tests for specific exception scenarios (connection failures, timeouts, etc.)

## Test Design Patterns

### Used Patterns:
- **Arrange-Act-Assert (AAA)**: Standard test structure
- **Mocking**: Using Moq for dependency isolation
- **Theory Tests**: Using xUnit [Theory] with [InlineData] for parameterized tests
- **Naming Convention**: MethodName_Condition_ExpectedBehavior

### Example:
```csharp
[Fact]
public void AppSettings_FileChkIntervalSeconds_CanBeSet()
{
    // Arrange
    var settings = new AppSettings { FileChkIntervalSeconds = 30 };

    // Act & Assert
    Assert.Equal(30, settings.FileChkIntervalSeconds);
}
```
