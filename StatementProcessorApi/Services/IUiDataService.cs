using StatementProcessorModels;

namespace StatementProcessorApi.Services;

public interface IUiDataService
{
    Task<JobDataDto?> GetJobData(GetJobDataParameters parameters);

    Task<JobDto?> GetActiveJob(JobProcessParameters parameters);
}