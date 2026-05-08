using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using StatementProcessorModels;
using StatementProcessorWeb.Models;
using StatementProcessorWeb.Services;
using System.Net;
using System.Reflection;
using StatementProcessorWeb.Extensions;
namespace StatementProcessorWeb.Components.Pages
{
    public partial class Home
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        [Inject] private IApiDataService? DataService { get; set; }
        [Inject] protected ILogService? LogService { get; set; }

        [Inject] private NavigationManager? NavManager { get; set; }

        private HubConnection? HubConnection { get; set; }

        private GetJobDataParameters? DataParameters { get; set; }

        private JobDto? CurrentJob { get; set; }

        private JobDataDto? RunningJob { get; set; }

        private string? AppUser { get; set; }

        private bool BtnWaitVisible { get; set; }

        private string BtnText { get; set; } = "Start";

        private bool CanStop { get; set; }

        private bool IsStopping { get; set; }



        protected override async Task OnInitializedAsync()
        {
            if (AuthenticationStateTask != null)
            {
                var authState = await AuthenticationStateTask;

                AppUser = authState.User.Identity?.Name?.Split('\\').Last();
                await LoadCurrentJob();

                //start signalR connection to see job steps
                HubConnection = new HubConnectionBuilder()
                    .WithUrl(NavManager!.ToAbsoluteUri("./notificationhub"), options =>
                    {
                        options.Credentials = CredentialCache.DefaultNetworkCredentials;
                    })
                    .WithAutomaticReconnect()
                    .Build();

                HubConnection.On<string>("ServerMessage", async (message) =>
                {
                    await LoadData();

                    if (message.Contains("Ready to review files"))
                    {
                        BtnText = "Start Next Steps";
                        BtnWaitVisible = false;
                        CurrentJob = null;
                        CanStop = false;


                    }
                    else if (message.Contains("No PDF files to process") || message.Contains("Statement process complete"))
                    {
                        BtnText = "Start";
                        BtnWaitVisible = false;
                        CurrentJob = null;
                        CanStop = false;

                    }
                    else if (message.Contains("Running process has been stopped"))
                    {
                        BtnText = "Start";
                        BtnWaitVisible = false;
                        CurrentJob = null;
                        CanStop = false;
                        IsStopping = false;
                    }

                    await InvokeAsync(StateHasChanged);


                });

                await HubConnection.StartAsync();

            }
        }

        private async Task SetJobProcess()
        {
            try
            {
                switch (BtnText)
                {
                    //start job
                    case "Start":
                        await StartJob();
                        CanStop = true;
                        break;
                    case "Start Next Steps": //create & email report and move files to appropriate folders
                        BtnText = "Process Running";
                        BtnWaitVisible = true;
                        CanStop = false;
                        await DataService?.ReportProcess(new ReportProcessParameters
                        {
                            AppUser = AppUser,
                            ReportDate = DateTime.Now.PreviousBusinessDay().ToShortDateString()
                        })!;

                        break;

                }
            }
            catch (Exception ex)
            {
                await LogService?.LogAlert(AppLogPrep.AppLogSetup(AppUser, NavManager?.Uri!,
                    MethodName.GetMethodName(MethodBase.GetCurrentMethod()), ex))!;

                NavManager?.NavigateTo("./error");
            }
        }

        private async Task StartJob()
        {
            try
            {
                if (CurrentJob == null)
                {
                    BtnText = "Process Running";
                    BtnWaitVisible = true;

                    StateHasChanged();

                    CurrentJob = await DataService?.CreateJob(new JobProcessParameters
                    {
                        AppUser = AppUser

                    })!;

                    if (CurrentJob != null)
                    {
                        DataParameters = new GetJobDataParameters
                        {
                            AppUser = AppUser,
                            JobId = CurrentJob.JobId
                        };

                        var ready = await DataService.FilesReady(DataParameters);

                        if (ready)
                        {
                            //start compression service
                            await DataService.StartCompression(new JobProcessParameters
                            {
                                AppUser = AppUser

                            });

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                await LogService?.LogAlert(AppLogPrep.AppLogSetup(AppUser, NavManager?.Uri!,
                    MethodName.GetMethodName(MethodBase.GetCurrentMethod()), ex))!;

                NavManager?.NavigateTo("./error");
            }
        }

        private async Task LoadCurrentJob()
        {
            try
            {
                CurrentJob = await DataService?.GetActive(new JobProcessParameters
                {
                    AppUser = AppUser

                })!;

                if (CurrentJob != null)
                {
                    DataParameters = new GetJobDataParameters
                    {
                        AppUser = AppUser,
                        JobId = CurrentJob.JobId
                    };

                    await LoadData();

                    if (RunningJob?.JobSteps != null)
                        foreach (var step in RunningJob.JobSteps.OrderByDescending(o => o.StepDate).ToList())
                        {
                            if (step.StepValue != null && (step.StepValue.Contains("No PDF files to process") ||
                                                           step.StepValue.Contains("Statement process complete")))
                            {
                                BtnText = "Start";
                                BtnWaitVisible = false;
                                CurrentJob = null;
                                CanStop = false;
                                return;
                            }
                            if (step.StepValue != null && step.StepValue.Contains("Running process has been stopped"))
                            {
                                BtnText = "Start";
                                BtnWaitVisible = false;
                                CurrentJob = null;
                                CanStop = false;
                                IsStopping = false;
                                return;
                            }

                            var reviewReady = RunningJob.JobSteps.FirstOrDefault(w =>
                                w.JobId == CurrentJob!.JobId && w.StepValue == "Ready to review files");

                            if (reviewReady == null)
                            {
                                BtnText = "Process Running";
                                BtnWaitVisible = true;
                                CanStop = true;
                               
                            }
                            else
                            {
                                BtnText = "Start Next Steps";
                                BtnWaitVisible = false;
                                CanStop = false;
                               
                            }
                           





                        }
                }
            }
            catch (Exception ex)
            {
                await LogService?.LogAlert(AppLogPrep.AppLogSetup(AppUser, NavManager?.Uri!,
                    MethodName.GetMethodName(MethodBase.GetCurrentMethod()), ex))!;

                NavManager?.NavigateTo("./error");
            }
        }

        private async Task LoadData()
        {
            try
            {
                if (DataParameters != null)
                {
                    var data = await DataService?.GetJobData(DataParameters)!;

                    if (data != null)
                    {
                        RunningJob = data;
                        await InvokeAsync(StateHasChanged);

                    }
                }

            }
            catch (Exception ex)
            {
                await LogService?.LogAlert(AppLogPrep.AppLogSetup(AppUser, NavManager?.Uri!,
                    MethodName.GetMethodName(MethodBase.GetCurrentMethod()), ex))!;

                NavManager?.NavigateTo("./error");
            }
        }

        private async Task StopProcess()
        {
            try
            {
                if (CurrentJob != null)
                {
                    IsStopping = true;
                    CanStop = false;

                    StateHasChanged();
                    if (!await DataService?.StopJob(new JobProcessParameters
                    {
                        AppUser = AppUser
                    })!) throw new ApplicationException($"Unable to stop current process with job id {CurrentJob.JobId}");
                }


            }
            catch (Exception ex)
            {
                await LogService?.LogAlert(AppLogPrep.AppLogSetup(AppUser, NavManager?.Uri!,
                    MethodName.GetMethodName(MethodBase.GetCurrentMethod()), ex))!;

                NavManager?.NavigateTo("./error");
            }
        }

    }
}
