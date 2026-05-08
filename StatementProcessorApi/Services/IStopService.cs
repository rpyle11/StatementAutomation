using StatementProcessorModels;

namespace StatementProcessorApi.Services;

public interface IStopService
{
    Task<bool> StopProcess(JobProcessParameters parameters);
}