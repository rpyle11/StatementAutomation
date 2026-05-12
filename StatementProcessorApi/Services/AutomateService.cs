using System.Data;
using System.IO.Compression;
using StatementProcessorApi.Entities;
using StatementProcessorApi.Models;
using StatementProcessorModels;
using System.Reflection;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace StatementProcessorApi.Services
{
    public class AutomateService(ISvcManager serviceManager, IDataService dataService, ILogService logService, IOptions<AppSettings> settings, IEmailService emailService, ISendMessage sendMessage) : IAutomateService
    {
        public async Task<JobDto?> StartProcess(JobProcessParameters parameters)
        {

            try
            {

                JobDto jobData;

                var job = await dataService.ActiveJobExists(parameters.AppUser);
                if (job == null)
                {
                    var newJob = await dataService.AddJob(new Jobs
                    {
                        JobId = Guid.NewGuid(),
                        StartDateTime = DateTime.Now,
                        JobUser = parameters.AppUser
                    });

                    if (newJob != null)
                    {
                        jobData = new JobDto
                        {
                            Id = newJob.Id,
                            JobId = newJob.JobId,
                            JobUser = newJob.JobUser,
                            StopDateTime = newJob.StopDateTime,
                            StartDateTime = newJob.StartDateTime

                        };
                    }
                    else
                    {
                        throw new DataException("Unable to create job");

                    }

                }
                else
                {
                    return new JobDto
                    {
                        Id = job.Id,
                        JobId = job.JobId,
                        JobUser = job.JobUser,
                        StopDateTime = job.StopDateTime,
                        StartDateTime = job.StartDateTime

                    };
                }

                return jobData;

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

        public async Task<bool> CompressionComplete(CompressCompleteParameters parameters)
        {
            try
            {
                var serviceList = await dataService.GetServices(parameters.AppUser);

                var compSvc =
                    serviceList?.FirstOrDefault(f => f.ServiceName == settings.Value.PdfCompressorSvc);

                if (compSvc == null)
                    throw new DataException($"Unable to retrieve service data for {settings.Value.PdfCompressorSvc}");

                await WriteJob(new WriteJobStepParameters
                {
                    JobId = parameters.JobId,
                    Message = "Stopping compression service",
                    AppUser = parameters.AppUser
                });

                //stop service
                await serviceManager.StopService(new ServiceParameters
                {
                    ServiceName = compSvc.ServiceName,
                    RemoteServer = compSvc.ServerName,
                    AppUser = parameters.AppUser,
                    JobId = parameters.JobId

                });

                await WriteJob(new WriteJobStepParameters
                {
                    JobId = parameters.JobId,
                    Message = "Compression File Process Complete",
                    AppUser = parameters.AppUser
                });

                var waitCount = 0;
                do
                {
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    waitCount++;

                } while (IsFileLocked(parameters.ZipFileName) && waitCount <= 20); //1 minute wait max

                //upload zip file to outgoing oms directory
                await WriteJob(new WriteJobStepParameters
                {
                    JobId = parameters.JobId,
                    Message = $"Uploading file {Path.GetFileName(parameters.ZipFileName)}",
                    AppUser = parameters.AppUser
                });

                var keyFile = new PrivateKeyFile(settings.Value.KeyFile!);

                using var ftpClient = new SftpClient(settings.Value.FtpHostUrl!, 22, settings.Value.FtpUser!, keyFile);
                //may need credentials? need ftp info

                await ftpClient.ConnectAsync(CancellationToken.None);

                if (ftpClient.IsConnected)
                {
                    if (parameters.ZipFileName != null)
                    {
                        await using var localStream = File.Create(parameters.ZipFileName);
                        await ftpClient.UploadFileAsync(localStream, $"{settings.Value.UploadFtpUrl}{Path.GetFileName(parameters.ZipFileName)}", CancellationToken.None);
                    }
                }
                else
                {
                    throw new SftpException(StatusCode.Failure, $"Unable to create ftp connection to {settings.Value.UploadFtpUrl}{Path.GetFileName(parameters.ZipFileName)}");

                }


                await WriteJob(new WriteJobStepParameters
                {
                    JobId = parameters.JobId,
                    Message = $"File {Path.GetFileName(parameters.ZipFileName)} successfully uploaded",
                    AppUser = parameters.AppUser
                });


                //move file to archive
                if (!File.Exists(parameters.ZipFileName)) throw new IOException($"Zip file {Path.GetFileName(parameters.ZipFileName)} does not exist.");


                var archiveDir = await dataService.GetDirectory(settings.Value.ArchiveDirName, parameters.AppUser);

                File.Move(parameters.ZipFileName,
                    Path.Combine(archiveDir?.UncPath!, Path.GetFileName(parameters.ZipFileName)), true);

                await WriteJob(new WriteJobStepParameters
                {
                    JobId = parameters.JobId,
                    Message = $"Zip file {Path.GetFileName(parameters.ZipFileName)} moved to archive {archiveDir?.UncPath}",
                    AppUser = parameters.AppUser
                });

                var pollSvc =
                    serviceList?.FirstOrDefault(f => f.ServiceName == settings.Value.FilePollerSvc);


                if (pollSvc == null)
                    throw new DataException($"Unable to retrieve service data for {settings.Value.PdfCompressorSvc}");

                await WriteJob(new WriteJobStepParameters
                {
                    JobId = parameters.JobId,
                    Message = "Starting Polling service",
                    AppUser = parameters.AppUser
                });

                //start  polling service
                await serviceManager.StartService(new ServiceParameters
                {
                    ServiceName = pollSvc.ServiceName,
                    RemoteServer = pollSvc.ServerName,
                    AppUser = parameters.AppUser

                });

                return true;

            }
            catch (Exception ex)
            {
                await WriteJob(new WriteJobStepParameters
                {
                    JobId = parameters.JobId,
                    Message = $"ERROR: {ex.Message}",
                    AppUser = parameters.AppUser
                });

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

        public async Task<bool> ProcessIncoming(FileCheckParameters parameters, string? appUser)
        {
            var currentJob = await dataService.GetCurrentJob(appUser);

            try
            {

                if (currentJob == null) throw new DataException("Unable to retrieve current job");


                var serviceList = await dataService.GetServices(currentJob.JobUser);

                var compSvc =
                    serviceList?.FirstOrDefault(f => f.ServiceName == settings.Value.FilePollerSvc);

                if (compSvc == null)
                    throw new DataException($"Unable to retrieve service data for {settings.Value.FilePollerSvc}");

                if (!parameters.ReachedWaitLimit)
                {
                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Stopping polling service",
                        AppUser = currentJob.JobUser
                    });

                    //stop service
                    await serviceManager.StopService(new ServiceParameters
                    {
                        ServiceName = compSvc.ServiceName,
                        RemoteServer = compSvc.ServerName,
                        AppUser = currentJob.JobUser

                    });
                }


                if (parameters.ReachedWaitLimit)
                {
                    //send email alert that file does not exist in the incoming
                    if (!await emailService.SendMessage(new EmailFields
                    {
                        FromAddress = settings.Value.AppLogFromEmail,
                        Subject = "OMS Incoming NO FILE",
                        ToAddress = settings.Value.NoIncomingFileEmail,
                        MessageBody = "OMS Incoming did not receive a file in the allotted time allowed",
                        UseHtml = true

                    }, currentJob.JobUser)) throw new ApplicationException("Unable to send No OMS file email");
                }
                else if (File.Exists(parameters.DownloadedFile))
                {
                    var exactTo = await dataService.GetDirectory(settings.Value.ToPaladinDir, appUser);

                    //create directory in ToPaladin Directory to extract files to
                    //this directory gets moved to an archive folder later in process
                    var extractDir = Path.Combine(exactTo?.UncPath!,
                        Path.GetFileNameWithoutExtension(parameters.DownloadedFile));

                    if (!Directory.Exists(extractDir))
                    {
                        Directory.CreateDirectory(extractDir);
                    }
                    else
                    {
                        //remove all files in directory
                        foreach (var item in Directory.GetFiles(extractDir))
                        {
                            File.Delete(item);
                        }
                    }

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = $"Extracting files to {extractDir}",
                        AppUser = currentJob.JobUser
                    });

                    await ZipFile.ExtractToDirectoryAsync(parameters.DownloadedFile, extractDir);

                    var dir = await dataService.AddProcessFile(new ProcessObjects
                    {
                        FileDirName = Path.GetFileNameWithoutExtension(parameters.DownloadedFile),
                        JobId = currentJob.JobId
                    }, appUser);

                    //delete the downloaded zip file
                    File.Delete(Path.Combine(exactTo?.UncPath!,
                        Path.GetFileName(parameters.DownloadedFile)));

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Ready to review files",
                        AppUser = currentJob.JobUser
                    });

                    return dir?.GetType() == typeof(ProcessObjects);


                }

            }
            catch (Exception ex)
            {
                if (currentJob != null)
                {
                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = $"ERROR: {ex.Message}",
                        AppUser = currentJob.JobUser
                    });
                }

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

        private static bool IsFileLocked(string? filePath)
        {
            try
            {
                using var stream = File.Open(filePath!, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                // File is not locked by another process with conflicting access rights.
                return false;

            }
            catch (IOException ex)
            {
                const int errorSharingViolation = 32;
                const int errorLockViolation = 33;

                var errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & 0x0000FFFF;

                if (errorCode is errorSharingViolation or errorLockViolation)
                {
                    return true; // File is locked.
                }

                // Re-throw the exception if it's for another reason (e.g., file not found, path error, etc.).
                throw;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public async Task<bool> FilesToProcess(GetJobDataParameters parameters)
        {
            var currentJob = await dataService.GetCurrentJob(parameters.AppUser);
            try
            {
                //verify there are files to be processed
                var watchDir = await dataService.GetDirectory("OriginalStatements", parameters.AppUser);
                var omsFiles = Directory.GetFiles(watchDir?.UncPath!, "*.pdf");
                if (omsFiles.Length != 0) return true;

                if (currentJob != null)
                {
                    await dataService.SetJobComplete(new Jobs
                    {
                        JobId = currentJob.JobId,
                        JobUser = parameters.AppUser,
                        StartDateTime = currentJob.StartDateTime
                    });

                    await WriteJob(new WriteJobStepParameters
                    {
                        AppUser = currentJob.JobUser,
                        JobId = currentJob.JobId,
                        Message = "No PDF files to process"
                    });

                    return false;
                }



            }
            catch (Exception ex)
            {
                if (currentJob != null)
                {
                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = $"ERROR: {ex.Message}",
                        AppUser = currentJob.JobUser
                    });
                }

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

        public async Task<bool> VerifyFtpConnection(string? appUser)
        {
            try
            {
                var keyFile = new PrivateKeyFile(settings.Value.KeyFile!);
                using var ftpClient = new SftpClient(settings.Value.FtpHostUrl!, 22, settings.Value.FtpUser!, keyFile);
                await ftpClient.ConnectAsync(CancellationToken.None);
                if (!ftpClient.IsConnected)
                {
                    throw new SftpException(StatusCode.Failure,
                        $"Unable to create ftp connection to {settings.Value.FtpHostUrl}");
                }

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

        public async Task<bool> StartCompression(JobProcessParameters parameters)
        {
            var currentJob = await dataService.GetCurrentJob(parameters.AppUser);

            try
            {


                var serviceList = await dataService.GetServices(parameters.AppUser);

                var compSvc =
                    serviceList?.FirstOrDefault(f => f.ServiceName == settings.Value.PdfCompressorSvc);

                if (compSvc == null)
                    throw new DataException($"Unable to retrieve service data for {settings.Value.PdfCompressorSvc}");

                if (currentJob != null)
                {
                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Starting compression service",
                        AppUser = currentJob.JobUser
                    });

                    return await serviceManager.StartService(new ServiceParameters
                    {
                        ServiceName = compSvc.ServiceName,
                        RemoteServer = compSvc.ServerName,
                        AppUser = parameters.AppUser,
                        JobId = currentJob.JobId
                    });
                }
            }
            catch (Exception ex)
            {
                if (currentJob != null)
                {
                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = $"ERROR: {ex.Message}",
                        AppUser = currentJob.JobUser
                    });
                }

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
