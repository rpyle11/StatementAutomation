namespace StatementProcessorApi.Models
{
    public class ServiceParameters
    {
       public string? RemoteServer { get; init; } 
       public string? ServiceName { get; init; }

       public Guid JobId { get; set; }
       public string? AppUser { get; set; }
    }
}
