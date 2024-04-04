using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Omie.Models.Webhook;
using VDC.Integration.APIClient.Shopify.Enum.Webhook;
using VDC.Integration.APIClient.Shopify.Models;
using VDC.Integration.APIClient.Shopify.Models.Request;
using VDC.Integration.Application.Actors.API;
using VDC.Integration.Domain.Dtos;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Messages.Omie;
using VDC.Integration.Domain.Messages.ServiceBus;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Queues;
using VDC.Integration.EntityFramework.Repositories;
using VDC.Integration.EntityFramework.Repositories.Omie;
using WebhookOrder = VDC.Integration.APIClient.Shopify.Models.Webhook.WebhookOrder;
using WebhookTypeExtension = VDC.Integration.APIClient.Omie.Enum.Webhook.WebhookTypeExtension;

namespace VDC.Integration.Application.Services
{
    public class WebhookService
    {
        private readonly ShopifyService _shopifyService;
        private readonly TenantService _tenantService;
        private readonly TenantRepository _tenantRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookService> _logger;
        private readonly OmieOrderIntegrationRepository _omieOrderIntegrationRepository;
        private readonly IDistributedCache _cache;

        public WebhookService(IServiceProvider serviceProvider,
            ShopifyService shopifyService,
            TenantService tenantService,
            TenantRepository tenantRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            OmieOrderIntegrationRepository omieOrderIntegrationRepository,
            IDistributedCache cache)
        {
            _shopifyService = shopifyService;
            _tenantService = tenantService;
            _tenantRepository = tenantRepository;
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _omieOrderIntegrationRepository = omieOrderIntegrationRepository;
            _cache = cache;

            using (var scope = serviceProvider.CreateScope())
            {
                _logger = scope.ServiceProvider.GetRequiredService<ILogger<WebhookService>>();
            }
        }

        public WebhookType? GetShopifyWebhookType(string type)
        {
            foreach (WebhookType webhook in Enum.GetValues(typeof(WebhookType)))
            {
                if (webhook.GetTopics().Any(t => t.header == type))
                    return webhook;
            }

            return null;
        }
        public APIClient.Omie.Enum.Webhook.WebhookType? GetOmieWebhookType(string type)
        {
            foreach (APIClient.Omie.Enum.Webhook.WebhookType webhook in Enum.GetValues(typeof(APIClient.Omie.Enum.Webhook.WebhookType)))
            {
                if (WebhookTypeExtension.GetTopics(webhook).Any(t => string.Equals(t, type, StringComparison.OrdinalIgnoreCase)))
                    return webhook;
            }

            return null;
        }

        public bool IsAuthenticShopifyWebhook(string hmacHeader, string body, string secret)
        {
            if (string.IsNullOrWhiteSpace(hmacHeader))
                return false;

            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            string hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

            return hash == hmacHeader;
        }

        public async Task ReadShopifyWebhook(string body, WebhookType type, Tenant tenant)
        {
            switch (type)
            {
                case WebhookType.OrdersCreate:
                    await WebhookShopifyOrdersCreate(body, tenant, false);
                    break;
                case WebhookType.OrdersUpdate:
                    await WebhookShopifyOrdersCreate(body, tenant, true);
                    break;
                default:
                    _logger.LogInformation($"WebhookService:134 | Caiu no default do switch - type {type} - Id {tenant.Id}");
                    break;
            }
        }

