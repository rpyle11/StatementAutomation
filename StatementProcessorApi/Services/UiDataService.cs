using StatementProcessorApi.Models;
using StatementProcessorModels;
using System.Reflection;

namespace StatementProcessorApi.Services
{
    public class UiDataService(IDataService dataService,  ILogService logService) : IUiDataService
    {
        public async Task<JobDataDto?> GetJobData(GetJobDataParameters parameters)
        {
            try
            {
                var job = await dataService.GetCurrentJobByJobId(parameters.JobId.GetValueOrDefault(), parameters.AppUser);

                if (job != null)
                {
                    var jbDto = new JobDto
                    {
                        JobId = job.JobId,
                        JobUser = job.JobUser,
                        Id = job.Id,
                        StopDateTime = job.StopDateTime,
                        StartDateTime = job.StartDateTime
                    };
                    var jobData = new JobDataDto
                    {
                        Job = jbDto
                    };

                    var jbSteps = await dataService.GetCurrentJobSteps(jobData.Job.JobId, parameters.AppUser);

                    if (jbSteps == null) return jobData;
                    jobData.JobSteps = [];
                    foreach (var js in jbSteps.OrderByDescending(o => o.StepDate))
                    {
                        jobData.JobSteps.Add(new JobStepsDto
                        {
                            JobId = js.JobId,
                            Id = js.Id,
                            StepDate = js.StepDate,
                            StepValue = js.StepValue
                        });
                    }

                    return jobData;
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

            return new JobDataDto { Job = new JobDto(), JobSteps = [] };
        }

        public async Task<JobDto?> GetActiveJob(JobProcessParameters parameters)
        {
            try
            {
                var job = await dataService.GetCurrentJob(parameters.AppUser);

                if (job != null)
                {
                    return new JobDto
                    {
                        JobId = job.JobId,
                        JobUser = job.JobUser,
                        Id = job.Id,
                        StopDateTime = job.StopDateTime,
                        StartDateTime = job.StartDateTime
                    };
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

            return null;
        }
    }
}
