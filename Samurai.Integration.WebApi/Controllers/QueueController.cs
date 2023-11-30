using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.Shopify;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress.Order;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Nexaas;
using Samurai.Integration.Domain.Messages.Omie;
using Samurai.Integration.Domain.Messages.Pier8;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Shopify.Models.Results;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.WebApi.ViewModels;
using Samurai.Integration.WebApi.ViewModels.Queue;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class QueueController : BaseController<QueueController>
    {
        private readonly TenantRepository _tenantRepository;
        private readonly TenantService _tenantService;
        private readonly ILogger<ShopifyRESTClient> _loggerShopifyRESTClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public QueueController(ILogger<QueueController> logger,
                                TenantRepository tenantRepository,
                                TenantService tenantService,
                                IHttpClientFactory httpClientFactory,
                                IConfiguration configuration,
                                IServiceProvider serviceProvider)
            : base(logger)
        {
            _tenantRepository = tenantRepository;
            _tenantService = tenantService;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

            using (var scope = _serviceProvider.CreateScope())
            {
                _loggerShopifyRESTClient = scope.ServiceProvider.GetService<ILogger<ShopifyRESTClient>>();
            }
        }

        [HttpPost("Order/ShopifyFilterDate")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostShopifyOrderFilterDateMessage([FromBody] QueueOrderShopifyFilterDateViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                var tenant = await _tenantRepository.GetById(message.TenantId);
                IFormatProvider culture = new CultureInfo("en-US", true);
                var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff'Z'";
                var dateMin = message.DateMin.ToString(format);
                var dateMax = message.DateMax.ToString(format);
                string versionShopify = tenant.Id == 57 ?
                    _configuration.GetSection("Shopify")["NewVersion"] : _configuration.GetSection("Shopify")["Version"];

                var apps = tenant.ShopifyData.GetShopifyApps();
                var app = apps.First();
                var client = new ShopifyRESTClient(_loggerShopifyRESTClient, _httpClientFactory, tenant.ShopifyData.Id.ToString(), tenant.ShopifyData.ShopifyStoreDomain, versionShopify, app.ShopifyPassword);

                var method = $"orders.json?limit=50&createdAtMin={dateMin}&createdAtMax={dateMax}";

                var response = await client.Get<Orders>(method);

                var ordersForIntegration = response.orders.Where(x => (x.tags is null) || !x.tags.Contains("IsIntg-True-Intg")).Select(s => s.id).ToList();

                await Task.WhenAll(
                    ordersForIntegration.Select(async orderId => await _tenantService.EnqueueMessageShopifyOrder(tenant, orderId, ShopifyQueue.ListOrderQueue))
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostShopifyOrderMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostShopifyOrderMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            return Ok(result);
        }

        [HttpPost("Order/Shopify")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostShopifyOrderMessage([FromBody] QueueOrderShopifyViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };

            try
            {
                var tenant = await _tenantRepository.GetById(message.TenantId);
                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var ordersIds = message.OrderId.Split(";").Select(s => long.Parse(s));

                await Task.WhenAll(
                    ordersIds.Select(async orderId => await _tenantService.EnqueueMessageShopifyOrder(tenant, orderId, ShopifyQueue.ListOrderQueue))
                    );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostShopifyOrderMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostShopifyOrderMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Order/SC")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostSCOrderMessage([FromBody] QueueOrderSCViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.OrderNumber))
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, SellerCenterQueue.ListOrderQueue);
                var serviceBusMessage = new ServiceBusMessage(new ListOrderMessage { OrderNumber = message.OrderNumber });
                await queue.SendAsync(serviceBusMessage.GetMessage(message.OrderNumber));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostSCOrderMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostSCOrderMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Collection/Shopify")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostShopifyAllCollectionsMessage([FromBody] QueueAllCollectionsShopifyViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || message.TenantId <= 0)
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.ProductIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                await _tenantService.CreateQueue(tenant, ShopifyQueue.UpdateAllCollectionsQueue);

                var queue = _tenantService.GetQueueClient(tenant, ShopifyQueue.UpdateAllCollectionsQueue);
                var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateAllCollectionsMessage());
                await queue.SendAsync(serviceBusMessage.GetMessage(message.TenantId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostShopifyAllCollectionsMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostShopifyAllCollectionsMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Product/Millennium")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostProductMillenniumMessage([FromBody] QueueProductMilleniumViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ProductId))
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.ProductIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Millennium)
                {
                    result.AddError("Dados inválidos", "Tenant não é Millenniun.", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, MillenniumQueue.ListFullProductQueue);
                var serviceBusMessage = new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = message.ProductId });
                await queue.SendAsync(serviceBusMessage.GetMessage(message.ProductId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostProductMillenniumMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostProductMillenniumMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Order/Millennium")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostMillenniumOrderMessage([FromBody] QueueOrderMillenniumViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };

            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ExternalOrderId))
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Millennium)
                {
                    result.AddError("Dados inválidos", "Tenant não é Millenniun.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var ordersIds = message.ExternalOrderId.Split(";");

                var count = 0;
                foreach (var externalOrderId in ordersIds)
                {
                    if (string.IsNullOrWhiteSpace(externalOrderId))
                    {
                        count++;
                        continue;
                    }

                    var queue = _tenantService.GetQueueClient(tenant, MillenniumQueue.ListOrderQueue);
                    var serviceBusMessage = new ServiceBusMessage(new MillenniumListOrderMessage { ExternalOrderId = externalOrderId });
                    await queue.SendAsync(serviceBusMessage.GetMessage(externalOrderId));
                    await queue.CloseAsync();
                }

                if (count > 0)
                {
                    result.AddError("Dados inválidos", $"Ids inválidos {count} | Ids válidos {ordersIds.Count() - count}", GetType().FullName);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostMillenniumOrderMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostMillenniumOrderMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Product/Nexaas")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostProductNexaasMessage([FromBody] QueueProductNexaasViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null)
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                if (message.ListAllProducts != true && (message.ProductId ?? 0) <= 0)
                {
                    result.AddError("Dados inválidos", "É necessário informar um código de produto ou a flag ListAllProducts como true.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Nexaas)
                {
                    result.AddError("Dados inválidos", "Tenant não é Nexaas.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.ProductIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var productId = message.ListAllProducts == true ? 0 : message.ProductId;
                var queue = _tenantService.GetQueueClient(tenant, message.ListAllProducts == true ? NexaasQueue.ListAllProductsQueue : NexaasQueue.ListFullProductQueue);
                var serviceBusMessage = message.ListAllProducts == true ? new ServiceBusMessage(new NexaasListAllProductsMessage()) : new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = productId.ToString() });
                await queue.SendAsync(serviceBusMessage.GetMessage(productId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostProductNexaasMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostProductNexaasMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Order/Nexaas")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostNexaasOrderMessage([FromBody] QueueOrderNexaasViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ExternalOrderId))
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Nexaas)
                {
                    result.AddError("Dados inválidos", "Tenant não é Nexaas.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, NexaasQueue.ListOrderQueue);
                var serviceBusMessage = new ServiceBusMessage(new NexaasListOrderMessage { ExternalOrderId = message.ExternalOrderId });
                await queue.SendAsync(serviceBusMessage.GetMessage(message.ExternalOrderId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostNexaasOrderMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostNexaasOrderMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Product/Omie")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostProductOmieMessage([FromBody] QueueProductOmieViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null)
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                if (message.ListAllProducts != true && (message.ProductId ?? 0) <= 0)
                {
                    result.AddError("Dados inválidos", "É necessário informar um código de produto ou a flag ListAllProducts como true.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Omie)
                {
                    result.AddError("Dados inválidos", "Tenant não é Omie.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.ProductIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var productId = message.ListAllProducts == true ? 0 : message.ProductId;
                var queue = _tenantService.GetQueueClient(tenant, message.ListAllProducts == true ? OmieQueue.ListAllProductsQueue : OmieQueue.ListFullProductQueue);
                var serviceBusMessage = message.ListAllProducts == true ? new ServiceBusMessage(new OmieListAllProductsMessage()) : new ServiceBusMessage(new ShopifyListERPFullProductMessage { ExternalId = productId.ToString() });
                await queue.SendAsync(serviceBusMessage.GetMessage(productId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostProductOmieMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostProductOmieMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Order/Omie")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostOmieOrderMessage([FromBody] QueueOrderOmieViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ExternalOrderId))
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Omie)
                {
                    result.AddError("Dados inválidos", "Tenant não é Omie.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, OmieQueue.ListOrderQueue);
                var serviceBusMessage = new ServiceBusMessage(new OmieListOrderMessage { ExternalOrderId = message.ExternalOrderId });
                await queue.SendAsync(serviceBusMessage.GetMessage(message.ExternalOrderId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostOmieOrderMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostOmieOrderMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Product/Bling")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostProductBlingMessage([FromBody] QueueProductBlingViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null)
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                if (message.ListAllProducts != true && string.IsNullOrWhiteSpace(message.ProductId))
                {
                    result.AddError("Dados inválidos", "É necessário informar um código de produto ou a flag ListAllProducts como true.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Bling)
                {
                    result.AddError("Dados inválidos", "Tenant não é Bling.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.ProductIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var productId = message.ListAllProducts == true ? string.Empty : message.ProductId;
                var queue = _tenantService.GetQueueClient(tenant, message.ListAllProducts == true ? BlingQueue.ListAllProductsQueue : BlingQueue.ListFullProductQueue);
                var serviceBusMessage = message.ListAllProducts == true ? new ServiceBusMessage(new BlingListAllProductsMessage()) : new ServiceBusMessage(new BlingListProductMessage { ExternalId = productId.ToString() });
                await queue.SendAsync(serviceBusMessage.GetMessage(productId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostProductBlingMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostProductBlingMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Order/Bling")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostBlingOrderMessage([FromBody] QueueOrderBlingViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ExternalOrderId))
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.Bling)
                {
                    result.AddError("Dados inválidos", "Tenant não é Bling.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, BlingQueue.ListOrderQueue);
                var serviceBusMessage = new ServiceBusMessage(new BlingListOrderMessage { OrderNumber = message.ExternalOrderId });
                await queue.SendAsync(serviceBusMessage.GetMessage(message.ExternalOrderId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostBlingOrderMessage Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostBlingOrderMessage Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Product/PluggTo")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostProductPluggToMessage([FromBody] QueueProductPluggToViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null)
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                if (message.ListAllProducts != true && (string.IsNullOrWhiteSpace(message.ExternalId) && string.IsNullOrWhiteSpace(message.ProductCode)))
                {
                    result.AddError("Dados inválidos", "É necessário informar um código de produto ou a flag ListAllProducts como true.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.PluggTo)
                {
                    result.AddError("Dados inválidos", "Tenant não é PluggTo.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.ProductIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var sku = (message.ListAllProducts == true || string.IsNullOrWhiteSpace(message.ProductCode)) ? string.Empty : message.ProductCode;
                var externalId = (message.ListAllProducts == true || string.IsNullOrWhiteSpace(message.ExternalId)) ? string.Empty : message.ExternalId;

                var accountUserId = tenant.PluggToData.AccountUserId;
                var accountSellerId = tenant.PluggToData.AccountSellerId;

                var queue = _tenantService.GetQueueClient(tenant, message.ListAllProducts == true ? PluggToQueue.ListAllProductsQueue : PluggToQueue.ListFullProductQueue);

                var serviceBusMessage = message.ListAllProducts == true ?
                    new ServiceBusMessage(new PluggToListAllProductsMessage() { AccountUserId = accountUserId, AccountSellerId = accountSellerId }) :
                    new ServiceBusMessage(new PluggToListProductMessage { AccountUserId = accountUserId, AccountSellerId = accountSellerId, Sku = sku, ExternalId = externalId });

                await queue.SendAsync(serviceBusMessage.GetMessage(sku ?? externalId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostProductPluggToMessage Post");

                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostProductPluggToMessage Post", ex.Message, GetType().FullName);

                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Order/PluggTo")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostPluggToOrderMessage([FromBody] QueueOrderPluggToViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ExternalOrderId))
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.PluggTo)
                {
                    result.AddError("Dados inválidos", "Tenant não é PluggTo.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, PluggToQueue.ListOrderQueue);
                var serviceBusMessage = new ServiceBusMessage(new PluggToListOrderMessage { ExternalOrderId = message.ExternalOrderId });

                await queue.SendAsync(serviceBusMessage.GetMessage(message.ExternalOrderId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostPluggToOrderMessage Post");

                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostPluggToOrderMessage Post", ex.Message, GetType().FullName);

                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Order/AliExpress")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> PostAliExpressOrderMessage([FromBody] QueueOrderAliExpressViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || message.AliExpressOrderId <= 0)
                {
                    result.AddError("Dados inválidos", "mensagem vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Type != TenantType.AliExpress)
                {
                    result.AddError("Dados inválidos", "Tenant não é AliExpress.", GetType().FullName);
                    return BadRequest(result);
                }

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                {
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, AliExpressQueue.ListOrderQueue);
                var serviceBusMessage = new ServiceBusMessage(new AliExpressGetOrderMessage
                {
                    AliExpressOrdersIds = new List<AliExpressGetOrderMessage.AliExpressOrder>()
                    {
                        new AliExpressGetOrderMessage.AliExpressOrder() { AliExpressOrderId = message.AliExpressOrderId }
                    }
                });

                await queue.SendAsync(serviceBusMessage.GetMessage(message.AliExpressOrderId));
                await queue.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on PostAliExpressOrderMessage Post");

                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on PostAliExpressOrderMessage Post", ex.Message, GetType().FullName);

                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("{tenantId}/Count")]
        [Authorize(Roles = "Administrador,Suporte,Viewer,Lojista")]
        public async Task<IActionResult> GetQueuesCount(long tenantId)
        {
            if (!CanAcessTenantInformation(tenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result<List<QueueCount>>() { StatusCode = HttpStatusCode.OK };
            try
            {
                var tenant = await _tenantRepository.GetById(tenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                result.Value = await _tenantService.GetQueuesCount(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GetQueueCount Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on GetQueueCount Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("GetQueuesOverflow")]
        [Authorize(Roles = "Administrador,Suporte,Viewer")]
        public async Task<IActionResult> GetQueuesOverflow()
        {
            var result = new Domain.Results.Result<List<OfOverflow>>() { StatusCode = HttpStatusCode.OK };
            try
            {
                var tenant = await _tenantRepository.GetAll();

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                result.Value = await _tenantService.GetQueuesOverflow(tenant
                                                                      .Where(x => x.Status && (x.OrderIntegrationStatus || x.ProductIntegrationStatus))
                                                                      .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GetQueuesOverflow Get");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on GetQueuesOverflow Get", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            result.Value = result.Value.OrderBy(o => o.TanantName).ToList();
            return Ok(result);
        }

        [HttpPost("Pier8/UpdateTrackingByIdShopify")]
        [Authorize(Roles = "Administrador,Suporte,Lojista")]
        public async Task<IActionResult> UpdateTrackingByIdShopify([FromBody] Pier8UpdateTrackingByIdsViewModel message)
        {
            if (!CanAcessTenantInformation(message.TenantId))
            {
                return Unauthorized();
            }

            var result = new Domain.Results.Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ExternalOrderId))
                {
                    result.AddError("Dados inválidos", "ExternalOrderId vazia.", GetType().FullName);
                    return BadRequest(result);
                }

                var tenant = await _tenantRepository.GetById(message.TenantId);

                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (!tenant.EnablePier8Integration)
                {
                    result.AddError("Dados inválidos", "Integração Pier8 não está ativa", GetType().FullName);
                    return BadRequest(result);
                }

                var queue = _tenantService.GetQueueClient(tenant, Pier8Queue.ProcessUpdateTrackingQueue);

                foreach (var order in message.Orders)
                {
                    var serviceBusMessage = new ServiceBusMessage(new Pier8UpdateTrackingMessage { ExternalOrderId = order });
                    await queue.SendAsync(serviceBusMessage.GetMessage(order));
                }

                await queue.CloseAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on UpdateTrackingByIdShopify Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on UpdateTrackingByIdShopify Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

    }
}