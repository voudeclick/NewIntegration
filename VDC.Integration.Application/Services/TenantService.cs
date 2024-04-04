using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Millennium;
using VDC.Integration.APIClient.Millennium.Models.Results;
using VDC.Integration.APIClient.ServiceBus;
using VDC.Integration.APIClient.Shopify;
using VDC.Integration.APIClient.Shopify.Enum.Webhook;
using VDC.Integration.APIClient.Shopify.Models.Request;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Application.Strategy.Interfaces;
using VDC.Integration.Application.Strategy.QueuesOverflow;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Entities.Database.MethodPayment;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Enums.Millennium;
using VDC.Integration.Domain.Messages.Millennium;
using VDC.Integration.Domain.Messages.ServiceBus;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Messages.Webjob;
using VDC.Integration.Domain.Queues;
using VDC.Integration.Domain.Repositories;
using VDC.Integration.Domain.Shopify.Models.Results.REST;
using VDC.Integration.EntityFramework.Repositories;

namespace VDC.Integration.Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantRepository _tenantRepository;
        private readonly ServiceBusService _serviceBusService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ShopifyApiClient> _loggerShopifyApiClient;
        private readonly ILogger<ShopifyRESTClient> _loggerShopifyRESTClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MillenniumSessionToken _millenniumSessionToken;
        private readonly IMethodPaymentRepository _methodPaymentRepository;
        
        public TenantService(TenantRepository tenantRepository,
                             ServiceBusService serviceBusService,
                             IConfiguration configuration,
                             ILogger<TenantService> logger,
                             IServiceProvider serviceProvider,
                             IHttpClientFactory httpClientFactory,
                             MillenniumSessionToken millenniumSessionToken,
                             IMethodPaymentRepository methodPaymentRepository)
        {
            _tenantRepository = tenantRepository;
            _serviceBusService = serviceBusService;
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _millenniumSessionToken = millenniumSessionToken;
            _methodPaymentRepository = methodPaymentRepository;

            using (var scope = _serviceProvider.CreateScope())
            {
                _loggerShopifyApiClient = scope.ServiceProvider.GetService<ILogger<ShopifyApiClient>>();
                _loggerShopifyRESTClient = scope.ServiceProvider.GetService<ILogger<ShopifyRESTClient>>();
            }
        }

        public async Task SaveTenant(Tenant tenant)
        {
            _tenantRepository.Save(tenant);
            await _tenantRepository.CommitAsync();
        }

        public async Task UpdateTenant(Tenant tenant)
        {
            _tenantRepository.Update(tenant);
            await _tenantRepository.CommitAsync();
        }

        public IntegrationType ErpType(long id) => _tenantRepository.GetErpType(id);

        public async Task CreateQueues(Tenant tenant)
        {
            try
            {


                foreach (var tenantQueue in TenantQueue.GetAllQueues())
                {
                    if (!await _serviceBusService.QueueExists(tenantQueue))
                        await _serviceBusService.CreateQueue(tenantQueue);

                }

                if (tenant.IntegrationType == IntegrationType.Shopify)
                {
                    foreach (var shopifyQueue in ShopifyQueue.GetAllQueues())
                    {
                        if (await _serviceBusService.QueueExists(GetQueueName(tenant, shopifyQueue)) == false)
                            await _serviceBusService.CreateQueue(GetQueueName(tenant, shopifyQueue));
                    }
                }

                if (tenant.Type == TenantType.Millennium)
                {
                    foreach (var millenniumQueue in MillenniumQueue.GetAllQueues())
                    {
                        if (await _serviceBusService.QueueExists(GetQueueName(tenant, millenniumQueue)) == false)
                            await _serviceBusService.CreateQueue(GetQueueName(tenant, millenniumQueue));
                    }
                }

                if (tenant.Type == TenantType.Omie)
                {
                    foreach (var omieQueue in OmieQueue.GetAllQueues())
                    {
                        if (await _serviceBusService.QueueExists(GetQueueName(tenant, omieQueue)) == false)
                            await _serviceBusService.CreateQueue(GetQueueName(tenant, omieQueue));
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task CreateQueue(Tenant tenant, string queueName)
        {
            if (await _serviceBusService.QueueExists(GetQueueName(tenant, queueName)) == false)
                await _serviceBusService.CreateQueue(GetQueueName(tenant, queueName));
        }

        public async Task DeleteQueues(Tenant tenant)
        {
            if (tenant.IntegrationType == IntegrationType.Shopify)
            {
                foreach (var shopifyQueue in ShopifyQueue.GetAllQueues())
                {
                    if (await _serviceBusService.QueueExists(GetQueueName(tenant, shopifyQueue)))
                        await _serviceBusService.DeleteQueue(GetQueueName(tenant, shopifyQueue));
                }
            }

            if (tenant.Type == TenantType.Millennium)
            {
                foreach (var millenniumQueue in MillenniumQueue.GetAllQueues())
                {
                    if (await _serviceBusService.QueueExists(GetQueueName(tenant, millenniumQueue)))
                        await _serviceBusService.DeleteQueue(GetQueueName(tenant, millenniumQueue));
                }
            }

            if (tenant.Type == TenantType.Omie)
            {
                foreach (var omieQueue in OmieQueue.GetAllQueues())
                {
                    if (await _serviceBusService.QueueExists(GetQueueName(tenant, omieQueue)))
                        await _serviceBusService.DeleteQueue(GetQueueName(tenant, omieQueue));
                }
            }
        }

        public async Task DeleteQueue(Tenant tenant, string queueName, bool sbDefault)
        {
            var queueExist = await _serviceBusService.QueueExists(GetQueueName(tenant, queueName), sbDefault);
            if (queueExist)
                await _serviceBusService.DeleteQueue(GetQueueName(tenant, queueName));
        }

        public async Task SendUpdateTenantMessages(Tenant tenant, TenantType? oldTenantType = null)
        {
            if (tenant.IntegrationType == IntegrationType.Shopify)
            {
                var queue = _serviceBusService.GetQueueClient(TenantQueue.UpdateShopifyWebjobQueue);
                var serviceBusMessage = new ServiceBusMessage(new UpdateTenantMessage { TenantId = tenant.Id, TimerData = DateTime.Now });
                await queue.SendAsync(serviceBusMessage.GetMessage(tenant.Id));
                await queue.CloseAsync();
            }

            if (tenant.Type == TenantType.Millennium || oldTenantType == TenantType.Millennium)
            {
                var queue = _serviceBusService.GetQueueClient(TenantQueue.UpdateMillenniumWebjobQueue);
                var serviceBusMessage = new ServiceBusMessage(new UpdateTenantMessage { TenantId = tenant.Id, TimerData = DateTime.Now });
                await queue.SendAsync(serviceBusMessage.GetMessage(tenant.Id));
                await queue.CloseAsync();
            }

            if (tenant.Type == TenantType.Omie || oldTenantType == TenantType.Omie)
            {
                var queue = _serviceBusService.GetQueueClient(TenantQueue.UpdateOmieWebjobQueue);
                var serviceBusMessage = new ServiceBusMessage(new UpdateTenantMessage { TenantId = tenant.Id, TimerData = DateTime.Now });
                await queue.SendAsync(serviceBusMessage.GetMessage(tenant.Id));
                await queue.CloseAsync();
            }
        }

        public async Task<WebhookQueryOutput> QueryWebhooks(IHttpClientFactory httpClientFactory, Tenant tenant)
        {
            string versionShopify = _configuration.GetSection("Shopify")["Version"];

            var apps = tenant.ShopifyData.GetShopifyApps();
            var app = apps.First();
            var client = new ShopifyApiClient(httpClientFactory, tenant.ShopifyData.Id.ToString(), tenant.ShopifyData.ShopifyStoreDomain,
                versionShopify,
                app.ShopifyPassword);

            return await client.Post(new WebhookQuery());
        }

        public async Task CreateWebhooks(IHttpClientFactory httpClientFactory, Tenant tenant)
        {
            if (tenant.IntegrationType == IntegrationType.Shopify)
            {
                string versionShopify = _configuration.GetSection("Shopify")["Version"];

                var apps = tenant.ShopifyData.GetShopifyApps();
                var app = apps.First();
                var client = new ShopifyApiClient(httpClientFactory,
                                                  tenant.ShopifyData.Id.ToString(),
                                                  tenant.ShopifyData.ShopifyStoreDomain,
                                                  versionShopify,
                                                  app.ShopifyPassword);

                var webhookResult = await client.Post(new WebhookQuery());
                var webhooks = webhookResult.webhookSubscriptions.edges;
                if (webhooks != null)
                {
                    foreach (var webhook in webhooks)
                    {
                        var result = await client.Post(new WebhookDeleteMutation(new WebhookDeleteMutationInput
                        {
                            id = webhook.node.id
                        }));
                        if (result.webhookSubscriptionDelete?.userErrors?.Any() == true)
                            throw new Exception($"Error in delete shopify webhook: {JsonSerializer.Serialize(result.webhookSubscriptionDelete.userErrors)}");
                    }
                }

                if (tenant.Status == true && tenant.ShopifyData.OrderIntegrationStatus == true)
                {
                    List<string> topics = Enum.GetValues(typeof(WebhookType)).Cast<WebhookType>().SelectMany(w => w.GetTopics().Select(t => t.value)).ToList();

                    foreach (var topic in topics)
                    {
                        var result = await client.Post(new WebhookCreateMutation(new WebhookCreateMutationInput
                        {
                            topic = topic,
                            webhookSubscription = new WebhookSubscription
                            {
                                callbackUrl = string.Format(_configuration.GetSection("Shopify")["WebhookEndpoint"], tenant.Id)
                            }
                        }));
                        if (result.webhookSubscriptionCreate?.userErrors?.Any() == true)
                            throw new Exception($"Error in create shopify webhook: {JsonSerializer.Serialize(result.webhookSubscriptionCreate.userErrors)}");
                    }
                }

                tenant.ShopifyData.SetShopifyApps(apps);
                _tenantRepository.Update(tenant);
                await _tenantRepository.CommitAsync();
            }

        }

        public async Task DeleteWebhooks(IHttpClientFactory httpClientFactory, Tenant tenant)
        {
            try
            {
                if (tenant.IntegrationType == IntegrationType.Shopify)
                {
                    var app = tenant.ShopifyData.GetShopifyApps().FirstOrDefault();
                    if (app != null)
                    {
                        string versionShopify = _configuration.GetSection("Shopify")["Version"];

                        var client = new ShopifyApiClient(httpClientFactory, tenant.ShopifyData.Id.ToString(), tenant.ShopifyData.ShopifyStoreDomain,
                            versionShopify,
                            app.ShopifyPassword);

                        var webhookResult = await client.Post(new WebhookQuery());
                        var webhooks = webhookResult.webhookSubscriptions?.edges;
                        if (webhooks != null)
                        {
                            foreach (var webhook in webhooks)
                            {
                                var result = await client.Post(new WebhookDeleteMutation(new WebhookDeleteMutationInput
                                {
                                    id = webhook.node.id
                                }));
                                if (result.webhookSubscriptionDelete?.userErrors?.Any() == true)
                                    throw new Exception($"Error in delete shopify webhook: {JsonSerializer.Serialize(result.webhookSubscriptionDelete.userErrors)}");
                            }
                        }
                    }
                }
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateCarrierService(IHttpClientFactory httpClientFactory, Tenant tenant)
        {
            if (tenant.IntegrationType == IntegrationType.Shopify)
            {
                var app = tenant.ShopifyData.GetShopifyApps().FirstOrDefault();
                if (app != null)
                {
                    string versionShopify = _configuration.GetSection("Shopify")["Version"];

                    var client = new ShopifyRESTClient(_loggerShopifyRESTClient, httpClientFactory, tenant.ShopifyData.Id.ToString(), tenant.ShopifyData.ShopifyStoreDomain,
                        versionShopify,
                        app.ShopifyPassword);

                    var carrierEnabled = false;

                    string callbackUrl = string.Format(_configuration.GetSection("Shopify")["CarrierServiceEndpoint"], tenant.Id);
                    CarrierServiceResult currentServices = null;
                    try
                    {
                        currentServices = await client.Get<CarrierServiceResult>("carrier_services.json");
                    }
                    catch (Exception ex)
                    {
                        //algumas lojas antigas podem não ter dado permissão de shipping no app.
                        //como é algo bem específico de algumas lojas melhor nem dar erro
                        _logger.LogError(ex, "Error on carrier_services Get");
                    }

                    var integrationService = currentServices?.carrier_services.Where(c => c.name == "VDC.Integration").FirstOrDefault();
                    if (integrationService != null)
                    {
                        if (carrierEnabled)
                        {
                            if (integrationService.callback_url != callbackUrl ||
                                integrationService.active == false)
                            {
                                await client.Put($"carrier_services/{integrationService.id}.json", new
                                {
                                    carrier_service = new
                                    {
                                        id = integrationService.id,
                                        name = "VDC.Integration",
                                        active = true,
                                        callback_url = callbackUrl
                                    }
                                });
                            }
                        }
                        else
                        {
                            if (integrationService.active == true)
                            {
                                await client.Delete($"carrier_services/{integrationService.id}.json");
                            }
                        }
                    }
                    else
                    {
                        if (carrierEnabled)
                        {
                            await client.Post($"carrier_services.json", new
                            {
                                carrier_service = new
                                {
                                    name = "VDC.Integration",
                                    callback_url = callbackUrl,
                                    service_discovery = true,
                                    carrier_service_type = "api",
                                    format = "json"
                                }
                            });
                        }
                    }
                }
            }

        }

        public async Task<List<OfOverflow>> GetQueuesOverflow(List<Tenant> tenants)
        {
            var queuesOverflow = new List<OfOverflow>();
            var validateIntegrationType = new List<IValidatesIntegrationType>
            {
                new Shopify(),
                new Millennium(),
                new Omie(),
            };

            var validatesIntegrationTypes = new ValidatesIntegrationTypes(validateIntegrationType);
            foreach (var tenant in tenants)
            {
                var queueTenants = new OfOverflow();
                queueTenants.TanantName = tenant.StoreName;

                var result = await validatesIntegrationTypes.ReturnQueues(tenant, _serviceBusService, GetQueueName, Enqueue);
                if (result != null && result.Count > 0)
                {
                    queueTenants.QueuesOverflow.AddRange(result);
                    queuesOverflow.Add(queueTenants);
                }
            }

            return queuesOverflow;
        }

        private QueueOverflow Enqueue(Tenant tenant, string shopifyQueue, QueueRuntimeInfo info)
        {
            return new QueueOverflow
            {
                QueueName = shopifyQueue,
                MessageCount = info.MessageCountDetails.ActiveMessageCount,
                DeadLetterMessageCount = info.MessageCountDetails.DeadLetterMessageCount,
            };
        }

        public async Task<List<QueueCount>> GetQueuesCount(Tenant tenant)
        {
            List<QueueCount> result = new List<QueueCount>();

            if (tenant.IntegrationType == IntegrationType.Shopify)
            {
                foreach (var shopifyQueue in ShopifyQueue.GetAllQueues())
                {
                    var info = await _serviceBusService.GetQueueRuntimeInfo(GetQueueName(tenant, shopifyQueue));
                    result.Add(new QueueCount
                    {
                        QueueName = shopifyQueue,
                        MessageCount = info.MessageCountDetails.ActiveMessageCount,
                        DeadLetterMessageCount = info.MessageCountDetails.DeadLetterMessageCount,
                        ScheduledMessageCount = info.MessageCountDetails.ScheduledMessageCount
                    });
                }
            }

            if (tenant.Type == TenantType.Millennium)
            {
                foreach (var millenniumQueue in MillenniumQueue.GetAllQueues())
                {
                    var info = await _serviceBusService.GetQueueRuntimeInfo(GetQueueName(tenant, millenniumQueue));
                    result.Add(new QueueCount
                    {
                        QueueName = millenniumQueue,
                        MessageCount = info.MessageCountDetails.ActiveMessageCount,
                        DeadLetterMessageCount = info.MessageCountDetails.DeadLetterMessageCount,
                        ScheduledMessageCount = info.MessageCountDetails.ScheduledMessageCount
                    });
                }
            }

            if (tenant.Type == TenantType.Omie)
            {
                foreach (var omieQueue in OmieQueue.GetAllQueues())
                {
                    var info = await _serviceBusService.GetQueueRuntimeInfo(GetQueueName(tenant, omieQueue));
                    result.Add(new QueueCount
                    {
                        QueueName = omieQueue,
                        MessageCount = info.MessageCountDetails.ActiveMessageCount,
                        DeadLetterMessageCount = info.MessageCountDetails.DeadLetterMessageCount,
                        ScheduledMessageCount = info.MessageCountDetails.ScheduledMessageCount
                    });
                }
            }
            return result;
        }

        public QueueClient GetQueueClient<T>(T tenant, string queueName, bool sbDefault = true)
        where T : IBaseQueue
        {
            return _serviceBusService.GetQueueClient(GetQueueName(tenant, queueName));
        }

        public string GetQueueName<T>(T tenant, string queueName) where T : IBaseQueue
        {
            return $"{tenant.StoreHandle}-{tenant.Id}/{queueName}";
        }

        private async Task<(T, string)> GetListTransIdOfGivenDate<T>(Tenant tenant, string method, DateTime dateUpdate, TransIdType type, CancellationToken cancellationToken)
        {
            var login = tenant.MillenniumData.GetLogins().FirstOrDefault();

            var millenniumData = new MillenniumData(tenant);

            var _apiClient = new MillenniumApiClient(_httpClientFactory, millenniumData, tenant.MillenniumData.Url, login.Login, login.Password, _millenniumSessionToken);

            var parameterDate = type == TransIdType.ListaVitrine ? "DATA_ATUALIZACAO" : "DATA_ATUALIZACAO_INICIAL";

            var param = new Dictionary<string, string>() { };
            param.Add("vitrine", tenant.MillenniumData.VitrineId.ToString());
            param.Add("$top", "2");
            param.Add(parameterDate, dateUpdate.ToString("yyyy-MM-dd"));

            return await _apiClient.GetContent<T>(QueryHelpers.AddQueryString(method, param), cancellationToken);
        }

        public async Task<long> GetTransIdListaVitrine<T>(Tenant tenant, string method, DateTime dateUpdate, TransIdType type, CancellationToken cancellationToken)
        {
            try
            {
                var obj = await GetListTransIdOfGivenDate<T>(tenant, method, dateUpdate, type, cancellationToken);
                var millenniumApiListProductsResult = JsonSerializer.Deserialize<MillenniumApiListProductsResult>(obj.Item2);

                var values = millenniumApiListProductsResult.GetValues();

                var transId = values?.Select(s => s.trans_id).LastOrDefault();
                if (!(transId is null))
                    return (long)transId;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            throw new Exception("Erro ao obter transId vitrine");
        }

        public async Task<long> GetTransIdPrecoDeTabela<T>(Tenant tenant, string method, DateTime dateUpdate, TransIdType type, CancellationToken cancellationToken)
        {
            try
            {
                var obj = await GetListTransIdOfGivenDate<T>(tenant, method, dateUpdate, type, cancellationToken);
                var millenniumApiListPricesResult = JsonSerializer.Deserialize<MillenniumApiListPricesResult>(obj.Item2);
                var values = millenniumApiListPricesResult.GetValues();

                var transId = values?.Select(s => s.trans_id).LastOrDefault();
                if (!(transId is null))
                    return (long)transId;
            }
            catch (Exception)
            {
                throw;
            }

            throw new Exception("Erro ao obter transId preço");
        }

        public async Task<long> GetTransIdSaldoDeEstoque<T>(Tenant tenant, string method, DateTime dateUpdate, TransIdType type, CancellationToken cancellationToken)
        {
            try
            {
                var obj = await GetListTransIdOfGivenDate<T>(tenant, method, dateUpdate, type, cancellationToken);
                var millenniumApiListPricesResult = JsonSerializer.Deserialize<MillenniumApiListStocksResult>(obj.Item2);
                var values = millenniumApiListPricesResult.GetValues();

                var transId = values?.Select(s => s.trans_id).LastOrDefault();
                if (transId > 0 && !(transId is null))
                    return (long)transId;
            }
            catch (Exception)
            {
                throw;
            }

            throw new Exception("Erro ao obter transId estoque");
        }

        public async Task EnqueueMessageShopifyOrder(Tenant tenant, long orderId, string type)
        {
            var queue = GetQueueClient(tenant, type);
            var serviceBusMessage = new ServiceBusMessage(new ShopifyListOrderMessage { ShopifyId = orderId });
            await queue.SendAsync(serviceBusMessage.GetMessage(orderId));
            await queue.CloseAsync();
        }

        public async Task<string> GetTrackingCodeFromOrder(Tenant tenant, string IdOrderMillenium, CancellationToken cancellationToken)
        {
            try
            {
                var login = tenant.MillenniumData.GetLogins().FirstOrDefault();

                var millenniumData = new MillenniumData(tenant);

                var _apiClient = new MillenniumApiClient(_httpClientFactory, millenniumData, tenant.MillenniumData.Url, login.Login, login.Password, _millenniumSessionToken);

                //busca primeiro o pedido na Millennium
                var paramOrder = new Dictionary<string, string>() { };
                paramOrder.Add("vitrine", tenant.MillenniumData.VitrineId.ToString());
                paramOrder.Add("$format", "json");
                paramOrder.Add("cod_pedidov", $"{IdOrderMillenium}");

                var urlOrder = "api/millenium_eco/pedido_venda/listapedidos";
                var resultOrder = await _apiClient.Get<MillenniumApiListOrdersResult>(QueryHelpers.AddQueryString(urlOrder, paramOrder), cancellationToken);

                if (resultOrder.value.Count.Equals(0))
                    return string.Empty;

                var pedidov = resultOrder.value.Select(x => x.pedidov).FirstOrDefault();

                //Com o retorno "listapedidos", é utilizado o pedidov para buscar o código de rastreio
                var param = new Dictionary<string, string>() { };
                param.Add("vitrine", tenant.MillenniumData.VitrineId.ToString());
                param.Add("$format", "json");
                param.Add("data_atualizacao_inicial", "2020-01-24");
                param.Add("list_pedidov", $"({pedidov})");

                var url = "api/millenium_eco/pedido_venda/consultastatus";
                var result = await _apiClient.Get<MillenniumApiListOrdersStatusResult>(QueryHelpers.AddQueryString(url, param), cancellationToken);

                return !string.IsNullOrWhiteSpace(result.value.FirstOrDefault().numero_objeto) ? result.value.FirstOrDefault().numero_objeto : string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateConfigPaymentTypeAsync(Tenant tenant)
        {
            await _methodPaymentRepository.CreateConfigPaymentTypeAsync(tenant);
        }

        public async Task UpdateConfigPaymentTypeAsync(Tenant tenant)
        {
            await _methodPaymentRepository.UpdateConfigPaymentTypeAsync(tenant);
        }

        public async Task<List<MethodPayment>> GetConfigPaymentTypeAsync(Tenant tenant)
        {
            return await _methodPaymentRepository.GetConfigPaymentTypeAsync(tenant);
        }
    }
}