        public async Task WebhookShopifyOrdersCreate(string body, Tenant tenant, bool isUpdate)
        {
            var webhookOrder = JsonConvert.DeserializeObject<WebhookOrder>(body);
            _logger.LogWarning("WebhookShopifyOrdersCreate | TenantId: {0} - Pedido Recebido: orderId: {1} - orderPaid? {2}  - Json: {3}", tenant?.Id, webhookOrder?.id, webhookOrder?.financial_status, body);
            try
            {
                if (webhookOrder.closed_at != null
                    || (webhookOrder.tags.Contains("IsIntg-True-Intg")
                    && webhookOrder.fulfillment_status == WebhookOrderFulfillmentStatus.fulfilled
                    && webhookOrder.financial_status == WebhookFinancialStatus.paid))
                    return;

                QueueClient queue = _tenantService.GetQueueClient(tenant, ShopifyQueue.ListOrderQueue);

                var serviceBusMessage = new ServiceBusMessage(new ShopifyListOrderMessage
                {
                    ShopifyId = webhookOrder.id,
                    Body = new { webhookOrder.cancelled_at, webhookOrder.financial_status }
                });

                try
                {
                    var messageWasScheduled = await TryToScheduleMessageFirstAsync(tenant, isUpdate, webhookOrder, queue, serviceBusMessage);

                    if (messageWasScheduled)
                        return;

                    var cache = new TenantOrderCacheDto()
                    {
                        TenantId = tenant.Id,
                        ShopifyOrderId = webhookOrder.id,
                        LastMessageDateUtc = DateTime.UtcNow,
                    };

                    await _cache.SetStringAsync(cache.GetKey(), JsonConvert.SerializeObject(cache));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar setar o cache para o pedido id: {0} - tenantId: {1} | Order: {2}", webhookOrder.id, tenant.Id, body);
                }

                await queue.SendAsync(serviceBusMessage.GetMessage(webhookOrder.id, isUpdate ? tenant.ShopifyData.DelayProcessOfShopifyUpdateOrderMessages : 0));
                await queue.CloseAsync();
                _logger.LogWarning("Send order update {0} - TenantId: {1}", webhookOrder.id, tenant.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading Shopify Order Webhook - {webhookOrder.id} {Environment.NewLine} {body}", ex);
            }
        }

        public async Task ReadOmieWebhook(OmieWebhook webhook, APIClient.Omie.Enum.Webhook.WebhookType webhookType, Tenant tenant)
        {
            (QueueClient Queue, ServiceBusMessage Message)? data = null;

            switch (webhookType)
            {
                case APIClient.Omie.Enum.Webhook.WebhookType.ProductCreate:
                    if (webhook.@event.codigo_familia > 0 &&
                        webhook.@event.inativo != "S")
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListFullProductQueue),
                                new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = webhook.@event.codigo_familia.ToString() }));
                    }
                    break;
                case APIClient.Omie.Enum.Webhook.WebhookType.ProductEdit:
                    if (webhook.@event.codigo_produto > 0)
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListPartialProductQueue),
                                new ServiceBusMessage(new OmieListPartialProductMessage { ProductSkuId = webhook.@event.codigo_produto.Value, ProductSkuCode = webhook.@event.codigo }));
                    }
                    break;
                case APIClient.Omie.Enum.Webhook.WebhookType.ProductRemove:
                    if (webhook.@event.codigo_familia > 0)
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListFullProductQueue),
                                new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = webhook.@event.codigo_familia.ToString() }));
                    }
                    break;
                case APIClient.Omie.Enum.Webhook.WebhookType.Stock:
                    if (webhook.@event.codigo_produto > 0)
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListStockQueue),
                                new ServiceBusMessage(new OmieListStockSkuMessage { ProductSkuId = webhook.@event.codigo_produto.Value, ProductSkuCode = webhook.@event.codigo }));
                    }
                    break;
                case APIClient.Omie.Enum.Webhook.WebhookType.Order:
                    if (!string.IsNullOrWhiteSpace(webhook.@event.codIntPedido))
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListOrderQueue),
                                new ServiceBusMessage(new OmieListOrderMessage { ExternalOrderId = webhook.@event.codIntPedido }));
                    }
                    break;
                default:
                    break;
            }

            if (data != null)
            {
                await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(webhook.messageId));
                await data.Value.Queue.CloseAsync();
            }
        }

        private async Task<bool> ValidAndReturnProductKit(Tenant tenant, WebhookOrder order)
        {
            var orderTags = order.tags.Split(",").Select(x => { return x.Trim(); }).ToList();
            var hasTagProcessedStock = orderTags.Contains(Tags.OrderProcessedStockKit);

            var shopifyData = new ShopifyDataMessage(tenant);
            var client = new ShopifyApi(_serviceProvider, new CancellationTokenSource().Token, shopifyData, shopifyData.ShopifyApps.FirstOrDefault());

            foreach (var item in order.line_items)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var response = await client.Receive(new ProductByIdQuery(item.product_id.Value, "25"));

                    if (response.Result == Result.Error)
                        throw new Exception($"not found product {item.product_id}");

                    var product = response.Data.product;

                    if (product.tags.Select(x => { return x.Trim(); }).Contains("Assinatura", StringComparer.OrdinalIgnoreCase))
                        return true;

                }

            }

            return false;
        }
        private async Task<bool> TryToScheduleMessageFirstAsync(Tenant tenant, bool isUpdate, WebhookOrder webhookOrder, QueueClient queue, ServiceBusMessage serviceBusMessage)
        {
            try
            {
                var keyCache = new TenantOrderCacheDto()
                {
                    TenantId = tenant.Id,
                    ShopifyOrderId = webhookOrder.id,
                }.GetKey();

                var cache = await _cache.GetStringAsync(keyCache);

                if (string.IsNullOrEmpty(cache))
                    return false;

                _logger.LogInformation("TenantId: {0} - Cache: {1}", tenant.Id, cache);

                var tenantOrderCache = JsonConvert.DeserializeObject<TenantOrderCacheDto>(cache);

                var delayInMinutes = 2;

                var scheduleEnqueueTimeUtc = tenantOrderCache.LastMessageDateUtc?.AddMinutes(delayInMinutes);
                var dateTimeUtcNow = DateTime.UtcNow;

                if (scheduleEnqueueTimeUtc < dateTimeUtcNow)
                    scheduleEnqueueTimeUtc = dateTimeUtcNow.AddMinutes(delayInMinutes);

                tenantOrderCache.LastMessageDateUtc = scheduleEnqueueTimeUtc;

                await _cache.SetStringAsync(tenantOrderCache.GetKey(), JsonConvert.SerializeObject(tenantOrderCache));

                var message = serviceBusMessage.GetMessage(webhookOrder.id, isUpdate ? tenant.ShopifyData.DelayProcessOfShopifyUpdateOrderMessages : 0);

                var sequenceMessageScheduled = await queue.ScheduleMessageAsync(message, (DateTime)scheduleEnqueueTimeUtc);
                await queue.CloseAsync();

                return sequenceMessageScheduled > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedido no cache | TenantId: {0} - OrderId: {1}", tenant.Id, webhookOrder.id);
                return false;
            }
        }
    }
}