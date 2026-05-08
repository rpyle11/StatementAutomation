using FilePoller.Entities;

namespace FilePoller.Services;

public interface IDataService
{
    Task<Jobs?> GetStartedJob();
  

    Task<Directories?> GetDirectory(string? dirName);
}