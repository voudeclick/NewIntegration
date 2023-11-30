using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Samurai.Integration.Application.DependencyInjection;
using Samurai.Integration.EntityFramework.Database;
using Serilog;
using System.Threading.Tasks;

namespace Samurai.Integration.NexaasWebJob
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
            })
            .ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables(prefix: "ASPNETCORE_");
            })
            .ConfigureAppConfiguration((context, b) =>
            {
                b.AddEnvironmentVariables();
                b.AddJsonFile("appsettings.json", optional: true);
                b.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName.ToLower()}.json", optional: true);
            })
            .ConfigureLogging((context, b) =>
            {
                Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(context.Configuration)
                                .CreateLogger();

                b.AddSerilog(dispose: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<DatabaseContext>(options => options
                                .UseLazyLoadingProxies()
                                .UseSqlServer(context.Configuration.GetConnectionString("Database")));
                services.AddScoped<Functions, Functions>();
                services.AddHttpClient();
                DependencyInjectionStartup.ConfigureDI(services);
            });

            var host = builder.Build();
            using (host)
            {
                var cancellationToken = new WebJobsShutdownWatcher().Token;
                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;
                await host.StartAsync(cancellationToken);
                _ = jobHost.CallAsync(nameof(Functions.NexaasOrchestrator), cancellationToken);
                await host.WaitForShutdownAsync(cancellationToken);
            }
        }
    }
}
