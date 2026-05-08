using Microsoft.Extensions.Options;
using StatementProcessorApi.Models;
using StatementProcessorModels;
using System.Net.Http.Headers;
using System.Reflection;

namespace StatementProcessorApi.Services
{
    public class SendMessage(HttpClient httpClient, IOptions<AppSettings> settings, ILogService logService) : ISendMessage
    {
        public async Task<bool> SendClientMessage(MessageParameters message, string? appUser)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response =
                    await httpClient.PostAsJsonAsync($"{httpClient.BaseAddress}{settings.Value.MessageUrl}", message);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                var logMsg = $"Error: {ex.Message}";

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    logMsg += $"Inner Message {ex.InnerException.Message}";
                }

                await logService.LogAlert(new AppLog
                {
                    AppUser = appUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }

            return false;
           
        }
    }
}
