using System.ComponentModel.DataAnnotations;

namespace StatementProcessorModels
{
    public class ReportProcessParameters
    {
        [Required(ErrorMessage = "ReportDate is required")]
        [MaxLength(10, ErrorMessage = "ReportDate maximum length is 10 characters")]
        public string? ReportDate { get; set; }

        [Required(ErrorMessage = "AppUser is required")]
        [MaxLength(20, ErrorMessage = "AppUser maximum length is 20 characters")]
        public string? AppUser { get; set; }

        
    }
}
