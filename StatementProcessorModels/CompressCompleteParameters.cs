
using System.ComponentModel.DataAnnotations;

namespace StatementProcessorModels
{
    public class CompressCompleteParameters
    {
        public Guid JobId { get; init; }

        [Required(ErrorMessage = "ZipFileName is required")]
        [MaxLength(1000, ErrorMessage = "ZipFileName maximum length is 1000 characters")]
        public string? ZipFileName { get; init; }

        [Required(ErrorMessage = "AppUser is required")]
        [MaxLength(20, ErrorMessage = "AppUser maximum length is 20 characters")]
        public string? AppUser { get; init; }
    }
}
