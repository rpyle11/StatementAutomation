using Compressor.Entities;
using Compressor.Models;
using Compressor.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Compressor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service.log"),
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true

                )
                .CreateLogger();

            builder.Services.AddHostedService<Worker>();
            builder.Services.AddWindowsService(opts =>
            {
                opts.ServiceName = builder.Configuration.GetSection("AppSettings:ServiceName").Value!;

            });

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddHttpClient<ILogService, LogService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("AppSettings:LogUrl").Value!);

            });
            builder.Services.AddDbContext<StatementAutomationContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DataConnection")));

            builder.Services.AddSerilog();
            builder.Services.AddSingleton<IDataService, DataService>();
            builder.Services.AddSingleton<IProcessor, Processor>();
            builder.Services.AddSingleton<IProcessLogger, ProcessLogger>();

            var host = builder.Build();
            host.Run();
        }
    }
}
