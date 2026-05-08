using StatementProcessorModels;

namespace StatementProcessorApi.Services;

public interface IAutomateService
{
    Task<JobDto?> StartProcess(JobProcessParameters parameters);

    Task<bool> CompressionComplete(CompressCompleteParameters parameters);

    Task<bool> ProcessIncoming(FileCheckParameters parameters, string? appUser);

    Task<bool> WriteJob(WriteJobStepParameters parameters);

    Task<bool> FilesToProcess(GetJobDataParameters parameters);

    Task<bool> StartCompression(JobProcessParameters parameters);

    Task<bool> VerifyFtpConnection(string? appUser);

}