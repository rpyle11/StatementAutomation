using System.ComponentModel.DataAnnotations;

namespace StatementProcessorApi.Models
{
    public class EmailFields
    {
        [Required]
        [MaxLength(50)]
        public string? Subject { get; init; }

        [Required]
        [MaxLength(4000)]
        public string? MessageBody { get; init; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? FromAddress { get; init; }

        [Required]
        [MaxLength(4000)]
        public string? ToAddress { get; init; }

        [MaxLength(4000)]
        public string? CcAddress { get; init; }

        [Required]
        public bool UseHtml { get; init; }

        public List<string?>? Attachments { get; init; }




    }
}
