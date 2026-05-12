using Microsoft.Extensions.Options;
using StatementProcessorApi.Entities;
using StatementProcessorApi.Models;
using StatementProcessorModels;
using System.Reflection;
using Telerik.Reporting;

namespace StatementProcessorApi.Services
{
    public class ReportProcess(IDataService dataService, ILogService logService, IOptions<AppSettings> settings, IWebHostEnvironment webHostEnvironment, IEmailService emailService, ISendMessage sendMessage) : IReportProcess
    {
        public async Task<bool> GenerateReport(ReportProcessParameters parameters)
        {
            var currentJob = await dataService.GetCurrentJob(parameters.AppUser);

            try
            {
                var extractedRootDir = await dataService.GetDirectory(settings.Value.ToPaladinDir, parameters.AppUser);

                var extractedDir = await dataService.GetUnzippedFiles(currentJob!.JobId, parameters.AppUser);

                var unzippedPath = Path.Combine(extractedRootDir?.UncPath!, extractedDir?.FileDirName!);

                var countsData = new CountsData
                {
                    ReportTitle = string.Format(settings.Value.CountsReportTitle!,
                        parameters.ReportDate)
                };

                await WriteJob(new WriteJobStepParameters
                {
                    JobId = currentJob.JobId,
                    Message = "Reading unzipped files names to get counts",
                    AppUser = parameters.AppUser
                });

                foreach (var pdfFile in Directory.GetFiles(unzippedPath, "*.pdf"))
                {
                    var fileNameOnly = Path.GetFileName(pdfFile);

                    var segment = fileNameOnly.Split("_")[1];

                    switch (segment.ToLower())
                    {
                        case "ts1":
                            if (countsData.Ts1Count <= 0)
                                countsData.Ts1Count = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "ts2":
                            if (countsData.Ts2Count <= 0)
                                countsData.Ts2Count = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "ts3":
                            if (countsData.Ts3Count <= 0)
                                countsData.Ts3Count = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "ts4":
                            if (countsData.Ts4Count <= 0)
                                countsData.Ts4Count = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "ts5":
                            if (countsData.Ts5Count <= 0)
                                countsData.Ts5Count = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "ts6":
                            if (countsData.Ts6Count <= 0)
                                countsData.Ts6Count = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "ts7":
                            if (countsData.Ts7Count <= 0)
                                countsData.Ts7Count = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "hold":
                            if (countsData.HoldCount <= 0)
                                countsData.HoldCount = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                        case "lg":
                            if (countsData.LgCount <= 0)
                                countsData.LgCount = int.Parse(fileNameOnly.Split("_")[2]);
                            break;
                    }

                }

                await WriteJob(new WriteJobStepParameters
                {
                    JobId = currentJob.JobId,
                    Message = "Generating Counts PDF File",
                    AppUser = parameters.AppUser
                });

                var rpt = Path.Combine(webHostEnvironment.WebRootPath, "StatementCount.trdp");

                var rptPackage = new ReportPackager();
                await using var rptStream = new MemoryStream(await File.ReadAllBytesAsync(rpt));
                var countsReport = (Report)rptPackage.UnpackageDocument(rptStream);

                countsReport.DataSource = countsData;

                var tbl = (Table)countsReport.Items.Find("StatementCountsTable", true)[0];
                tbl.DataSource = countsData;

                var deviceInfo = new System.Collections.Hashtable();
                var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();

                var reportSource = new InstanceReportSource { ReportDocument = countsReport };

                var result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

                var dtReportDate = DateTime.Parse(parameters.ReportDate!).ToString("yyyyMMdd");


                var fileName = string.Format(settings.Value.CountsReportName!, dtReportDate);

                var tempCountDir = await dataService.GetDirectory("Report", parameters.AppUser);

                await using var fs = new FileStream(Path.Combine(tempCountDir?.UncPath!, fileName),
                    FileMode.Create);
                fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
                fs.Close();

                var countsReportPdf = Path.Combine(tempCountDir?.UncPath!, fileName);

                if (File.Exists(countsReportPdf))
                {
                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Sending statement counts email",
                        AppUser = parameters.AppUser
                    });

                    var userData = new AppUserData(parameters.AppUser);
                    var subjectReportDate = DateTime.Parse(parameters.ReportDate!).ToString("MM-dd-yyyy");

                    var emailSent = await emailService.SendMessage(new EmailFields
                    {
                        Attachments = [countsReportPdf],
                        CcAddress = settings.Value.CountsCcEmailAddress,
                        Subject = $"Statement Count of {subjectReportDate}",
                        UseHtml = true,
                        FromAddress = userData.EmailAddress,
                        ToAddress = settings.Value.CountsEmailAddress,
                        MessageBody = settings.Value.CountsMsgBody

                    }, parameters.AppUser);

                    if (!emailSent) throw new ApplicationException("Unable to send statement counts email");

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Moving Counts PDF to archive",
                        AppUser = parameters.AppUser
                    });

