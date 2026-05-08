namespace StatementProcessorModels
{
    public class JobStepsDto
    {
        public int Id { get; set; }

        public Guid JobId { get; set; }

        public string? StepValue { get; set; }

        public DateTime StepDate { get; set; }
    }
}
