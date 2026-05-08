using System.ComponentModel.DataAnnotations;

namespace FilePoller.Models
{
    public class AppSettings
    {
        public string? ServiceName { get; set; }

        public string? AppLogEmailSubject { get; init; }
        [MaxLength(500)]
        public string? AppLogFromEmail { get; init; }
        [MaxLength(500)]
        public string? AppLogNotifyEmail { get; init; }

        public int FileChkIntervalSeconds { get; set; }

        public string? IncomingUrl { get; set; }

        public int WaitTimeLoopCount { get; set; }

        public string? ApiUrl { get; set; }

        public string? FtpUrl { get; set; }

        public string? NextStepPath { get; set; }

        public string? DownloadToName { get; set; }

       public string? StepLogUrl { get; set; }

       public string? KeyFile { get; set; }

       public string? FtpUser { get; set; }
    }
}
