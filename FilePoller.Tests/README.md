# FilePoller.Tests - Unit Test Project

## Quick Start

### Run all tests:
```powershell
dotnet test FilePoller.Tests
```

### Run tests with output:
```powershell
dotnet test FilePoller.Tests --logger "console;verbosity=detailed"
```

### Run specific test class:
```powershell
dotnet test FilePoller.Tests --filter "FullyQualifiedName~FilePoller.Tests.Services.DataServiceTests"
```

### Generate code coverage report:
```powershell
dotnet test FilePoller.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Project Structure

```
FilePoller.Tests/
├── Services/
│   ├── DataServiceTests.cs       (Database operations testing)
│   ├── LogServiceTests.cs        (Alert logging testing with mock HTTP)
│   └── ProcessLoggerTests.cs     (Job step logging testing)
├── Models/
│   ├── AppSettingsTests.cs       (Configuration model testing)
│   └── AppLogTests.cs            (Error log model testing)
├── Entities/
│   └── EntitiesTests.cs          (Database entities testing)
├── WorkerTests.cs                (Background service testing)
├── TEST_SUMMARY.md               (Detailed test documentation)
└── FilePoller.Tests.csproj       (Project file with dependencies)
```

## Test Coverage Summary

- **Total Tests**: 64
- **Passed**: 64 (100%)
- **Failed**: 0
- **Categories**: 5 (Services, Worker, Models, Entities, Integration)

### By Component:
- **Worker (BackgroundService)**: 15 tests
- **AppSettings Model**: 14 tests  
- **AppLog Model**: 10 tests
- **Entity Models (Jobs, Directories)**: 13 tests
- **DataService**: 4 tests
- **LogService**: 4 tests
- **ProcessLogger**: 4 tests

## Key Test Patterns

### Unit Tests (Pure)
Model and entity tests that have no external dependencies:
```csharp
[Fact]
public void AppSettings_FileChkIntervalSeconds_CanBeSet()
{
    var settings = new AppSettings { FileChkIntervalSeconds = 30 };
    Assert.Equal(30, settings.FileChkIntervalSeconds);
}
```

### Service Tests with Mocking
Tests using Moq for dependency isolation:
```csharp
[Fact]
public async Task DataService_ShouldCallLogService_OnException()
{
    var mockLogService = new Mock<ILogService>();
    mockLogService.Setup(s => s.LogAlert(It.IsAny<AppLog>()))
        .Returns(Task.FromResult(true));
    // ...
}
```

### Theory Tests (Parameterized)
Tests that run multiple times with different inputs:
```csharp
[Theory]
[InlineData(10)]
[InlineData(30)]
[InlineData(60)]
public void Worker_Settings_ShouldPreserveFileCheckInterval(int intervalSeconds)
{
    var settings = new AppSettings { FileChkIntervalSeconds = intervalSeconds };
    Assert.Equal(intervalSeconds, settings.Value.FileChkIntervalSeconds);
}
```

### Integration Test Approach
Services that make HTTP calls use mock HTTP message handlers:
```csharp
public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
```

## Testing Best Practices Used

1. **Arrange-Act-Assert (AAA)** - Clear test structure
2. **Single Responsibility** - Each test validates one behavior
3. **Meaningful Names** - MethodName_Condition_ExpectedResult
4. **No Test Interdependencies** - Tests can run in any order
5. **Isolation** - Mocking external dependencies
6. **Parametrized Tests** - Theory tests for multiple scenarios
7. **Null/Edge Cases** - Testing boundary conditions

## Dependencies

### Test Framework
- **xUnit.net** (v2.9.3) - Modern .NET testing framework
- **xunit.runner.visualstudio** (v3.1.5) - VS Test Explorer support

### Mocking & Isolation
- **Moq** (v4.20.70) - Mocking framework for creating test doubles

### Infrastructure
- **Microsoft.NET.Test.Sdk** (v18.4.0) - Test discovery and execution
- **coverlet.collector** (v10.0.0) - Code coverage measurement
- **JunitXml.TestLogger** (v8.0.0) - JUnit XML test reporting

## What's Tested

✅ **Service Layer**
- DataService database operations
- LogService HTTP alert logging  
- ProcessLogger job step logging

✅ **Worker Component**
- BackgroundService lifecycle (Start/Stop/Execute)
- Dependency injection
- Configuration management
- Async operations with cancellation

✅ **Models & Entities**
- AppSettings configuration properties
- AppLog error information model
- Jobs entity (job lifecycle)
- Directories entity (file locations)

⚠️ **Not Tested** (Requires External Resources)
- SFTP file operations (Renci.SshNet)
- Real HTTP API calls
- Database operations (Entity Framework)
- Timer-based polling logic
- File system operations

## Future Enhancements

### Short Term
- [ ] Add mocked SFTP tests for file operations
- [ ] Add more exception scenario tests
- [ ] Add configuration validation tests

### Medium Term  
- [ ] Create integration test project
- [ ] Add end-to-end workflow tests
- [ ] Performance benchmarks

### Long Term
- [ ] Containerized test environment
- [ ] CI/CD pipeline integration
- [ ] Coverage reporting dashboard

## Running Specific Tests

### By Outcome:
```powershell
# Run only passed tests
dotnet test FilePoller.Tests --filter "Outcome=Passed"

# Run only failed tests
dotnet test FilePoller.Tests --filter "Outcome=Failed"
```

### By Category:
```powershell
# Run only service tests
dotnet test FilePoller.Tests --filter "FullyQualifiedName~Services"

# Run only model tests
dotnet test FilePoller.Tests --filter "FullyQualifiedName~Models"

# Run only entity tests
dotnet test FilePoller.Tests --filter "FullyQualifiedName~Entities"
```

### By Individual Test:
```powershell
# Run specific test method
dotnet test FilePoller.Tests --filter "FullyQualifiedName=FilePoller.Tests.WorkerTests.Worker_IsBackgroundService"
```

## Continuous Integration

### GitHub Actions Example:
```yaml
- name: Run FilePoller Tests
  run: dotnet test FilePoller.Tests --logger "trx" --collect:"XPlat Code Coverage"
```

### Azure Pipelines Example:
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'FilePoller.Tests'
    arguments: '--collect:"XPlat Code Coverage"'
```

## Troubleshooting

### Tests fail with "connection refused"
These are integration tests attempting real connections. Use the mock HTTP handlers provided.

### Tests timeout
The Worker tests use cancellation tokens. Ensure timeout settings are adequate for your environment.

### Coverage missing
Enable coverage collection:
```powershell
dotnet test FilePoller.Tests /p:CollectCoverage=true
```

## Contact & Support

For test-related issues or questions:
1. Review TEST_SUMMARY.md for detailed documentation
2. Check individual test comments for test intent
3. Examine mock setup in test constructors

## License

Follows the same license as the FilePoller project.
