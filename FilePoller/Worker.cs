using System.Data;
using FilePoller.Entities;
using FilePoller.Models;
using FilePoller.Services;

using Microsoft.Extensions.Options;
using Serilog;
using StatementProcessorModels;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Timers;
using Renci.SshNet;
using Timer = System.Timers.Timer;


namespace FilePoller
{
    public class Worker(IOptions<AppSettings> settings, ILogService logService, IDataService dataService, IProcessLogger stepLogger) : BackgroundService
    {

        private Timer? _svcTimer;
        private int _fileChecked;
        private Jobs? _currentJob;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _currentJob = await dataService.GetStartedJob();

                _svcTimer = new Timer(TimeSpan.FromSeconds(settings.Value.FileChkIntervalSeconds));
                _svcTimer.AutoReset = true;
                _svcTimer.Elapsed += _svcTimer_Elapsed;
                _svcTimer.Enabled = true;

                await WriteLogData($"Waiting for file at {settings.Value.FtpUrl}{settings.Value.IncomingUrl}");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private async void _svcTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {

                _svcTimer?.Enabled = false;

                var downloadedFile = await CheckAndDownloadFile();

                if (string.IsNullOrEmpty(downloadedFile))
                {
                    if (_fileChecked >= settings.Value.WaitTimeLoopCount)
                    {
                        if (!await SendFileReceivedRequest(new FileCheckParameters
                        {
                            ReachedWaitLimit = true,
                            DownloadedFile = null
                        })) throw new HttpRequestException($"Unable to send request to Api {settings.Value.ApiUrl}");

                        await WriteLogData("No file alert email sent");
                        _fileChecked = 0;
                        await WriteLogData($"Waiting for file at {settings.Value.FtpUrl}{settings.Value.IncomingUrl}");
                    }
                    else
                    {
                        _fileChecked++;
                    }

                    _svcTimer?.Enabled = true;
                }
                else
                {
                    if (!await SendFileReceivedRequest(new FileCheckParameters
                    {
                        ReachedWaitLimit = false,
                        DownloadedFile = downloadedFile
                    })) throw new HttpRequestException($"Unable to send request to Api {settings.Value.ApiUrl}");

                }

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
                    LogMsg = $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                    AppUser = ServiceAcctName.GetServiceAccountName(settings.Value)
                });
            }

        }

        private async Task<string?> CheckAndDownloadFile()
        {
            try
            {
                var keyFile = new PrivateKeyFile(settings.Value.KeyFile!);
                //may need credentials? need ftp info
                await using var ftpClient = new SftpClient(settings.Value.FtpUrl,22, keyFile);
                await ftpClient.Connect();

                if (ftpClient.IsConnected)
                {
                    var items = await ftpClient.GetListing(settings.Value.IncomingUrl);

                    if (items.Length > 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                        var file = items.FirstOrDefault()?.FullName;

                        var downloadTo = await dataService.GetDirectory(settings.Value.DownloadToName);

                        if (downloadTo == null)
                            throw new DataException(
                                $"Unable to get directory data for {settings.Value.DownloadToName}");


                        await WriteLogData($"Downloading file: {file} to {downloadTo.UncPath}");

                        var downloadStatus = await ftpClient.DownloadFile(Path.Combine(downloadTo.UncPath, Path.GetFileName(file)!), file,FtpLocalExists.Overwrite);

                        if (downloadStatus == FtpStatus.Success)
                        {

                            await WriteLogData($"File: {file} successfully downloaded");

                            await WriteLogData($"Deleting file {Path.GetFileName(file)} from FTP folder");

                            await ftpClient.DeleteFile(file);

                            return Path.Combine(downloadTo.UncPath, Path.GetFileName(file)!);
                        }



                    }
                }
                else
                {
                    throw new FtpException($"Unable to create ftp connection to {settings.Value.FtpUrl}");
                }
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
                    LogMsg = $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                    AppUser = ServiceAcctName.GetServiceAccountName(settings.Value)
                });
            }

            return null;
        }

        private async Task<bool> SendFileReceivedRequest(FileCheckParameters parameters)
        {
            try
            {

                var httpHandler = new HttpClientHandler();
                httpHandler.Credentials = CredentialCache.DefaultNetworkCredentials;

                using var client = new HttpClient(httpHandler);
                client.BaseAddress = new Uri(settings.Value.ApiUrl!);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsJsonAsync($"{client.BaseAddress}{settings.Value.NextStepPath}", parameters);

                return response.IsSuccessStatusCode;
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
                    LogMsg = $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                    AppUser = ServiceAcctName.GetServiceAccountName(settings.Value)
                });
            }

            return false;
        }

        private async Task WriteLogData(string message)
        {
            try
            {
                Log.Information(message);

                await stepLogger.WriteProcessLog(new WriteJobStepParameters
                {
                    JobId = _currentJob!.JobId,
                    Message = message,
                    AppUser = _currentJob.JobUser
                });

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
                    LogMsg = $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                    AppUser = ServiceAcctName.GetServiceAccountName(settings.Value)
                });
            }

        }


    }
}
