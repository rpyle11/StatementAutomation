using StatementProcessorModels;

namespace StatementProcessorApi.Services;

public interface ISendMessage
{
    Task<bool> SendClientMessage(MessageParameters message, string? appUser);
}