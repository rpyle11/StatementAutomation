using Compressor.Models;
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;
using System.Text;
using Compressor.Entities;
using StatementProcessorModels;

namespace Compressor.Services
{
    public class Processor(ILogService logService, IOptions<AppSettings> settings, IDataService dataService, IProcessLogger stepLogger) : IProcessor
    {
        public async Task<bool> CompressFile(CompressFileParameters parameters)
        {
            try
            {
                if (File.Exists(parameters.FileName))
                {
                    Log.Information("Compressing file {pdfFile}",  Path.GetFileName(parameters.FileName));
                    await stepLogger.WriteProcessLog(new WriteJobStepParameters
                    {
                        JobId = parameters.JobId,
                        Message = $"Compressing file {Path.GetFileName(parameters.FileName)}",
                        AppUser = ServiceAcctName.GetServiceAccountName(settings.Value)
                    });

                    var gsVersion = GhostscriptVersionInfo.GetLastInstalledVersion();

                    var outputPath = Path.Combine(parameters.OutputDirectory!, $"{Path.GetFileName(parameters.FileName)}");

                    var arguments = new List<string>
                    {
                        "-q",                    // Quiet mode
                        "-dNOPAUSE",             // Do not pause after each page
                        "-dBATCH",               // Exit after processing
                        "-dSAFER",               // Operate in safer mode
                        "-sDEVICE=pdfwrite",     // Set output device to PDF
                        $"-dCompatibilityLevel={settings.Value.CompatibilityLevel}", // Set PDF compatibility level
                        $"-dColorImageFilter={settings.Value.ColorImageFilter}",
                        $"-dPDFSETTINGS={settings.Value.CompressionLevel}", // Apply the compression setting
                        $"-dEmbedAllFonts={settings.Value.EmbedAllFonts.ToString().ToLower()}",  // Ensure fonts are embedded
                        $"-dSubsetFonts={settings.Value.SubsetFonts.ToString().ToLower()}",    // Use font subsetting
                        "-sOutputFile=" + outputPath, // Set the output file path
                        parameters.FileName             // Specify the input file path
                    };

                    try
                    {
                        var flInfo = new FileInfo(parameters.FileName);
                        var dtStart = DateTime.Now;
                       
                        using var gs = new GhostscriptProcessor(gsVersion);
                        gs.Process(arguments.ToArray());

                        if (File.Exists(outputPath))
                        {
                            var compFile = new FileInfo(outputPath);

                            var argsString = new StringBuilder();
                            foreach (var arg in arguments)
                            {
                                if (argsString.Length == 0)
                                {
                                    argsString.Append(arg);
                                }
                                else
                                {
                                    argsString.Append($"{arg};");
                                }
                               
                            }

                            var logData = await dataService.AddLog(new CompressionLogs
                            {
                                Arguments = argsString.ToString(),
                                CompressDate = DateOnly.FromDateTime(DateTime.Now),
                                CompressedFileName = outputPath,
                                CompressedFileSize = compFile.Length,
                                FileName = Path.GetFileName(parameters.FileName),
                                FileSize = flInfo.Length,
                                StartTime = TimeOnly.FromDateTime(dtStart),
                                StopTime = TimeOnly.FromDateTime(DateTime.Now)
                            });

                            return logData == null ? throw new ApplicationException($"Unable to add compression log for file {Path.GetFileName(parameters.FileName)}") : true;
                        }

                      
                    }
                    catch (GhostscriptException ex)
                    {
                        var logMsg = $"Error: {ex.Message}";

                        if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                        {
                            logMsg += $"Inner Message {ex.InnerException.Message}";
                        }
                        Log.Error("GhostscriptException Error {logMsg}", logMsg);
                        await logService.LogAlert(new AppLog
                        {
                            AppUser = ServiceAcctName.GetServiceAccountName(settings.Value),
                            LogMsg =
                                $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                            MessageType = AppLog.MessageTypeEnum.Error,
                            SendEmail = true,
                        });

                        return false;
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
                    AppUser = ServiceAcctName.GetServiceAccountName(settings.Value),
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
