using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Omie.Models.Webhook;
using VDC.Integration.APIClient.Shopify.Enum.Webhook;
using VDC.Integration.Application.Services;
using VDC.Integration.Domain.Dtos;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Results;
using VDC.Integration.EntityFramework.Repositories;
using WebhookOrder = VDC.Integration.APIClient.Shopify.Models.Webhook.WebhookOrder;

namespace VDC.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly TenantRepository _tenantRepository;
        private readonly TenantService _tenantService;
        private readonly WebhookService _webhookService;
        private readonly IDistributedCache _cache;

        public WebhookController(ILogger<WebhookController> logger,
                                TenantRepository tenantRepository,
                                TenantService tenantService,
                                WebhookService webhookService,
                                IDistributedCache cache)
        {
            _logger = logger;
            _tenantRepository = tenantRepository;
            _tenantService = tenantService;
            _webhookService = webhookService;
            _cache = cache;
        }

        [HttpPost("Shopify/{id}")]
        public async Task<IActionResult> PostShopify(long id)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };
            WebhookType? webhookType = default;
            string body = default;
            try
            {
                _logger.LogInformation("Headers: {0}", JsonConvert.SerializeObject(Request.Headers));
                var headers = Request.Headers;
                webhookType = _webhookService?.GetShopifyWebhookType(headers.FirstOrDefault(kvp => kvp.Key.Equals("X-Shopify-Topic", StringComparison.OrdinalIgnoreCase)).Value);
                var hmacHeader = headers.FirstOrDefault(kvp => kvp.Key.Equals("X-Shopify-Hmac-SHA256", StringComparison.OrdinalIgnoreCase)).Value;

                var tenant = await _tenantRepository.GetById(id);

                var shopifyApp = tenant.ShopifyData?.GetShopifyApps().FirstOrDefault(a => a.Webhook);

                Request.EnableBuffering();
                var requestBody = Request.Body;
                requestBody.Seek(0, SeekOrigin.Begin);
                body = await new StreamReader(requestBody).ReadToEndAsync();

                var webhookOrder = JsonConvert.DeserializeObject<WebhookOrder>(body);
                _logger.LogWarning("Pedido Recebido: tenantId: {0} - orderId: {1} - orderPaid? {2}  - Json: {3}", tenant?.Id, webhookOrder?.id, webhookOrder?.financial_status, body);

                if (tenant == null)
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);

                if (tenant.Status == false || tenant.OrderIntegrationStatus == false)
                    result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);

                if (shopifyApp == null)
                    result.AddError("Dados inválidos", "Shopify app webhook não encontrado.", GetType().FullName);

                if (!_webhookService.IsAuthenticShopifyWebhook(hmacHeader, body, shopifyApp.ShopifySecret))
                    result.AddError("Dados inválidos", $"Hash inválido {hmacHeader}.", GetType().FullName);

                if (webhookType == null)
                    result.AddError("Dados inválidos", "Tipo de webhook inválido.", GetType().FullName);

                if (result.IsFailure)
                {
                    var Error = JsonConvert.SerializeObject(result.Errors);
                    _logger.LogError($"WebhookController:93 | tenant {id} - type {webhookType} - {result} | {Error} ");
                    return BadRequest(result);
                }

                await _webhookService.ReadShopifyWebhook(body, webhookType.Value, tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Webhook tenant {id} - type {webhookType} - Error {ex.Message} body {body}");
                return BadRequest();
            }

            return Ok(result);
        }

        [HttpPost("Omie/{id}")]
        public async Task<IActionResult> PostOmie(long id, [FromBody] OmieWebhook omieWebhook)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                var tenant = await _tenantRepository.GetById(id);

                if (tenant == null)
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                else if (!tenant.Status)
                    result.AddError("Dados inválidos", "Integração desligada para o tenant.", GetType().FullName);
                else if (tenant.Type != TenantType.Omie)
                    result.AddError("Dados inválidos", "tenant não é Omie.", GetType().FullName);
                else if (omieWebhook.ping != "omie")
                {
                    var webhookType = _webhookService.GetOmieWebhookType(omieWebhook.topic);

                    if (webhookType == null)
                        result.AddError("Dados inválidos", "Topic inválido.", GetType().FullName);
                    if (omieWebhook.appKey != tenant.OmieData.AppKey)
                        result.AddError("Dados inválidos", "AppKey inválida.", GetType().FullName);
                    else if (omieWebhook.@event == null)
                        result.AddError("Dados inválidos", "Event vazio.", GetType().FullName);
                    else if (webhookType == APIClient.Omie.Enum.Webhook.WebhookType.Order
                        && !tenant.OrderIntegrationStatus)
                        result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    else if (new List<APIClient.Omie.Enum.Webhook.WebhookType> {
                             APIClient.Omie.Enum.Webhook.WebhookType.ProductCreate,
                             APIClient.Omie.Enum.Webhook.WebhookType.ProductEdit,
                             APIClient.Omie.Enum.Webhook.WebhookType.ProductRemove,
                             APIClient.Omie.Enum.Webhook.WebhookType.Stock }.Contains(webhookType.Value)
                        && !tenant.ProductIntegrationStatus)
                        result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                    else
                    {
                        await _webhookService.ReadOmieWebhook(omieWebhook, webhookType.Value, tenant);
                    }
                }

                if (result.IsFailure)
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on WebhookController PostOmie - {id} - {System.Text.Json.JsonSerializer.Serialize(omieWebhook)}");
                Log.Error(ex, $"Error on WebhookController PostOmie - {id} - {System.Text.Json.JsonSerializer.Serialize(omieWebhook)}");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on WebhookController PostOmie", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("Cache/{tenantId}/{orderId}")]
        public IActionResult GetCache(long tenantId, long orderId)
        {
            var keyCache = new TenantOrderCacheDto()
            {
                TenantId = tenantId,
                ShopifyOrderId = orderId,
            }.GetKey();

            var cache = _cache.GetString(keyCache);

            if (string.IsNullOrEmpty(cache))
                return Ok($"Sem dados para a OrderId: {orderId}");

            return Ok(JsonConvert.DeserializeObject<TenantOrderCacheDto>(cache));
        }

        [HttpPost("Cache")]
        public IActionResult SetCache()
        {
            var cache = new TenantOrderCacheDto()
            {
                TenantId = 71,
                ShopifyOrderId = 3333333333547,
                LastMessageDateUtc = DateTime.UtcNow,
            };

            _cache.SetString(cache.GetKey(), JsonConvert.SerializeObject(cache));

            return Ok();
        }

        private void LogToGuessSomeInformationAboutAuthenticity()
        {
            LogInformation("Headers: ", JsonConvert.SerializeObject(GetAllHeaders()));
            LogInformation("QueryString: ", Request.QueryString.Value);
            LogInformation("RemoteIpAddress: ", Request.HttpContext?.Connection?.RemoteIpAddress?.ToString());
            LogBody();
        }

        private void LogBody()
        {
            Request.Body.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                var body = reader.ReadToEndAsync().Result;

                LogInformation("Body: ", body);

            };
        }

        private void LogInformation(string message, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            content = content.Replace("{", "{{")
                       .Replace("}", "}}");

            _logger.LogInformation("{0}: {1}", message, content);
        }

        private Dictionary<string, string> GetAllHeaders()
        {
            var requestHeaders =
               new Dictionary<string, string>();
            foreach (var header in Request.Headers)
            {
                requestHeaders.Add(header.Key, header.Value);
            }
            return requestHeaders;
        }


    }
}
