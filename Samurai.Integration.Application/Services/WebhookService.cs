using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.Bling.Models.Webhook;
using Samurai.Integration.APIClient.Nexaas.Models.Enum;
using Samurai.Integration.APIClient.Nexaas.Models.Webhook;
using Samurai.Integration.APIClient.Omie.Enum.Webhook;
using Samurai.Integration.APIClient.Omie.Models.Webhook;
using Samurai.Integration.APIClient.Pier8.Models.Webhook;
using Samurai.Integration.APIClient.PluggTo;
using Samurai.Integration.APIClient.PluggTo.Enum;
using Samurai.Integration.APIClient.PluggTo.Models.Requests;
using Samurai.Integration.APIClient.PluggTo.Models.Results;
using Samurai.Integration.APIClient.PluggTo.Models.Webhooks;
using Samurai.Integration.APIClient.Shopify.Enum.Webhook;
using Samurai.Integration.APIClient.Shopify.Models;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.APIClient.Shopify.Models.Webhooks;
using Samurai.Integration.APIClient.Tray.Enum;
using Samurai.Integration.APIClient.Tray.Models;
using Samurai.Integration.Application.Actors.API;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.Nexaas;
using Samurai.Integration.Domain.Messages.Omie;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Messages.Tray.OrderActor;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.EntityFramework.Repositories.Omie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmieEnum = Samurai.Integration.APIClient.Omie.Enum.Webhook;
using ShopifyEnum = Samurai.Integration.APIClient.Shopify.Enum.Webhook;
using WebhookOrder = Samurai.Integration.APIClient.Shopify.Models.Webhooks.WebhookOrder;

