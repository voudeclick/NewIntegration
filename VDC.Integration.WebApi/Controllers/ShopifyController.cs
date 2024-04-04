using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Shopify;
using VDC.Integration.Application.Services;
using VDC.Integration.Domain.Dtos;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Results;
using VDC.Integration.EntityFramework.Repositories;
using VDC.Integration.WebApi.ViewModels.Shopify;

namespace VDC.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class ShopifyController : BaseController<ShopifyController>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TenantRepository _tenantRepository;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ShopifyService _shopifyService;

        private readonly ILogger<ShopifyRESTClient> _loggerShopifyRESTClient;

        public ShopifyController(ILogger<ShopifyController> logger,
                                IHttpClientFactory httpClientFactory,
                                IConfiguration configuration,
                                TenantRepository tenantRepository,
                                IServiceProvider serviceProvider,
                                ShopifyService ShopifyService)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _tenantRepository = tenantRepository;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _shopifyService = ShopifyService;

            using (var scope = _serviceProvider.CreateScope())
            {
                _loggerShopifyRESTClient = scope.ServiceProvider.GetService<ILogger<ShopifyRESTClient>>();
            }
        }

        [HttpGet("{id}/GetAllWarehouses")]
        [Authorize(Roles = "Administrador,Suporte,Viewer,Lojista")]
        public async Task<IActionResult> GetAllWarehouses(long id)
        {
            if (!CanAcessTenantInformation(id))
            {
                return Unauthorized();
            }

            var result = new Result<dynamic>() { StatusCode = HttpStatusCode.OK };
            try
            {
                var entity = await _tenantRepository.GetById(id);

                if (entity == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                string versionShopify = _configuration.GetSection("Shopify")["Version"];

                if (entity.IntegrationType == IntegrationType.Shopify)
                {
                    _logger.LogInformation($"ShopifyController on tenant {entity.StoreName}", new { Method = nameof(GetAllWarehouses), TenantId = id, ShopifyVersion = versionShopify });

                    var apps = entity.ShopifyData.GetShopifyApps();
                    var app = apps.First();
                    var client = new ShopifyRESTClient(_loggerShopifyRESTClient, _httpClientFactory, entity.ShopifyData.Id.ToString(), entity.ShopifyData.ShopifyStoreDomain, versionShopify, app.ShopifyPassword);

                    var response = await client.Get<ShopifyLocation>("locations.json");

                    result.Value = response;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on ShopifyController", new { Method = nameof(GetAllWarehouses), TenantId = id });
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on ShopifyController GetAllWarehouses", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("GetAllIntegrationFail")]
        public async Task<ActionResult> IntegrationFail(CancellationToken cancellationToken)
        {
            try
            {
                var result = new Result<List<StoreLostedOrdersDto>>() { StatusCode = HttpStatusCode.OK };
                var tenants = await _tenantRepository.GetActiveOrderIntegrationByIntegrationType(IntegrationType.Shopify);

                if (tenants == null)
                {
                    result.AddError("Dados inválidos", "Tenants não encontrados.", GetType().FullName);
                    return BadRequest(result);
                }

                result.Value = await _shopifyService.GetLostOrders(tenants, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("UpdateNoteAttributes/{id}")]
        [Authorize(Roles = "Administrador,Suporte,Viewer,Lojista")]
        public async Task<IActionResult> UpdateNoteAttributes(long id)
        {

            if (!CanAcessTenantInformation(id))
            {
                return Unauthorized();
            }

            var result = new Result<dynamic>() { StatusCode = HttpStatusCode.OK };
            try
            {
                var entity = await _tenantRepository.GetById(id);

                if (entity == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                string versionShopify = _configuration.GetSection("Shopify")["Version"];

                if (entity.IntegrationType == IntegrationType.Shopify)
                {
                    var apps = entity.ShopifyData.GetShopifyApps();
                    var app = apps.First();
                    var client = new ShopifyRESTClient(_loggerShopifyRESTClient, _httpClientFactory, entity.ShopifyData.Id.ToString(), entity.ShopifyData.ShopifyStoreDomain, versionShopify, app.ShopifyPassword);

                    //await _shopifyService.UpdateOrderNoteAttributes(client);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on ShopifyController", new { Method = nameof(GetAllWarehouses), TenantId = id });
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on ShopifyController GetAllWarehouses", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }
    }
}
