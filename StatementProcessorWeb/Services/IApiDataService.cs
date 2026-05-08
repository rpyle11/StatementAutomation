using StatementProcessorModels;

namespace StatementProcessorWeb.Services;

public interface IApiDataService
{
    Task<JobDto?> CreateJob(JobProcessParameters parameters);

    Task<JobDataDto?> GetJobData(GetJobDataParameters parameters);

    Task<JobDto?> GetActive(JobProcessParameters parameters);

    Task<bool> FilesReady(GetJobDataParameters parameters);

    Task<bool> StartCompression(JobProcessParameters parameters);

    Task<bool> ReportProcess(ReportProcessParameters parameters);

    Task<bool> StopJob(JobProcessParameters parameters);
}