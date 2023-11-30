using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Database;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.UpdateServiceBusTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Beginning Processing");
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var env = new HostingEnvironment
            {
                EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ApplicationName = AppDomain.CurrentDomain.FriendlyName,
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
            };

            IConfiguration config = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", true, true)
                  .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                  .AddEnvironmentVariables()
                  .Build();

            var services = new ServiceCollection();
            services.AddDbContext<DatabaseContext>(options => options
                                .UseLazyLoadingProxies()
                                .UseSqlServer(config.GetConnectionString("Database")));
            services.AddSingleton<IHostingEnvironment>(env);
            services.AddSingleton<IConfiguration>(config);
            services.AddLogging(configure => configure.AddConsole());
            DependencyInjectionStartup.ConfigureDI(services);
            var serviceProvider = services.BuildServiceProvider();

            var tenantRepository = serviceProvider.GetService<TenantRepository>();
            var tenantService = serviceProvider.GetService<TenantService>();
            var _serviceBusService = serviceProvider.GetService<ServiceBusService>();

            Console.WriteLine($"Getting Tenants for envirionment {environmentName}");
            var allTenants = await tenantRepository.GetActive();

            Console.WriteLine($"Found {allTenants.Count()} tenants");
            foreach (var tenant in allTenants)
            {
                Console.WriteLine($"Creating queues for tenant {tenant.StoreHandle}");
                await tenantService.CreateQueues(tenant);
                Console.WriteLine($"Queues created for tenant {tenant.StoreHandle}");
            }

            Console.WriteLine("Creating Tenant Queues");
            if (await _serviceBusService.QueueExists(TenantQueue.UpdateShopifyWebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateShopifyWebjobQueue);
            if (await _serviceBusService.QueueExists(TenantQueue.UpdateMillenniumWebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateMillenniumWebjobQueue);
            if (await _serviceBusService.QueueExists(TenantQueue.UpdateNexasWebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateNexasWebjobQueue);
            if (await _serviceBusService.QueueExists(TenantQueue.UpdatePier8WebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdatePier8WebjobQueue);
            if (await _serviceBusService.QueueExists(TenantQueue.UpdateSellerCenterWebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateSellerCenterWebjobQueue);
            Console.WriteLine("Ending Processing");
        }
    }
}
