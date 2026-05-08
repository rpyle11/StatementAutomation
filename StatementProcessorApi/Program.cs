
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StatementProcessorApi.Entities;
using StatementProcessorApi.Models;
using StatementProcessorApi.Services;
using System.Net;

namespace StatementProcessorApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

         

            builder.Services.AddControllers();
          
            builder.Services.AddOpenApi();

            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });

            builder.Services.AddHttpClient<ILogService, LogService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("AppSettings:LogUrl").Value!);

            });

            builder.Services.AddDbContext<StatementAutomationContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DataConnection")));

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.AddScoped<IDataService, DataService>();
            builder.Services.AddScoped<IUiDataService, UiDataService>();
            builder.Services.AddScoped<ISvcManager, SvcManager>();
            builder.Services.AddScoped<IAutomateService, AutomateService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IReportProcess, ReportProcess>();
            builder.Services.AddScoped<IStopService, StopService>();
            builder.Services.AddHttpClient<ISendMessage, SendMessage>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("AppSettings:ClientBaseUrl").Value!);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Credentials = CredentialCache.DefaultNetworkCredentials
            });

            var app = builder.Build();

            app.MapOpenApi();

            app.UseAuthorization();
            app.UseStaticFiles();
            app.MapScalarApiReference(opts =>
            {
                opts.Title = "Statement Processor Api";
                opts.Theme = ScalarTheme.Default;
                opts.ShowSidebar = true;
                opts.DarkMode = true;
            });

            app.MapControllers();

            app.Run();
        }
    }
}
