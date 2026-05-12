using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using StatementProcessorApi.Entities;
using StatementProcessorApi.Models;
using StatementProcessorApi.Services;
using StatementProcessorModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace StatementProcessorApi.Tests
{
   

    public class AutomateServiceTests
    {
        private readonly Mock<ISvcManager> _svcManager = new();
        private readonly Mock<IDataService> _dataService = new();
        private readonly Mock<ILogService> _logService = new();
        private readonly Mock<IEmailService> _emailService = new();
        private readonly Mock<ISendMessage> _sendMessage = new();

        private readonly IOptions<AppSettings> _settings;

        public AutomateServiceTests()
        {
            _settings = Options.Create(new AppSettings
            {
                PdfCompressorSvc = "PdfCompressor",
                FilePollerSvc = "FilePoller",
                KeyFile = "testkey",
                FtpHostUrl = "ftp.test.com",
                FtpUser = "user",
                UploadFtpUrl = "/upload/",
                ArchiveDirName = "Archive",
                ToPaladinDir = "ToPaladin"
            });
        }

        private AutomateService CreateService() =>
            new(_svcManager.Object, _dataService.Object, _logService.Object, _settings, _emailService.Object, _sendMessage.Object);

        [Fact]
        public async Task StartProcess_ShouldCreateNewJob_WhenNoActiveJobExists()
        {
            // Arrange
            var service = CreateService();
            var parameters = new JobProcessParameters { AppUser = "testUser" };

            _dataService.Setup(d => d.ActiveJobExists("testUser"))
                .ReturnsAsync((Jobs?)null);

            _dataService.Setup(d => d.AddJob(It.IsAny<Jobs>()))
                .ReturnsAsync(new Jobs
                {
                    Id = 1,
                    JobId = Guid.NewGuid(),
                    JobUser = "testUser",
                    StartDateTime = DateTime.Now
                });

            // Act
            var result = await service.StartProcess(parameters);

            // Assert
            result.Should().NotBeNull();
            result!.JobUser.Should().Be("testUser");
        }

        [Fact]
        public async Task StartProcess_ShouldReturnExistingJob_WhenActiveJobExists()
        {
            var service = CreateService();
            var parameters = new JobProcessParameters { AppUser = "testUser" };

            var existingJob = new Jobs
            {
                Id = 2,
                JobId = Guid.NewGuid(),
                JobUser = "testUser",
                StartDateTime = DateTime.Now
            };

            _dataService.Setup(d => d.ActiveJobExists("testUser"))
                .ReturnsAsync(existingJob);

            var result = await service.StartProcess(parameters);

            result.Should().NotBeNull();
            result!.Id.Should().Be(2);
        }

        [Fact]
        public async Task WriteJob_ShouldWriteStep_AndSendMessage()
        {
            var service = CreateService();

            _dataService.Setup(d => d.WriteJobStep(It.IsAny<JobSteps>(), "user"))
                .ReturnsAsync(true);

            var result = await service.WriteJob(new WriteJobStepParameters
            {
                AppUser = "user",
                JobId = Guid.NewGuid(),
                Message = "Test message"
            });

            result.Should().BeTrue();
            _sendMessage.Verify(s => s.SendClientMessage(It.IsAny<MessageParameters>(), "user"), Times.Once);
        }

        [Fact]
        public async Task FilesToProcess_ShouldReturnTrue_WhenPdfFilesExist()
        {
            var service = CreateService();

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "file.pdf"), "dummy");

            _dataService.Setup(d => d.GetDirectory("OriginalStatements", "user"))
                .ReturnsAsync(new Directories { UncPath = tempDir });

            var result = await service.FilesToProcess(new GetJobDataParameters { AppUser = "user" });

            result.Should().BeTrue();
        }



    }
}
