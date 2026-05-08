using StatementProcessorApi.Entities;
using StatementProcessorApi.Models;
using StatementProcessorModels;
using System.Reflection;
using System.ServiceProcess;
#pragma warning disable CA1416

namespace StatementProcessorApi.Services
{
    public class SvcManager(ILogService logService, IDataService dataService, ISendMessage sendMessage) : ISvcManager
    {
       
        public async Task<bool> StartService(ServiceParameters parameters)
        {
            using var sc = new ServiceController(parameters.ServiceName!, parameters.RemoteServer!);
            try
            {
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        sc.Start();
                        // Optional: Wait for the service to reach the 'Running' state
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));

                        return true;
                    case ServiceControllerStatus.Running:
                        await WriteJob(new WriteJobStepParameters
                        {
                            AppUser = parameters.AppUser,
                            JobId = parameters.JobId,
                            Message =
                                $"Service {parameters.ServiceName} on server {parameters.RemoteServer} is already running and must be stopped"
                        });

                        return false;
                    case ServiceControllerStatus.StartPending:
                    case ServiceControllerStatus.StopPending:
                    case ServiceControllerStatus.ContinuePending:
                    case ServiceControllerStatus.PausePending:
                    case ServiceControllerStatus.Paused:
                        break;
                    default:
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

        public async Task<bool> StopService(ServiceParameters parameters)
        {
            using var sc = new ServiceController(parameters.ServiceName!, parameters.RemoteServer!);
            try
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    // Optional: Wait for the service to reach the 'Running' state
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(60));

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

        public async Task<bool> WriteJob(WriteJobStepParameters parameters)
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

                return logged;

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
    }
}
