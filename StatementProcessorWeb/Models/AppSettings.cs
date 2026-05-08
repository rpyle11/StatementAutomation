namespace StatementProcessorWeb.Models
{
    public class AppSettings
    {
        public string? AppLogEmailSubject { get; init; }

        public string? AppLogFromEmail { get; init; }

        public string? AppLogNotifyEmail { get; init; }

        public string? StartJobUrl { get; set; }

        public string? JobDataUrl { get; set; }

        public string? ActiveJobUrl { get; set; }

        public string? FilesReadyUrl { get; set; }

        public string? StartCompressionUrl { get; set; }

        public string? ReportProcessUrl { get; set; }

        public string? StopProcessUrl { get; set; }

    }
}
