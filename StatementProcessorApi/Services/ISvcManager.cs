using StatementProcessorApi.Models;

namespace StatementProcessorApi.Services;

public interface ISvcManager
{
    Task<bool> StartService(ServiceParameters parameters);

    Task<bool> StopService(ServiceParameters parameters);
}