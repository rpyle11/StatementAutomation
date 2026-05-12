namespace StatementProcessorApi.Models
{
    public class ServiceParameters
    {
       public string? RemoteServer { get; init; } 
       public string? ServiceName { get; init; }

       public Guid JobId { get; init; }
       public string? AppUser { get; init; }
    }
}
