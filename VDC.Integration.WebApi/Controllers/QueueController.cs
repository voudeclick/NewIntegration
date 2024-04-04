using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Shopify;
using VDC.Integration.Application.Services;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Messages.Millennium;
using VDC.Integration.Domain.Messages.Omie;
using VDC.Integration.Domain.Messages.ServiceBus;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Queues;
using VDC.Integration.Domain.Shopify.Models.Results;
using VDC.Integration.EntityFramework.Repositories;
using VDC.Integration.WebApi.ViewModels;

namespace VDC.Integration.WebApi.Controllers
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
                string versionShopify = _configuration.GetSection("Shopify")["Version"];

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

    }
}