                    //move file to archive folder
                    var archiveDir = await dataService.GetDirectory("RptArchive", parameters.AppUser);


                    File.Move(countsReportPdf, Path.Combine(archiveDir?.UncPath!, Path.GetFileName(countsReportPdf)),
                        true);

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Copying extracted files to appropriate directory",
                        AppUser = parameters.AppUser
                    });
                    //move all files to directories
                    foreach (var pdfFile in Directory.GetFiles(unzippedPath, "*.pdf"))
                    {
                        var fileNameOnly = Path.GetFileName(pdfFile);

                        var segment = fileNameOnly.Split("_")[1];

                        switch (segment.ToLower())
                        {
                            case "ts1":
                                var ts1Dir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(ts1Dir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "ts2":
                                var ts2Dir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(ts2Dir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "ts3":
                                var ts3Dir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(ts3Dir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "ts4":
                                var ts4Dir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(ts4Dir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "ts5":
                                var ts5Dir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(ts5Dir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "ts6":
                                var ts6Dir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(ts6Dir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "ts7":
                                var ts7Dir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(ts7Dir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "hold":
                                var tsHoldDir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(tsHoldDir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                            case "lg":
                                var tsLgDir = await dataService.GetDirectory(segment.ToUpper(), parameters.AppUser);
                                File.Copy(pdfFile, Path.Combine(tsLgDir?.UncPath!, Path.GetFileName(pdfFile)), true);
                                break;
                        }

                    }

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Moving extracted pdf files to archive",
                        AppUser = parameters.AppUser
                    });
                    //put extracted files with directory in archive directory
                    var zippedArchiveDir = await dataService.GetDirectory("ToPaladinArchive", parameters.AppUser);

                    var moveToDir = Path.Combine(zippedArchiveDir?.UncPath!, extractedDir?.FileDirName!);

                    //if directory already exists due to service being started a second time with same files?
                    if (Directory.Exists(Path.Combine(zippedArchiveDir?.UncPath!, extractedDir?.FileDirName!)))
                    {
                        Directory.Delete(Path.Combine(zippedArchiveDir?.UncPath!, extractedDir?.FileDirName!), true);
                    }

                    Directory.Move(unzippedPath, moveToDir);

                    //delete original files
                    var originalPdfsDir = await dataService.GetDirectory("OriginalStatements", parameters.AppUser);

                    foreach (var pdf in Directory.GetFiles(originalPdfsDir?.UncPath!, "*.pdf"))
                    {
                        File.Delete(pdf);
                        Thread.Sleep(1000);
                    }

                    //delete original zip file
                    await dataService.SetJobComplete(new Jobs
                    {
                        JobId = currentJob.JobId,
                        JobUser = currentJob.JobUser,
                        StartDateTime = currentJob.StartDateTime
                    });

                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Statement process complete",
                        AppUser = parameters.AppUser
                    });

                    return true;
                }

            }
            catch (Exception ex)
            {
                if (currentJob != null)
                {
                    await WriteJob(new WriteJobStepParameters
                    {
                        JobId = currentJob.JobId,
                        Message = "Reading unzipped files names to get counts",
                        AppUser = parameters.AppUser
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
