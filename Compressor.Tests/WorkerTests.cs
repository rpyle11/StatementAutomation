using Compressor.Models;
using Compressor.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compressor.Entities;
using StatementProcessorModels;

namespace Compressor.Tests
{
    public class WorkerTests
    {
        private readonly Mock<IOptions<AppSettings>> _settings = new();
        private readonly Mock<ILogService> _logService = new();
        private readonly Mock<IProcessor> _processor = new();
        private readonly Mock<IDataService> _dataService = new();
        private readonly Mock<IProcessLogger> _stepLogger = new();

        private readonly AppSettings _appSettings;

        public WorkerTests()
        {
            _appSettings = new AppSettings
            {
                NewStatementCopyFromName = "Original",
                FileFilter = "*.pdf",
                ToBeCompressed = "ToBeCompressed",
                Compressed = "Compressed",
                ZippedFile = "Zipped",
                ApiUrl = "http://localhost/",
                NextStepPath = "api/complete"
            };

            _settings.Setup(s => s.Value).Returns(_appSettings);

        }

        private Worker CreateWorker() =>
            new Worker(_settings.Object, _logService.Object, _processor.Object, _dataService.Object, _stepLogger.Object);


        [Fact]
        public async Task WriteLogData_ShouldWriteStepLog()
        {
            var worker = CreateWorker();

            // Inject a fake job
            typeof(Worker).GetField("_currentJob", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(worker, new Jobs { JobId = Guid.NewGuid(), JobUser = "ryan" });

            var method = typeof(Worker).GetMethod("WriteLogData", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)method.Invoke(worker, new object[] { "hello world" });

            _stepLogger.Verify(s => s.WriteProcessLog(It.IsAny<WriteJobStepParameters>()), Times.Once);
        }

    }
}
