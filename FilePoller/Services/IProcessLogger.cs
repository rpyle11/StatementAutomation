using StatementProcessorModels;

namespace FilePoller.Services;

public interface IProcessLogger
{
    Task<bool> WriteProcessLog(WriteJobStepParameters parameters);
}