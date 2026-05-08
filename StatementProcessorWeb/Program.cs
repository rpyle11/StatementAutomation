using Microsoft.AspNetCore.Authentication.Negotiate;
using StatementProcessorWeb.Components;
using StatementProcessorWeb.Hub;
using StatementProcessorWeb.Models;
using StatementProcessorWeb.Services;
using System.Net;

namespace StatementProcessorWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AppAccess", policy => policy.RequireRole(builder.Configuration.GetSection("AppSettings:ADGroups").Get<List<string>>()!));
            });

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddTelerikBlazor();
            builder.Services.AddSignalR();
            builder.Services.AddControllers();

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddHttpClient<ILogService, LogService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("AppSettings:LogUrl").Value!);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Credentials = CredentialCache.DefaultNetworkCredentials
            });

            builder.Services.AddHttpClient<IApiDataService, ApiDataService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("AppSettings:ApiUrl").Value!);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Credentials = CredentialCache.DefaultNetworkCredentials
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();

            app.MapStaticAssets();
           
            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapHub<NotificationHub>("/notificationhub");

            app.Run();
        }
    }
}
