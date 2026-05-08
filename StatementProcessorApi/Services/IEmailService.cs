using StatementProcessorApi.Models;

namespace StatementProcessorApi.Services
{
    public interface IEmailService
    {
        Task<bool> SendMessage(EmailFields emData, string? appUser);
    }
}