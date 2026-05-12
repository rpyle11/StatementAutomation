using System.ComponentModel.DataAnnotations;

namespace FilePoller.Models
{
    public class AppSettings
    {
        public string? ServiceName { get; init; }

        public string? AppLogEmailSubject { get; init; }
        [MaxLength(500)]
        public string? AppLogFromEmail { get; init; }
        [MaxLength(500)]
        public string? AppLogNotifyEmail { get; init; }

        public int FileChkIntervalSeconds { get; init; }

        public string? IncomingUrl { get; init; }

        public int WaitTimeLoopCount { get; init; }

        public string? ApiUrl { get; init; }

        public string? FtpUrl { get; init; }

        public string? NextStepPath { get; init; }

        public string? DownloadToName { get; init; }

       public string? StepLogUrl { get; init; }

       public string? KeyFile { get; init; }

       public string? FtpUser { get; init; }
    }
}
