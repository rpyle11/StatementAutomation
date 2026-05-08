using StatementProcessorWeb.Models;

namespace StatementProcessorWeb.Services;

public interface ILogService
{
    Task<bool> LogAlert(AppLog appLog);
}