using Microsoft.EntityFrameworkCore;
using StatementProcessorApi.Entities;
using StatementProcessorApi.Models;
using System.Data;
using System.Reflection;

namespace StatementProcessorApi.Services
{
    public class DataService(StatementAutomationContext context, ILogService logService) : IDataService
    {

        public async Task<Jobs?> GetCurrentJob(string? appUser)
        {
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

                await logService.LogAlert(new AppLog
                {
                    AppUser = appUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }

            return null;
        }

        public async Task<Jobs?> GetCurrentJobByJobId(Guid jobId, string? appUser)
        {
            try
            {
                return await context.Jobs.FirstOrDefaultAsync(f => f.JobId == jobId);
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

            return null;
        }
        public async Task<Jobs?> AddJob(Jobs job)
        {
            try
            {
                await context.Jobs.AddAsync(job);
                await context.SaveChangesAsync();

                return job;
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
                    AppUser = job.JobUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }

            return null;
        }

        public async Task<Jobs?> ActiveJobExists(string? appUser)
        {
            try
            {
                return await context.Jobs.FirstOrDefaultAsync(f => f.StopDateTime == null);

               
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

            return null;
        }

        public async Task<List<Entities.Services>?> GetServices(string? appUser)
        {
            try
            {
                return await context.Services.ToListAsync();
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

            return null;
        }

        public async Task<bool> WriteJobStep(JobSteps step, string? appUser)
        {
          
            try
            {
                await context.JobSteps.AddAsync(step);
                await context.SaveChangesAsync();

                return true;
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

        public async Task<Directories?> GetDirectory(string? dirName, string? appUser)
        {
          
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

                await logService.LogAlert(new AppLog
                {
                    AppUser = appUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }
           

            return null;
        }

        public async Task<Jobs?> SetJobComplete(Jobs job)
        {
            try
            {
                var inDb = await context.Jobs.FirstOrDefaultAsync(f => f.JobId == job.JobId);

                if (inDb == null) throw new DataException($"Unable to find started job {job.JobId}");

                inDb.StopDateTime = DateTime.Now;

                context.Jobs.Update(inDb);
                await context.SaveChangesAsync();

                return inDb;
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
                    AppUser = job.JobUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }


            return null;
        }

        public async Task<List<JobSteps>?> GetCurrentJobSteps(Guid jobId, string? appUser)
        {
            try
            {
                return await context.JobSteps.Where(w => w.JobId == jobId).ToListAsync();
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


            return null;
        }

        public async Task<ProcessObjects?> AddProcessFile(ProcessObjects prcFile, string? appUser)
        {
           
            try
            {
                await context.ProcessObjects.AddAsync(prcFile);
                await context.SaveChangesAsync();

                return prcFile;
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
            

            return null;
        }

        public async Task<ProcessObjects?> GetUnzippedFiles(Guid jobId, string? appUser)
        {
            try
            {
                return await context.ProcessObjects.FirstOrDefaultAsync(w => w.JobId == jobId && !w.IsFile);
               
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


            return null;
        }


    }
}
