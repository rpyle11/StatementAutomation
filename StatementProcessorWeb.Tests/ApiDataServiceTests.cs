using Xunit;
using Moq;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StatementProcessorWeb.Services;
using StatementProcessorModels;
using StatementProcessorWeb.Models;

namespace StatementProcessorWeb.Tests
{
    public class ApiDataServiceTests
    {
        private readonly Mock<ILogService> _logService = new();
        private readonly IOptions<AppSettings> _settings = Options.Create(new AppSettings
        {
            StartJobUrl = "/start",
            JobDataUrl = "/jobdata",
            ActiveJobUrl = "/active",
            FilesReadyUrl = "/filesready",
            StartCompressionUrl = "/compress",
            ReportProcessUrl = "/report",
            StopProcessUrl = "/stop"
        });

        private ApiDataService CreateService(FakeHttpMessageHandler handler)
        {
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.test.com")
            };

            return new ApiDataService(client, _logService.Object, _settings);
        }

        [Fact]
        public async Task CreateJob_ShouldReturnJobDto_WhenSuccess()
        {
            var handler = new FakeHttpMessageHandler();
            var expected = new JobDto { JobUser = "ryan" };

            handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expected)
            };

            var service = CreateService(handler);

            var result = await service.CreateJob(new JobProcessParameters { AppUser = "ryan" });

            result.Should().NotBeNull();
            result!.JobUser.Should().Be("ryan");
        }

        [Fact]
        public async Task CreateJob_ShouldLogError_WhenExceptionThrown()
        {
            var handler = new FakeHttpMessageHandler
            {
                Response = null! // Force null reference exception
            };

            var service = CreateService(handler);

            var result = await service.CreateJob(new JobProcessParameters { AppUser = "ryan" });

            result.Should().BeNull();
            _logService.Verify(l => l.LogAlert(It.IsAny<AppLog>()), Times.Once);
        }

       


    }
}
