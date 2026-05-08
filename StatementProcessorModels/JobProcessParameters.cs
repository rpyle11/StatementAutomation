using System.ComponentModel.DataAnnotations;

namespace StatementProcessorModels
{
    public class JobProcessParameters
    {
        [Required(ErrorMessage = "AppUser is required")]
        [MaxLength(20, ErrorMessage = "AppUser maximum length is 20 characters")]
        public string? AppUser { get; init; }

    
    }
}
