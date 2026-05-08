
using FilePoller.Models;

namespace FilePoller.Services;

public interface ILogService
{
    Task<bool> LogAlert(AppLog appLog);
}