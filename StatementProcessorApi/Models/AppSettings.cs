namespace StatementProcessorApi.Models
{
    public class AppSettings
    {
        public string? AppLogEmailSubject { get; init; }

        public string? AppLogFromEmail { get; init; }

        public string? AppLogNotifyEmail { get; init; }

        public string? FtpHostUrl { get; init; }

        public string? PdfCompressorSvc { get; set; }

        public string? UploadFtpUrl { get; set; }

        public string? FilePollerSvc { get; set; }

        public string? EmailAddrSep { get; set; }

        public string? SmtpServer { get; set; }

        public bool EmailAuthenticate { get; set; }

        public string? EmailRegExp { get; set; }

        public string? ArchiveDirName { get; set; }

        public string? NoIncomingFileEmail { get; set; }

        public string? MessageUrl { get; set; }

        public string? ToPaladinDir { get; set; }

        public string? CountsReportTitle { get; set; }

        public string? CountsReportName { get; set; }

        public string? CountsEmailAddress { get; set; }

        public string? CountsCcEmailAddress { get; set; } 

        public string? CountsMsgBody { get; set; }

        public string? KeyFile { get; init; }

        public string? FtpUser { get; init; }

    }
}
