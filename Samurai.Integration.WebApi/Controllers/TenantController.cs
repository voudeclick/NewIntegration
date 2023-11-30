using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.Millennium.Models.Results;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.WebApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
   // [Authorize(Roles = "Administrador,Suporte,Viewer")]

    public class TenantController : BaseController<TenantController>
    {
        private readonly TenantRepository _tenantRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TenantService _tenantService;        

        public TenantController(ILogger<TenantController> logger,
                                TenantRepository tenantRepository,
                                IHttpClientFactory httpClientFactory,
                                TenantService tenantService): base(logger)
        {
            _tenantRepository = tenantRepository;
            _httpClientFactory = httpClientFactory;
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] TenantDatatableRequestViewModel request)
        {
            try
            {
                var search = request.Search["value"];
                var paginated = await _tenantRepository.GetWhere(request.Status,
                                                                 request.ERP,
                                                                 request.Shop,
                                                                 search,
                                                                 (request.Start + request.Length) / request.Length,
                                                                 request.Length);


                var tenants = paginated.Select(x =>
                 {
                     //if (User.IsInRole("Viewer"))
                     //{
                     //    x.HideSensitiveData();
                     //}
                     return new TenantViewModel(x);
                 }).ToList();


                var response = new TenantDatatableResponseViewModel()
                {
                    Draw = request.Draw,
                    RecordsTotal = paginated.TotalItemCount,
                    RecordsFiltered = paginated.TotalItemCount,
                    Data = tenants
                };

                return Ok(response);

            }
            catch (Exception ex)
            {

                var result = new Result<List<TenantViewModel>>() { StatusCode = HttpStatusCode.InternalServerError };

                _logger.LogError(ex, "Error on TenantController GetAll");
                result.AddError("Error on TenantController GetAll", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "Administrador,Suporte,Viewer,Lojista")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(long id)
        {
            if (!CanAcessTenantInformation(id))
            {
                return Unauthorized();
            }

            var result = new Result<TenantViewModel>() { StatusCode = HttpStatusCode.OK };
            try
            {
                var entity = await _tenantRepository.GetById(id);

                if (entity == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                //if (User.IsInRole("Viewer"))
                //{
                //    entity.HideSensitiveData();
                //}

                result.Value = new TenantViewModel(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on TenantController Get");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on TenantController Get", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }


        [HttpPost]
        //[Authorize(Roles = "Administrador,Suporte")]
#warning TODO: Verificar fluxo de autorização no app de integração com a Marcela
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] TenantViewModel tenant)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK, Message = "1" };
            try
            {
                if (tenant == null)
                {
                    result.AddError("Dados inválidos", "tenant vazio.", GetType().FullName);
                    return BadRequest(result);
                }

                result.Merge(tenant.IsValid());

                if (result.IsFailure)
                    return BadRequest(result);

                var entity = new Tenant();
                entity.UpdateFrom(tenant);
                await _tenantService.SaveTenant(entity);
                await _tenantService.CreateQueues(entity);
                await _tenantService.CreateWebhooks(_httpClientFactory, entity);
                await _tenantService.UpdateCarrierService(_httpClientFactory, entity);
                await _tenantService.SendUpdateTenantMessages(entity);

                if (entity.Type == TenantType.Bling)
                {
                    result = new Result<long>() { StatusCode = HttpStatusCode.OK, Value = entity.Id };
                    await _tenantService.ValidateBlingUser(result, entity);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on TenantController Post");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on TenantController Post", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }



#warning TODO: Verificar fluxo de autorização no app de integração com a Marcela
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Put(long id, [FromBody] TenantViewModel tenant)
        {
            var result = new Result() { StatusCode = HttpStatusCode.OK };
            var entity = await _tenantRepository.GetById(id);
            try
            {
                if (entity == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                var oldType = entity.Type;

                result.Merge(tenant.IsValid());

                if (result.IsFailure)
                    return BadRequest(result);

                await _tenantService.DeleteWebhooks(_httpClientFactory, entity);
                await _tenantService.CreateWebhooks(_httpClientFactory, entity);
                await _tenantService.CreateQueues(entity);
                await _tenantService.UpdateCarrierService(_httpClientFactory, entity);
                await _tenantService.SendUpdateTenantMessages(entity, oldType);

                var newTenant = await _tenantRepository.GetById(id);
                tenant.ShopifyData.ShopifyAppJson = JsonConvert.SerializeObject(newTenant.ShopifyData.GetShopifyApps());

                entity.UpdateFrom(tenant);
                await _tenantService.UpdateTenant(entity);

                if (entity.Type == TenantType.Bling)
                    await _tenantService.ValidateBlingUser(result, entity);

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unavailable Shop"))
                {
                    tenant.Status = false;
                    tenant.OrderIntegrationStatus = false;
                    tenant.ProductIntegrationStatus = false;
                    await _tenantService.DeleteQueues(entity);
                    entity.UpdateFrom(tenant);
                    await _tenantService.UpdateTenant(entity);                   
                    return BadRequest("Loja indisponível - Integrações foram desativadas");
                }

                _logger.LogError(ex, "Error on TenantController Put");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on TenantController Put", ex.Message, GetType().FullName);
                result.AddError("stackTrace: ", ex.StackTrace.ToString(), "");
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        // [Authorize(Roles = "Administrador,Suporte,Lojista")]

        [AllowAnonymous]
        public async Task<IActionResult> Delete(long id)
        {
            if (!CanAcessTenantInformation(id))
            {
                return Unauthorized();
            }

            var result = new Result() { StatusCode = HttpStatusCode.OK };
            try
            {
                var entity = await _tenantRepository.GetById(id);

                if (entity == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }
                entity.Status = false;
                await _tenantService.DeleteWebhooks(_httpClientFactory, entity);
                await _tenantService.UpdateCarrierService(_httpClientFactory, entity);
                _tenantRepository.Delete(entity);
                await _tenantRepository.CommitAsync();
                await _tenantService.SendUpdateTenantMessages(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on TenantController Delete");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on TenantController Delete", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/ClearMillenniumTransId/{type}/{dateUpdate}")]
        // [Authorize(Roles = "Administrador,Suporte,Lojista")]
        [AllowAnonymous]
        public async Task<IActionResult> ClearMillenniumTransId(long id, TransIdType type, DateTime dateUpdate, CancellationToken cancellationToken)
        {
            if (!CanAcessTenantInformation(id))
            {
                return Unauthorized();
            }

            var result = new Result() { StatusCode = HttpStatusCode.OK };

            if (id <= 0 || dateUpdate > DateTime.UtcNow)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Dados inválidos", "", GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            try
            {
                var entity = await _tenantRepository.GetById(id);

                if (entity == null)
                {
                    result.AddError("Dados inválidos", "Tenant não encontrado.", GetType().FullName);
                    return BadRequest(result);
                }

                if (entity.Type != TenantType.Millennium)
                {
                    result.AddError("Dados inválidos", "Tenant não é Millenniun.", GetType().FullName);
                    return BadRequest(result);
                }

                var millenniumDataTransId = entity.MillenniumData.GetTransId(type);

                var url = "api/millenium_eco/produtos";

                if (!entity.MillenniumData.ControlProductByUpdateDate && type == TransIdType.ListaVitrine)
                    millenniumDataTransId.Value = await _tenantService.GetTransIdListaVitrine<MillenniumApiListProductsResult>(
                            entity, $"{url}/listavitrine", dateUpdate, type, cancellationToken
                        );


                if (!entity.MillenniumData.ControlPriceByUpdateDate && type == TransIdType.PrecoDeTabela)
                    millenniumDataTransId.Value = await _tenantService.GetTransIdPrecoDeTabela<MillenniumApiListPricesResult>(
                            entity, $"{url}/precodetabela", dateUpdate, type, cancellationToken
                        );

                if (!entity.MillenniumData.ControlStockByUpdateDate && type == TransIdType.SaldoDeEstoque)
                    millenniumDataTransId.Value = await _tenantService.GetTransIdSaldoDeEstoque<MillenniumApiListStocksResult>(
                            entity, $"{url}/saldodeestoque", dateUpdate, type, cancellationToken
                );

                millenniumDataTransId.MillenniumLastUpdateDate = dateUpdate;

                entity.MillenniumData.SetTransId(millenniumDataTransId);
                await _tenantRepository.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on TenantController ClearMillenniumTransId");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on TenantController ClearMillenniumTransId", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("ListWebhooks/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListWebhooks(long id)
        {
            //if (!CanAcessTenantInformation(id))
            //{
            //    return Unauthorized();
            //}

            var result = new Result<WebhookQueryOutput>() { StatusCode = HttpStatusCode.OK };
            var entity = await _tenantRepository.GetById(id);
            result.Value = await _tenantService.QueryWebhooks(_httpClientFactory, entity);
            return Ok(result);
        }

        [HttpGet("GetTenantForLogs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTenantForLogs()
        {
            var result = new Result<List<object>>() { StatusCode = HttpStatusCode.OK };
            try
            {
                return Ok((await _tenantRepository.GetActiveByIntegrationType(IntegrationType.Shopify)).Select(x => new { id = x.Id, storeName = x.StoreName }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on TenantController GetAll");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on TenantController GetAll", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }


        }

        [HttpGet("CreateQueues")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateQueues()
        {
            try
            {
                var tenants = await _tenantRepository.GetAll();
                tenants = tenants.Where(s => s.IntegrationType == IntegrationType.Shopify && s.Id == 16).ToList();

                foreach (var tenant in tenants)
                {
                    try
                    {
                        await _tenantService.CreateQueues(tenant);

                        //foreach (var queue in queues)
                        //{
                        //    await _tenantService.DeleteQueue(tenant, queue, false);
                        //}
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            return Ok();
        }
    }
}
