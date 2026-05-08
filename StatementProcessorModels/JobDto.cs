namespace StatementProcessorModels
{
    public class JobDto
    {
        public int Id { get; set; }

        public Guid JobId { get; init; }

        public DateTime StartDateTime { get; set; }

        public DateTime? StopDateTime { get; set; }

        public string? JobUser { get; init; }
    }
}
