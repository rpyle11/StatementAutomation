namespace StatementProcessorModels
{
    public class WriteJobStepParameters
    {
        public Guid JobId { get; set; }
        public string? Message { get; set; }
        public string? AppUser { get; set; }
    }
}
