namespace Compressor.Models
{
    public class CompressFileParameters
    {
        public Guid JobId { get; init; }

        public string? FileName { get; init; }

        public string? OutputDirectory { get; init; }
    }
}
