using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.Bling;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrador,Suporte,Viewer,Lojista")]
    public class BlingController : BaseController<BlingController>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TenantRepository _tenantRepository;

        public BlingController(ILogger<BlingController> logger,
                                IHttpClientFactory httpClientFactory,
                                TenantRepository tenantRepository)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _tenantRepository = tenantRepository;
        }

        [HttpGet("{id}/GetAllWarehouses")]
        public async Task<IActionResult> GetAllWarehouses(long id)
        {
            if(!CanAcessTenantInformation(id))
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

                if (entity.Type == TenantType.Bling)
                {
                    var client = new BlingApiClient(_httpClientFactory, entity.BlingData.ApiBaseUrl, entity.BlingData.APIKey, null);
                    
                    var response = await client.Get<dynamic>("Api/v2/depositos/json/");

                    result.Value = response;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on BlingController GetAllWarehouses");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on BlingController GetAllWarehouses", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/GetAllCategories")]
        public async Task<IActionResult> GetAllCategories(long id)
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

                if (entity.Type == TenantType.Bling)
                {
                    var client = new BlingApiClient(_httpClientFactory, entity.BlingData.ApiBaseUrl, entity.BlingData.APIKey, null);

                    var response = await client.Get<dynamic>("Api/v2/categorias/json/");

                    result.Value = response;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on BlingController GetAllCategories");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on BlingController GetAllCategories", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/GetAllSituacoes")]
        public async Task<IActionResult> GetAllSituacoes(long id)
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

                if (entity.Type == TenantType.Bling)
                {
                    var client = new BlingApiClient(_httpClientFactory, entity.BlingData.ApiBaseUrl, entity.BlingData.APIKey, null);

                    var response = await client.Get<dynamic>("Api/v2/situacao/Vendas/json/");

                    result.Value = response;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on BlingController GetAllSituacoes");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on BlingController GetAllSituacoes", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

    }
}
