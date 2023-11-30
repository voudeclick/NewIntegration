using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.Domain.Entities;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.Domain.Entities.Database.Logs;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.ServiceHangfire
{
    public class Clear
    {
        IServiceProvider _serviceProvider;

        CleanTable<ShopifyListOrderProcess> _shopifyListOrderProcess;
        CleanTable<ShopifyUpdateOrderTagNumberProcess> _shopifyUpdateOrderTagNumberProcess;
        CleanTable<MillenniumListProductManualProcess> _millenniumListProductManualProcess;
        CleanTable<MillenniumNewPricesProcess> _millenniumNewPricesProcess;
        CleanTable<MillenniumNewProductProcess> _millenniumNewProductProcess;
        CleanTable<MillenniumNewStockProcess> _millenniumNewStockProcess;
        CleanTable<MillenniumUpdateOrderProcess> _millenniumUpdateOrderProcess; 
        CleanTable<ShopifyProductIntegration> _shopifyProductIntegration;
        CleanTable<ShopifyUpdateOrderTagNumberProcess> _shopifyUpdateOrderTagNumberProcesses;
        CleanTable<LogsAbandonMessage> _logsAbandonMessage;

        private readonly ILogger<Clear> _logger;

        public Clear(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            using var scope = _serviceProvider.CreateScope();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<Clear>>();
        }

        public async Task Initialize()
        {
            _logger.LogInformation("Inicialização da limpeza do banco");

            _shopifyListOrderProcess = (CleanTable<ShopifyListOrderProcess>)_serviceProvider.GetService(typeof(CleanTable<ShopifyListOrderProcess>));

            _millenniumListProductManualProcess = (CleanTable<MillenniumListProductManualProcess>)_serviceProvider.GetService(typeof(CleanTable<MillenniumListProductManualProcess>));

            _millenniumNewPricesProcess = (CleanTable<MillenniumNewPricesProcess>)_serviceProvider.GetService(typeof(CleanTable<MillenniumNewPricesProcess>));

            _millenniumNewProductProcess = (CleanTable<MillenniumNewProductProcess>)_serviceProvider.GetService(typeof(CleanTable<MillenniumNewProductProcess>));

            _millenniumNewStockProcess = (CleanTable<MillenniumNewStockProcess>)_serviceProvider.GetService(typeof(CleanTable<MillenniumNewStockProcess>));

            _millenniumUpdateOrderProcess = (CleanTable<MillenniumUpdateOrderProcess>)_serviceProvider.GetService(typeof(CleanTable<MillenniumUpdateOrderProcess>));

            _shopifyUpdateOrderTagNumberProcess = (CleanTable<ShopifyUpdateOrderTagNumberProcess>)_serviceProvider.GetService(typeof(CleanTable<ShopifyUpdateOrderTagNumberProcess>));

            _shopifyProductIntegration = (CleanTable<ShopifyProductIntegration>)_serviceProvider.GetService(typeof(CleanTable<ShopifyProductIntegration>));

            _shopifyUpdateOrderTagNumberProcesses = (CleanTable<ShopifyUpdateOrderTagNumberProcess>)_serviceProvider.GetService(typeof(CleanTable<ShopifyUpdateOrderTagNumberProcess>));

            _logsAbandonMessage = (CleanTable<LogsAbandonMessage>)_serviceProvider.GetService(typeof(CleanTable<LogsAbandonMessage>));

            await ClearRepositories();
        }

        private async Task ClearRepositories()
        {
            await _shopifyListOrderProcess.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _millenniumListProductManualProcess.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _millenniumNewPricesProcess.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _millenniumNewProductProcess.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _millenniumNewStockProcess.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _millenniumUpdateOrderProcess.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _shopifyUpdateOrderTagNumberProcess.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _shopifyProductIntegration.ClearDatabase((x) => x.IntegrationDate < DateTime.Now.AddDays(-7));

            await _shopifyUpdateOrderTagNumberProcesses.ClearDatabase((x) => x.ProcessDate < DateTime.Now.AddDays(-7));

            await _logsAbandonMessage.ClearDatabase((x) => x.CreationDate < DateTime.Now.AddDays(-30));

            _logger.LogInformation("Conclusão da limpeza do banco");
        }
    }
}
