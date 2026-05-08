using StatementProcessorApi.Models;

namespace StatementProcessorApi.Services;

public interface ILogService
{
    Task<bool> LogAlert(AppLog appLog);
}