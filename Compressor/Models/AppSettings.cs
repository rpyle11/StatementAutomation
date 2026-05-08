using System.ComponentModel.DataAnnotations;

namespace Compressor.Models
{
    public class AppSettings
    {
        public string? ServiceName { get; set; }

        public string? AppLogEmailSubject { get; init; }
        [MaxLength(500)]
        public string? AppLogFromEmail { get; init; }
        [MaxLength(500)]
        public string? AppLogNotifyEmail { get; init; }

        public string? FileFilter { get; set; }

        public string? CompatibilityLevel { get; set; }

        public string? CompressionLevel { get; set; }

        public string? ColorImageFilter { get; set; }
        public bool EmbedAllFonts { get; set; }
        public bool SubsetFonts { get; set; }

        public string? ApiUrl { get; set; }

        public string? NextStepPath { get; set; }

        public string? ToBeCompressed { get; set; }

        public string? Compressed { get; set; }

        public string? ZippedFile { get; set; }

        public string? NewStatementCopyFromName { get; set; }

        public string? StepLogUrl { get; set; }

       
    }
}
