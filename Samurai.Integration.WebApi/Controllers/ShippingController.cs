using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Samurai.Integration.APIClient.Nexaas.Models.CarrierService;
using Samurai.Integration.APIClient.Nexaas.Models.Enum;
using Samurai.Integration.APIClient.Nexaas.Models.Results;
using Samurai.Integration.APIClient.Nexaas.Models.Webhook;
using Samurai.Integration.APIClient.Nexaas;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.EntityFramework.Repositories;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Samurai.Integration.APIClient.Nexaas.Models.Requests;
using Castle.Core.Internal;
using System.Net.Http;

namespace Samurai.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly ILogger<ShippingController> _logger;
        private readonly TenantRepository _tenantRepository;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NexaasService _nexaasService;

        public ShippingController(ILogger<ShippingController> logger,
                                TenantRepository tenantRepository,
                                IConfiguration config,
                                IMemoryCache cache,
                                IHttpClientFactory httpClientFactory,
                                NexaasService nexaasService)
        {
            _logger = logger;
            _tenantRepository = tenantRepository;
            _config = config;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _nexaasService = nexaasService;
        }

        [HttpPost("Calculate/{id}")]
        public async Task<IActionResult> CalculateShipping(long id)
        {
            Request.EnableBuffering();
            Stream requestBody = Request.Body;
            requestBody.Seek(0, System.IO.SeekOrigin.Begin);
            string body = await new StreamReader(requestBody).ReadToEndAsync();

            _logger.LogWarning($"{id} - {(Request.Headers["X-Shopify-Shop-Domain"] == StringValues.Empty ? "--Empty--" : Request.Headers["X-Shopify-Shop-Domain"].ToString())} Call to CalculateShipping - {body}");

            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<ShippingRequest>(body);
            var cacheKey = $"CalculateShipping-{id}";
            var tenant = await _cache.GetOrCreateAsync(cacheKey, context =>
            {
                context.SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
                return _tenantRepository.GetById(id);
            });

            try
            {
                if (tenant == null)
                    throw new Exception("Tenant not found");

                if (tenant.Type == Domain.Enums.TenantType.Nexaas)
                {
                    if (tenant.NexaasData.IsPickupPointEnabled)
                    {

                        var _client = new NexaasApiClient(_httpClientFactory, tenant.NexaasData.Url, tenant.NexaasData.Token, _config.GetSection("Nexaas")["Version"]);
                        var response = await _client.Post<NexaasApiPickupPointSearchResult>("pickup_points/search", new NexaasApiPickupPointSearchRequest
                        {
                            search = new NexaasApiPickupPointSearchRequest.Search
                            {
                                zip_code = request.rate.destination.postal_code,
                                stock_skus = request.rate.items.Select(x => new NexaasApiPickupPointSearchRequest.Stock_Skus
                                {
                                    sku_code = _nexaasService.SplitShopifySkuCode(x.sku).code,
                                    quantity = x.quantity
                                }).ToList()
                            }
                        });

                        return new JsonResult(new
                        {
                            rates = response.pickup_points.Select(x => new
                            {
                                service_code = $"pickup${x.stock_id}|{x.organization.average_withdrawal_term}",
                                service_name = string.Format(tenant.NexaasData.ServiceNameTemplate.IsNullOrEmpty() ? "Retira na Loja - {0}" : tenant.NexaasData.ServiceNameTemplate, x.organization.name),
                                total_price = (int)(x.organization.average_withdrawal_price * 100),
                                description = string.Format(tenant.NexaasData.DeliveryTimeTemplate.IsNullOrEmpty() ? "{0} dia(s) útei(s)" : tenant.NexaasData.DeliveryTimeTemplate, x.organization.average_withdrawal_term),
                                currency = "BRL"
                            }).ToList()
                        });
                    }
                }

            }
            catch
            {
                _cache.Remove(cacheKey);
                throw;
            }

            return Ok();
        }
    }
}
