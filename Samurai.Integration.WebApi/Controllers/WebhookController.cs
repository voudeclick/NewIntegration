
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.Bling.Models.Webhook;
using Samurai.Integration.APIClient.Nexaas.Models.Enum;
using Samurai.Integration.APIClient.Nexaas.Models.Webhook;
using Samurai.Integration.APIClient.Omie.Models.Webhook;
using Samurai.Integration.APIClient.Pier8.Models.Webhook;
using Samurai.Integration.APIClient.PluggTo.Models.Webhooks;
using Samurai.Integration.APIClient.SellerCenter.Models.Response;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.WebApi.Filters;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using OmieEnum = Samurai.Integration.APIClient.Omie.Enum.Webhook;
using WebhookOrder = Samurai.Integration.APIClient.Shopify.Models.Webhooks.WebhookOrder;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders;
using Samurai.Integration.APIClient.Shopify.Enum.Webhook;

namespace Samurai.Integration.WebApi.Controllers
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
                _logger.LogInformation("Headers: {0}",JsonConvert.SerializeObject(Request.Headers));
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

        [HttpPost("Nexaas/{id}")]
        public async Task<IActionResult> PostNexaas(long id, [FromBody] NexaasWebhook nexaasWebhook)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                LogToGuessSomeInformationAboutAuthenticity();

                var tenant = await _tenantRepository.GetById(id);

                if (tenant == null)
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                else if (!tenant.Status)
                    result.AddError("Dados inválidos", "Integração desligada para o tenant.", GetType().FullName);
                else if (tenant.Type != TenantType.Nexaas)
                    result.AddError("Dados inválidos", "tenant não é Nexaas.", GetType().FullName);
                else if (!nexaasWebhook.test)
                {
                    if (nexaasWebhook.object_id == null || nexaasWebhook.object_id <= 0)
                        result.AddError("Dados inválidos", "Id inválido.", GetType().FullName);
                    else if (nexaasWebhook.object_type == NexaasWebhookType.Order && !tenant.OrderIntegrationStatus)
                        result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    else if (nexaasWebhook.object_type != NexaasWebhookType.Order && !tenant.ProductIntegrationStatus)
                        result.AddError("Dados inválidos", "Integração de produtos desligada para o tenant.", GetType().FullName);
                }

                if (result.IsFailure)
                    return BadRequest(result);

                if (!nexaasWebhook.test)
                    await _webhookService.ReadNexaasWebhook(nexaasWebhook, tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on WebhookController PostNexaas - {id} - {System.Text.Json.JsonSerializer.Serialize(nexaasWebhook)}");
                Log.Error(ex, $"Error on WebhookController PostNexaas - {id} - {System.Text.Json.JsonSerializer.Serialize(nexaasWebhook)}");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on WebhookController PostNexaas", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
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
                    else if (webhookType == OmieEnum.WebhookType.Order
                        && !tenant.OrderIntegrationStatus)
                        result.AddError("Dados inválidos", "Integração de pedidos desligada para o tenant.", GetType().FullName);
                    else if (new List<OmieEnum.WebhookType> {
                             OmieEnum.WebhookType.ProductCreate,
                             OmieEnum.WebhookType.ProductEdit,
                             OmieEnum.WebhookType.ProductRemove,
                             OmieEnum.WebhookType.Stock }.Contains(webhookType.Value)
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

        [HttpPost("Pier8/{id}")]
        [TypeFilter(typeof(ClientIpCheckActionFilter), Arguments = new object[] { "152.67.42.203" })]
        public async Task<IActionResult> PostPier8(long id, [FromForm] Pier8Webhook pier8Webhook)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                LogInformation("RemoteIpAddress: ", Request.HttpContext?.Connection?.RemoteIpAddress?.ToString());
                var tenant = await _tenantRepository.GetById(id);

                if (tenant == null)
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                else if (!tenant.Status)
                    result.AddError("Dados inválidos", "Integração desligada para o tenant.", GetType().FullName);
                else if (!tenant.EnablePier8Integration)
                    result.AddError("Dados inválidos", "Integração pier8 desligada para o tenant.", GetType().FullName);


                await _webhookService.ReadPier8Webhook(pier8Webhook, tenant);

                if (result.IsFailure)
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on WebhookController Pier8 - {id} - {System.Text.Json.JsonSerializer.Serialize(pier8Webhook)}");
                Log.Error(ex, $"Error on WebhookController Pier8 - {id} - {System.Text.Json.JsonSerializer.Serialize(pier8Webhook)}");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on WebhookController Pier8", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("Bling/StockUpdate/{id}")]
        public async Task<IActionResult> PostStockUpdateBling(long id)
        {
            var blingStockUpdateModel = new BlingStockUpdateModel();
            var result = new Result() { StatusCode = HttpStatusCode.OK };

            try
            {
                Request.Form.TryGetValue("data", out StringValues data);

                LogToGuessSomeInformationAboutAuthenticity();

                if (string.IsNullOrEmpty(data.ToString()))
                {
                    result.AddError("Dados inválidos", "Form data inválido.", GetType().FullName);
                    return Ok();
                }

                blingStockUpdateModel = JsonConvert.DeserializeObject<BlingStockUpdateModel>(data.ToString());

                var tenant = await _tenantRepository.GetById(id);

                if (tenant == null)
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                else if (!tenant.Status)
                    result.AddError("Dados inválidos", "Integração desligada para o tenant.", GetType().FullName);
                else if (tenant.Type != TenantType.Bling)
                    result.AddError("Dados inválidos", "tenant não é Bling.", GetType().FullName);
                else
                    await _webhookService.ReadStockUpdateBlingWebhook(blingStockUpdateModel, tenant);

                if (result.IsFailure)
                {
                    _logger.LogError($"Error on WebhookController PostStockUpdateBling - {id} - {JsonConvert.SerializeObject(result)}");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                result.AddError("Error on WebhookController PostStockUpdateBling", ex.Message, GetType().FullName);
                _logger.LogCritical(ex, $"Error on WebhookController PostStockUpdateBling - {id} - Body: {JsonConvert.SerializeObject(blingStockUpdateModel)} - Result: {JsonConvert.SerializeObject(result)}");

                return Ok();
            }

            return Ok();
        }

        [HttpPost("Bling/OrderUpdate/{id}")]
        public async Task<IActionResult> PostOrderUpdateBling(long id)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };

            BlingOrderUpdateModel blingOrderUpdateModel = null;

            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = reader.ReadToEndAsync().Result;
                    blingOrderUpdateModel = JsonConvert.DeserializeObject<BlingOrderUpdateModel>(body.Replace("data=", string.Empty));
                };

                LogToGuessSomeInformationAboutAuthenticity();

                if (blingOrderUpdateModel == null)
                {
                    _logger.LogError($"Error on WebhookController PostOrderUpdateBling - {id} blingOrderUpdateModel is invalid {JsonConvert.SerializeObject(blingOrderUpdateModel)}");
                    return Ok();
                }

                var order = blingOrderUpdateModel.retorno?.pedidos?.FirstOrDefault()?.pedido;

                if (order != null && string.IsNullOrWhiteSpace(order.numeroPedidoLoja))
                    return Ok();

                var tenant = await _tenantRepository.GetById(id);

                if (tenant == null)
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                else if (!tenant.Status)
                    result.AddError("Dados inválidos", "Integração desligada para o tenant.", GetType().FullName);
                else if (tenant.Type != TenantType.Bling)
                    result.AddError("Dados inválidos", "tenant não é Bling.", GetType().FullName);
                else
                    await _webhookService.ReadOrderUpdateBlingWebhook(blingOrderUpdateModel, tenant);

                if (result.IsFailure)
                {
                    _logger.LogError($"Error on WebhookController PostOrderUpdateBling - {id} - {JsonConvert.SerializeObject(result)}");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                result.AddError("Error on WebhookController PostOrderUpdateBling", ex.Message, GetType().FullName);
                _logger.LogCritical(ex, $"Error on WebhookController PostOrderUpdateBling - {id} - Body: {JsonConvert.SerializeObject(blingOrderUpdateModel ?? new BlingOrderUpdateModel())} - Result: {JsonConvert.SerializeObject(result)}");

                return Ok();
            }

            return Ok();
        }

        [HttpPost("PluggTo/{id}")]
        [TypeFilter(typeof(ClientIpCheckActionFilter), Arguments = new object[] { "34.23.253.209" })]
        public async Task<IActionResult> PostPluggTo(long id, [FromBody] PluggToWebhook pluggToWebhook)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };

            _logger.LogWarning("id: " + id + " pluggToWebhook: " + JsonConvert.SerializeObject(pluggToWebhook));
            try
            {
                LogInformation("RemoteIpAddress: ", Request.HttpContext?.Connection?.RemoteIpAddress?.ToString());
                
                if (pluggToWebhook == null)
                    throw new Exception("pluggToWebhook is null");

                var tenant = await _tenantRepository.GetById(id);

                if (tenant == null)
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                else if (!tenant.Status)
                    result.AddError("Dados inválidos", "Integração desligada para o tenant.", GetType().FullName);
                else if (tenant.Type != TenantType.PluggTo)
                    result.AddError("Dados inválidos", "Tenant não é PluggTo.", GetType().FullName);
                else
                    await _webhookService.ReadPluggToWebhook(pluggToWebhook, tenant);

                if (result.IsFailure)
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on WebhookController PostPluggTo - {id} - {pluggToWebhook}");
                Log.Error(ex, $"Error on WebhookController PostPluggTo - {id} - {pluggToWebhook}");

                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on WebhookController PostPluggTo", ex.Message, GetType().FullName);

                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("SellerCenterOrder")]
        public async Task<IActionResult> PostSellerCenterOrder(SellerCenterOrderViewModel orderWrapper)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                var order = orderWrapper.GetOrder();

                foreach (var seller in order.OrderSellers)
                {
                    var tenant = await _tenantRepository.GetTetantBySellerId(seller.SellerId.ToString());
                    if (tenant is null)
                        continue;

                    await _webhookService.ReadOrderCreateSellerCenterWebhook(tenant, order.Id.ToString());
                }

                return StatusCode((int)HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                result.AddError("Erro", $"{ex.Message}", GetType().FullName);
                result.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
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
