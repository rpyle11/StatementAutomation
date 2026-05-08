using Compressor.Entities;
using StatementProcessorModels;

namespace Compressor.Services;

public interface IDataService
{
    Task<CompressionLogs> AddLog(CompressionLogs log);
  
    Task<Jobs?> GetStartedJob();
   
    Task<ProcessObjects?> AddProcessFile(ProcessObjects prcFile);

    Task<Directories?> GetDirectory(string? dirName);
}