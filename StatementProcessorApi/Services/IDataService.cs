using StatementProcessorApi.Entities;

namespace StatementProcessorApi.Services;

public interface IDataService
{
    Task<Jobs?> AddJob(Jobs job);

    Task<Jobs?> ActiveJobExists(string? appUser);

    Task<Jobs?> SetJobComplete(Jobs job);

    Task<List<Entities.Services>?> GetServices(string? appUser);

    Task<bool> WriteJobStep(JobSteps step, string? appUser);

    Task<Jobs?> GetCurrentJob(string? appUser);

    Task<Jobs?> GetCurrentJobByJobId(Guid jobId, string? appUser);

    Task<Directories?> GetDirectory(string? dirName, string? appUser);

    Task<List<JobSteps>?> GetCurrentJobSteps(Guid jobId, string? appUser);

    Task<ProcessObjects?> AddProcessFile(ProcessObjects prcFile, string? appUser);

    Task<ProcessObjects?> GetUnzippedFiles(Guid jobId, string? appUser);
}