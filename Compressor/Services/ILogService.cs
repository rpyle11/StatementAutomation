using Compressor.Models;

namespace Compressor.Services;

public interface ILogService
{
    Task<bool> LogAlert(AppLog appLog);
}