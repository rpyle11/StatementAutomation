using FilePoller.Entities;
using FilePoller.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;

namespace FilePoller.Services
{
    public class DataService(IConfiguration configuration, ILogService logService, IOptions<AppSettings> settings) : IDataService
    {
     
        public async Task<Jobs?> GetStartedJob()
        {
            var context = new StatementAutomationContext(configuration);
            try
            {
                return await context.Jobs.FirstOrDefaultAsync(f => !f.StopDateTime.HasValue);
            }
            catch (Exception ex)
            {
                var logMsg = $"Error: {ex.Message}";

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    logMsg += $"Inner Message {ex.InnerException.Message}";
                }

                Log.Error("Error {logMsg}", logMsg);
                await logService.LogAlert(new AppLog
                {
                    AppUser = ServiceAcctName.GetServiceAccountName(settings.Value),
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }
            finally
            {
                await context.DisposeAsync();
            }

            return null;
        }

      

        public async Task<Directories?> GetDirectory(string? dirName)
        {
            var context = new StatementAutomationContext(configuration);
            try
            {
                return await context.Directories.FirstOrDefaultAsync(f => f.Name == dirName);
            }
            catch (Exception ex)
            {
                var logMsg = $"Error: {ex.Message}";

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    logMsg += $"Inner Message {ex.InnerException.Message}";
                }

                Log.Error("Error {logMsg}", logMsg);
                await logService.LogAlert(new AppLog
                {
                    AppUser = ServiceAcctName.GetServiceAccountName(settings.Value),
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }
            finally
            {
                await context.DisposeAsync();
            }

            return null;
        }

    }
}
