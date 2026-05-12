using Microsoft.Extensions.Options;
using StatementProcessorApi.Entities;
using StatementProcessorApi.Models;
using StatementProcessorModels;
using System.Data;
using System.Reflection;

namespace StatementProcessorApi.Services
{
    public class StopService(ISvcManager serviceManager, IDataService dataService, ILogService logService, IOptions<AppSettings> settings, ISendMessage sendMessage) : IStopService
    {
        public async Task<bool> StopProcess(JobProcessParameters parameters)
        {
            var currentJob = await dataService.GetCurrentJob(parameters.AppUser);

            try
            {
                if (currentJob != null)
                {
                    var serviceList = await dataService.GetServices(parameters.AppUser);

                    var compSvc =
                        serviceList?.FirstOrDefault(f => f.ServiceName == settings.Value.PdfCompressorSvc);

                    var pollerSvc =
                        serviceList?.FirstOrDefault(f => f.ServiceName == settings.Value.FilePollerSvc);

                    if (compSvc == null)
                        throw new DataException($"Unable to retrieve service data for {settings.Value.PdfCompressorSvc}");

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Stopping services if running",
                        AppUser = parameters.AppUser
                    });

                    //stop service
                    await serviceManager.StopService(new ServiceParameters
                    {
                        ServiceName = compSvc.ServiceName,
                        RemoteServer = compSvc.ServerName,
                        AppUser = parameters.AppUser,
                        JobId = currentJob.JobId

                    });

                    await serviceManager.StopService(new ServiceParameters
                    {
                        ServiceName = pollerSvc?.ServiceName,
                        RemoteServer = pollerSvc?.ServerName,
                        AppUser = parameters.AppUser,
                        JobId = currentJob.JobId

                    });

                    await dataService.SetJobComplete(new Jobs
                    {
                        JobId = currentJob.JobId,
                        JobUser = parameters.AppUser,
                        StartDateTime = currentJob.StartDateTime,
                        StopDateTime = DateTime.Now,
                        Id = currentJob.Id
                    });

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Running process has been stopped",
                        AppUser = parameters.AppUser
                    });

                    return true;
                }

               
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
                    AppUser = parameters.AppUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }

            return false;
        }

        private async Task WriteJob(WriteJobStepParameters parameters)
        {
            try
            {
                var logged = await dataService.WriteJobStep(new JobSteps
                {
                    JobId = parameters.JobId,
                    StepDate = DateTime.Now,
                    StepValue = parameters.Message
                }, parameters.AppUser);

                if (logged) await sendMessage.SendClientMessage(new MessageParameters
                {
                    Message = parameters.Message

                }, parameters.AppUser);

               

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
                    AppUser = parameters.AppUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }

         

        }
    }
}
