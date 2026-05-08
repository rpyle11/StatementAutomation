using System.ComponentModel.DataAnnotations;

namespace StatementProcessorModels
{
    public class GetJobDataParameters
    {
        [Required(ErrorMessage = "JobId is required")]
        public Guid? JobId { get; set; }

        [Required(ErrorMessage = "AppUser is required")]
        [MaxLength(20, ErrorMessage = "AppUser maximum length is 20 characters")]
        public string? AppUser { get; set; }
    }
}