namespace Samurai.Integration.Application.Services
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

        public ShopifyEnum.WebhookType? GetShopifyWebhookType(string type)
        {
            foreach (ShopifyEnum.WebhookType webhook in Enum.GetValues(typeof(ShopifyEnum.WebhookType)))
            {
                if (webhook.GetTopics().Any(t => t.header == type))
                    return webhook;
            }

            return null;
        }
        public TrayWebhookType? GetTrayWebhookType(string type)
        {
            foreach (TrayWebhookType webhook in Enum.GetValues(typeof(TrayWebhookType)))
            {
                if (webhook.GetTopics().Any(t => t.value == type))
                    return webhook;
            }

            return null;
        }
        public OmieEnum.WebhookType? GetOmieWebhookType(string type)
        {
            foreach (OmieEnum.WebhookType webhook in Enum.GetValues(typeof(OmieEnum.WebhookType)))
            {
                if (webhook.GetTopics().Any(t => string.Equals(t, type, StringComparison.OrdinalIgnoreCase)))
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

        public async Task ReadShopifyWebhook(string body, ShopifyEnum.WebhookType type, Tenant tenant)
        {
            switch (type)
            {
                case ShopifyEnum.WebhookType.OrdersCreate:
                    await WebhookShopifyOrdersCreate(body, tenant, false);
                    break;
                case ShopifyEnum.WebhookType.OrdersUpdate:
                    await WebhookShopifyOrdersCreate(body, tenant, true);
                    break;
                default:
                    _logger.LogInformation($"WebhookService:134 | Caiu no default do switch - type {type} - Id {tenant.Id}");
                    break;
            }
        }


        //public async Task ReadTrayWebhook(TrayWebhook body, TrayWebhookType type, Tenant tenant)
        //{
        //    switch (type)
        //    {
        //        case TrayWebhookType.Order:
        //            await WebhookTrayOrders(body, tenant);
        //            break;
        //        default:
        //            break;
        //    }
        //}

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
                    _logger.LogError(ex, "Erro ao tentar setar o cache para o pedido id: {0} - tenantId: {1} | Order: {2}", webhookOrder.id,tenant.Id, body);
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

        //public async Task WebhookTrayOrders(TrayWebhook webhook, Tenant tenant)
        //{
        //    try
        //    {
        //        QueueClient queue = _tenantService.GetQueueClient(tenant, TrayQueue.ListOrderQueue);

        //        var serviceBusMessage = new ServiceBusMessage(new TrayListOrderMessage
        //        {
        //            OrderId = webhook.scope_id
        //        });
        //        await queue.SendAsync(serviceBusMessage.GetMessage(webhook.scope_id));
        //        await queue.CloseAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error reading Tray Order Webhook - {webhook.scope_id} {Environment.NewLine}", ex);
        //    }
        //}

        public async Task ReadNexaasWebhook(NexaasWebhook webhook, Tenant tenant)
        {
            (QueueClient Queue, ServiceBusMessage Message)? data = null;

            data = webhook switch
            {
                NexaasWebhook w when (w.object_type == NexaasWebhookType.ProductBrand && w.@event == "updated") =>
                                                    (_tenantService.GetQueueClient(tenant, NexaasQueue.ListVendorQueue),
                                                    new ServiceBusMessage(new NexaasListVendorMessage { Id = webhook.object_id.Value })),
                NexaasWebhook w when (w.object_type == NexaasWebhookType.StockSku && new List<string> { "added", "removed" }.Contains(w.@event)) =>
                                                    (_tenantService.GetQueueClient(tenant, NexaasQueue.ListStockQueue),
                                                    new ServiceBusMessage(new NexaasListStockSkuMessage { StockSkuId = webhook.object_id.Value })),
                NexaasWebhook w when (w.object_type == NexaasWebhookType.ProductSku && w.@event == "created") =>
                                                    (_tenantService.GetQueueClient(tenant, NexaasQueue.ListPartialProductQueue),
                                                    new ServiceBusMessage(new NexaasListPartialProductMessage { ProductSkuId = webhook.object_id.Value, NewSku = true })),
                NexaasWebhook w when (w.object_type == NexaasWebhookType.ProductSku && w.@event == "updated") =>
                                                    (_tenantService.GetQueueClient(tenant, NexaasQueue.ListPartialProductQueue),
                                                    new ServiceBusMessage(new NexaasListPartialProductMessage { ProductSkuId = webhook.object_id.Value, NewSku = false })),
                NexaasWebhook w when (w.object_type == NexaasWebhookType.ProductCategory && w.@event == "updated") =>
                                                    (_tenantService.GetQueueClient(tenant, ShopifyQueue.ListCategoryProductsToUpdateQueue),
                                                    new ServiceBusMessage(new ShopifyListCategoryProductsToUpdateMessage { CategoryId = webhook.object_id.ToString() })),
                NexaasWebhook w when (w.object_type == NexaasWebhookType.Order && w.@event == "updated") =>
                                                    (_tenantService.GetQueueClient(tenant, NexaasQueue.ListOrderQueue),
                                                    new ServiceBusMessage(new NexaasListOrderMessage { NexaasOrderId = webhook.object_id })),
                _ => null
            };

            if (data != null)
            {
                if (new List<NexaasWebhookType> { NexaasWebhookType.Order, NexaasWebhookType.StockSku }.Contains(webhook.object_type) == false
                    || webhook.organization_id == tenant.NexaasData.OrganizationId)
                {
                    await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(webhook.object_id));
                    await data.Value.Queue.CloseAsync();
                }
            }
        }

        public async Task ReadOmieWebhook(OmieWebhook webhook, OmieEnum.WebhookType webhookType, Tenant tenant)
        {
            (QueueClient Queue, ServiceBusMessage Message)? data = null;

            switch (webhookType)
            {
                case OmieEnum.WebhookType.ProductCreate:
                    if (webhook.@event.codigo_familia > 0 &&
                        webhook.@event.inativo != "S")
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListFullProductQueue),
                                new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = webhook.@event.codigo_familia.ToString() }));
                    }
                    break;
                case OmieEnum.WebhookType.ProductEdit:
                    if (webhook.@event.codigo_produto > 0)
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListPartialProductQueue),
                                new ServiceBusMessage(new OmieListPartialProductMessage { ProductSkuId = webhook.@event.codigo_produto.Value, ProductSkuCode = webhook.@event.codigo }));
                    }
                    break;
                case OmieEnum.WebhookType.ProductRemove:
                    if (webhook.@event.codigo_familia > 0)
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListFullProductQueue),
                                new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = webhook.@event.codigo_familia.ToString() }));
                    }
                    break;
                case OmieEnum.WebhookType.Stock:
                    if (webhook.@event.codigo_produto > 0)
                    {
                        data = (_tenantService.GetQueueClient(tenant, OmieQueue.ListStockQueue),
                                new ServiceBusMessage(new OmieListStockSkuMessage { ProductSkuId = webhook.@event.codigo_produto.Value, ProductSkuCode = webhook.@event.codigo }));
                    }
                    break;
                case OmieEnum.WebhookType.Order:
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

        public async Task ReadPier8Webhook(Pier8Webhook webhook, Tenant tenant)
        {

            var queue = _tenantService.GetQueueClient(tenant, Pier8Queue.WebHookProcessQueue);

            List<Task> tasks = new List<Task>();

            var message = new ServiceBusMessage(webhook.pedido);
            await queue.SendAsync(message.GetMessage($"{webhook.pedido.idPier}{webhook.pedido.statusCodigo}"));
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

        public async Task ReadStockUpdateBlingWebhook(BlingStockUpdateModel blingStockUpdateModel, Tenant tenant)
        {
            var productCode = blingStockUpdateModel.retorno.estoques.Select(x => x.estoque.codigo).FirstOrDefault();

            var message = tenant.IntegrationType switch
            {
                IntegrationType.Shopify => new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = productCode }),
                IntegrationType.SellerCenter => new ServiceBusMessage(new BlingListProductMessage { ExternalId = productCode }),
                _ => throw new NotImplementedException()
            };

            (QueueClient Queue, ServiceBusMessage Message)? data = (_tenantService.GetQueueClient(tenant, BlingQueue.ListFullProductQueue), message);

            await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(productCode));
            await data.Value.Queue.CloseAsync();
        }

        public async Task ReadOrderUpdateBlingWebhook(BlingOrderUpdateModel blingOrderUpdateModel, Tenant tenant)
        {
            var orderNumber = blingOrderUpdateModel.retorno?.pedidos?.FirstOrDefault().pedido?.numero;

            var message = tenant.IntegrationType switch
            {
                //TODO - Bling list order to shopify
                IntegrationType.SellerCenter => new ServiceBusMessage(new BlingListOrderMessage { OrderNumber = orderNumber }),
                IntegrationType.Shopify => new ServiceBusMessage(new BlingListOrderMessage { OrderNumber = orderNumber }),
                _ => throw new NotImplementedException()
            };

            (QueueClient Queue, ServiceBusMessage Message)? data = (_tenantService.GetQueueClient(tenant, BlingQueue.ListOrderQueue), message);

            await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(orderNumber));
            await data.Value.Queue.CloseAsync();
        }

        public async Task ReadOrderCreateSellerCenterWebhook(Tenant tenant, string orderNumber)
        {
            var message = new ServiceBusMessage(new ListOrderMessage { OrderNumber = orderNumber });

            (QueueClient Queue, ServiceBusMessage Message)? data = (_tenantService.GetQueueClient(tenant, SellerCenterQueue.ListNewOrdersQueue), message);

            await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(orderNumber));
            await data.Value.Queue.CloseAsync();
        }

        private async Task<(Tenant tenant, PluggToApiListProductsResult.Produto product)> GetProductPluggTo(PluggToWebhook webhook, Tenant tenant)
        {
            try
            {
                var product = new PluggToApiListProductsResult.Produto();

                var userId = webhook.user.ToString();

                var _client = new PluggToApiClient(_httpClientFactory, _configuration.GetSection("PluggTo")["Url"], tenant.PluggToData.ClientId, tenant.PluggToData.ClientSecret,
                                                    tenant.PluggToData.Username, tenant.PluggToData.Password);

                var param = new Dictionary<string, string>() { };
                param.Add("user_id", webhook.user.ToString());
                param.Add("id", webhook.id);

                var response = await _client.Get<PluggToApiListProductsResult>("products", param);
                if (response.result.Any())
                {
                    product = response.result.FirstOrDefault().Product;

                    var supplierId = product.supplier_id;

                    if (supplierId != null)
                        userId = supplierId;
                }
                _logger.LogWarning("UserId: " + userId + " Webhook.user: " + webhook.user);
                if (userId != webhook.user.ToString())
                {
                    tenant = await _tenantRepository.GetUserPluggTo(userId);

                    if (tenant == null)
                        throw new Exception("Tenant inválido");
                }

                return (tenant, product);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private async Task<(Tenant tenant, PluggToApiOrderResult order)> GetOrderPluggTo(PluggToWebhook webhook, Tenant tenant)
        {
            try
            {
                var userId = webhook.user.ToString();

                var _client = new PluggToApiClient(_httpClientFactory, _configuration.GetSection("PluggTo")["Url"], tenant.PluggToData.ClientId, tenant.PluggToData.ClientSecret,
                                                    tenant.PluggToData.Username, tenant.PluggToData.Password);

                var response = await _client.Get<PluggToApiOrderResult>($"orders/{webhook.id}");

                if (response.Order != null && !string.IsNullOrEmpty(response.Order.original_id))
                {
                    var ret = JsonConvert.DeserializeObject<PluggToOriginalId>(response.Order.original_id);
                    if (ret != null)
                        userId = ret.UserId;
                }

                if (userId != webhook.user.ToString())
                {
                    tenant = await _tenantRepository.GetUserPluggTo(userId);

                    if (tenant == null)
                        throw new Exception("Tenant inválido");
                }

                return (tenant, response);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task ReadPluggToWebhook(PluggToWebhook webhook, Tenant tenant)
        {

            if (webhook.type.ToUpper() == PluggToWebhookType.orders.ToString().ToUpper() &&
                webhook.action.ToUpper() == PluggToWebhookAction.updated.ToString().ToUpper())
            {
                var orderByTenant = await GetOrderPluggTo(webhook, tenant);

                if (orderByTenant.tenant != null)
                    tenant = orderByTenant.tenant;

                if (orderByTenant.order?.Order?.status != PluggToOrderStatus.pending.ToString())
                {
                    var message = tenant.IntegrationType switch
                    {
                        IntegrationType.SellerCenter => new ServiceBusMessage(new PluggToListOrderMessage
                        {
                            ExternalOrderId = webhook.id,
                            Order = orderByTenant.order != null ? JsonConvert.SerializeObject(orderByTenant.order) : null
                        }),
                        _ => throw new NotImplementedException()
                    };

                    (QueueClient Queue, ServiceBusMessage Message)? data = (_tenantService.GetQueueClient(tenant, PluggToQueue.ListOrderQueue), message);

                    await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(webhook.id));
                    await data.Value.Queue.CloseAsync();
                }
            }

            if (webhook.type.ToUpper() == PluggToWebhookType.products.ToString().ToUpper())
            {
                var productByTenant = await GetProductPluggTo(webhook, tenant);

                if (productByTenant.tenant != null)
                    tenant = productByTenant.tenant;

                var message = tenant.IntegrationType switch
                {
                    IntegrationType.SellerCenter => new ServiceBusMessage(new PluggToListProductMessage
                    {
                        AccountUserId = webhook.user.ToString(),
                        ExternalId = webhook.id,
                        Product = productByTenant.product != null ? JsonConvert.SerializeObject(productByTenant.product) : null
                    }),
                    _ => throw new NotImplementedException()
                };

                (QueueClient Queue, ServiceBusMessage Message)? data = (_tenantService.GetQueueClient(tenant, PluggToQueue.ListFullProductQueue), message);

                await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(webhook.id));
                await data.Value.Queue.CloseAsync();

            }
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
        //public async Task ReadAliExpressWebhook(long productId, Tenant tenant)
        //{
        //    var message = tenant.IntegrationType switch
        //    {
        //        IntegrationType.Tray => new ServiceBusMessage(new AliExpressListProductMessage
        //        {
        //            ProductId = productId,
        //            LocalCountry = "BR",
        //            LocalLanguage = "pt"
        //        }),
        //        _ => throw new NotImplementedException()
        //    };

        //    (QueueClient Queue, ServiceBusMessage Message)? data = (_tenantService.GetQueueClient(tenant, AliExpressQueue.ListFullProductQueue), message);

        //    await data.Value.Queue.SendAsync(data.Value.Message.GetMessage(productId));
        //    await data.Value.Queue.CloseAsync();

        //}
    }
}

//"{"TenantId":70,"ShopifyOrderId":4725501329592,"LastMessageDateUtc":"2022-09-26T14:37:35.8283879Z"}"