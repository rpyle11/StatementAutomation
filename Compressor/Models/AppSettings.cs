using System.ComponentModel.DataAnnotations;

namespace Compressor.Models
{
    public class AppSettings
    {
        public string? ServiceName { get; init; }

        public string? AppLogEmailSubject { get; init; }
        [MaxLength(500)]
        public string? AppLogFromEmail { get; init; }
        [MaxLength(500)]
        public string? AppLogNotifyEmail { get; init; }

        public string? FileFilter { get; init; }

        public string? CompatibilityLevel { get; init; }

        public string? CompressionLevel { get; init; }

        public string? ColorImageFilter { get; init; }
        public bool EmbedAllFonts { get; init; }
        public bool SubsetFonts { get; init; }

        public string? ApiUrl { get; init; }

        public string? NextStepPath { get; init; }

        public string? ToBeCompressed { get; init; }

        public string? Compressed { get; init; }

        public string? ZippedFile { get; init; }

        public string? NewStatementCopyFromName { get; init; }

        public string? StepLogUrl { get; init; }

       
    }
}
