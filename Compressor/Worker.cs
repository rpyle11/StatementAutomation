using Compressor.Entities;
using Compressor.Models;
using Compressor.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System.Data;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using StatementProcessorModels;


namespace Compressor
{
    public class Worker(IOptions<AppSettings> settings, ILogService logService, IProcessor process, IDataService dataService, IProcessLogger stepLogger) : BackgroundService
    {
        private static readonly string? BasePath = Path.GetDirectoryName(Environment.ProcessPath);

        private Jobs? _currentJob;
        private Directories? _statementDirectory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _statementDirectory = await dataService.GetDirectory(settings.Value.NewStatementCopyFromName);

                _currentJob = await dataService.GetStartedJob();

                if (_currentJob != null)
                {
                    await WriteLogData($"processing files in directory {_statementDirectory?.UncPath}");

                    await DoWork();
                }
                else
                {
                    throw new ApplicationException("Unable to get the started job");
                }

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private async Task DoWork()
        {
            try
            {

                var prcFiles = Directory.GetFiles(_statementDirectory?.UncPath!, settings.Value.FileFilter!);

                await CreateWorkingDirectories();

                await WriteLogData($"File count to be compressed: {prcFiles.Length}");

                if (await CopyNewStatements(prcFiles))
                {
                    foreach (var pdfFile in Directory.GetFiles(Path.Combine(BasePath!, settings.Value.ToBeCompressed!), settings.Value.FileFilter!))
                    {
                        //do not want to wait infinitely so putting counter in to throw exception when reaching a 60 seconds limit
                        var waitCount = 0;
                        do
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(3));
                            waitCount++;

                        } while (IsFileLocked(pdfFile) && waitCount <= 20); //1 minute wait max

                        if (waitCount >= 20) throw new IOException($"File {pdfFile} has been locked for over 60 seconds");

                        if (!await process.CompressFile(new CompressFileParameters
                        {
                            FileName = pdfFile,
                            JobId = _currentJob!.JobId,
                            OutputDirectory = Path.Combine(BasePath!, settings.Value.Compressed!)
                        }))
                        {
                            throw new IOException($"Unable to compress file {pdfFile}");
                        }

                    }

                    await WriteLogData($"Zipping files at {Path.Combine(BasePath!, settings.Value.Compressed!)}");

                    //zip all files in compressed directory
                    var compressedFiles =
                        Directory.GetFiles(
                            Path.Combine(BasePath!, settings.Value.Compressed!)).ToList();

                    if (compressedFiles.Count > 0)
                    {
                        await WriteLogData($"Total Files Compressed: {compressedFiles.Count}");

                        var zipFile = $"{Path.GetFileNameWithoutExtension(compressedFiles.FirstOrDefault())}.zip";

                        var fullZipPath =
                            Path.Combine(Path.Combine(BasePath!, settings.Value.ZippedFile!), zipFile);

                        await ZipFile.CreateFromDirectoryAsync(
                            Path.Combine(BasePath!, settings.Value.Compressed!),
                          fullZipPath, CompressionLevel.SmallestSize, false);

                        if (File.Exists(fullZipPath))
                        {

                            var zip = await dataService.AddProcessFile(new ProcessObjects
                            {
                                JobId = _currentJob!.JobId,
                                IsZip = true,
                                IsFile = true,
                                FileDirName = Path.GetFileName(fullZipPath)
                            });

                            if (zip == null)
                                throw new DataException(
                                    $"Unable to write log data for zip file {Path.GetFileName(fullZipPath)}");

                            await WriteLogData($"Zip file created at {Path.Combine(BasePath!, settings.Value.ZippedFile!)}");

                            //move file back to original location
                            File.Move(fullZipPath, Path.Combine(_statementDirectory?.UncPath!, Path.GetFileName(fullZipPath)), true);

                            await WriteLogData($"Moved zip file to {_statementDirectory?.UncPath}");

                            if (File.Exists(Path.Combine(_statementDirectory?.UncPath!, Path.GetFileName(fullZipPath))))
                            {
                                await WriteLogData("Removing directories/files no longer used");

                                //cleanup
                                Directory.Delete(Path.Combine(BasePath!, settings.Value.ZippedFile!), true);

                                Directory.Delete(Path.Combine(BasePath!, settings.Value.Compressed!), true);

                                Directory.Delete(Path.Combine(BasePath!, settings.Value.ToBeCompressed!), true);

                                if (!await SendCompleteMessage(Path.Combine(_statementDirectory?.UncPath!, Path.GetFileName(fullZipPath))))
                                    throw new HttpRequestException(
                                        "Unable to make successful request to StatementAutomation Api");

                            }

                        }
                        else
                        {
                            await WriteLogData("Zip file not generated");
                        }
                    }


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





        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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

        private async Task<bool> CopyNewStatements(string[] statementFiles)
        {
            try
            {
                if (statementFiles.Length > 0)
                {
                    var copyTo = Path.Combine(BasePath!, settings.Value.ToBeCompressed!);
                    await WriteLogData($"Copying files to {copyTo}");


                    foreach (var statement in statementFiles)
                    {

                        //do not want to wait infinitely so putting counter in to throw exception when reaching a 60 seconds limit
                        var waitCount = 0;
                        do
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(3));
                            waitCount++;

                        } while (IsFileLocked(statement) && waitCount <= 20); //1 minute wait max

                        var prcFile = await dataService.AddProcessFile(new ProcessObjects
                        {
                            FileDirName = Path.GetFileName(statement),
                            IsFile = true,
                            JobId = _currentJob!.JobId
                        });

                        if (prcFile == null)
                            throw new IOException($"Unable to move statement file {Path.GetFileName(statement)}");

                        File.Copy(statement, Path.Combine(copyTo ?? throw new IOException("ToBeCompressed directory is invalid"), Path.GetFileName(statement)), true);
                    }
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

        private async Task<bool> SendCompleteMessage(string fullZipPath)
        {
            try
            {
               

                var parameters = new CompressCompleteParameters
                {
                    JobId = _currentJob!.JobId,
                    AppUser = _currentJob.JobUser,
                    ZipFileName = fullZipPath

                };
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

      

        private async Task CreateWorkingDirectories()
        {
            try
            {


                if (BasePath != null && settings.Value.ToBeCompressed != null && !Directory.Exists(Path.Combine(BasePath, settings.Value.ToBeCompressed)))
                {
                    Directory.CreateDirectory(Path.Combine(BasePath, settings.Value.ToBeCompressed));
                }
                else
                {
                    if (BasePath != null)
                        if (settings.Value.ToBeCompressed != null)
                            foreach (var file in Directory.GetFiles(Path.Combine(BasePath,
                                         settings.Value.ToBeCompressed)))
                            {
                                File.Delete(file);
                            }
                }

                if (BasePath != null && settings.Value.Compressed != null && !Directory.Exists(Path.Combine(BasePath, settings.Value.Compressed)))
                {
                    Directory.CreateDirectory(Path.Combine(BasePath, settings.Value.Compressed));
                }
                else
                {
                    if (BasePath != null)
                        if (settings.Value.Compressed != null)
                            foreach (var file in Directory.GetFiles(Path.Combine(BasePath,
                                         settings.Value.Compressed)))
                            {
                                File.Delete(file);
                            }
                }

                if (BasePath != null && settings.Value.ZippedFile != null && !Directory.Exists(Path.Combine(BasePath, settings.Value.ZippedFile)))
                {
                    Directory.CreateDirectory(Path.Combine(BasePath, settings.Value.ZippedFile));
                }
                else
                {
                    if (BasePath != null)
                        if (settings.Value.ZippedFile != null)
                            foreach (var file in Directory.GetFiles(Path.Combine(BasePath,
                                         settings.Value.ZippedFile)))
                            {
                                File.Delete(file);
                            }
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


    }
}
