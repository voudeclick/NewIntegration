using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Omie.Models.Request.CategoriaCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.CategoriaCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.CenarioImposto;
using VDC.Integration.APIClient.Omie.Models.Request.CenarioImposto.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.EtapasFaturamento;
using VDC.Integration.APIClient.Omie.Models.Request.EtapasFaturamento.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.LocalEstoque;
using VDC.Integration.APIClient.Omie.Models.Request.LocalEstoque.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result;
using VDC.Integration.APIClient.Omie.Models.Result.CategoriasCadastro;
using VDC.Integration.APIClient.Omie.Models.Result.CenarioImposto;
using VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro;
using VDC.Integration.APIClient.Omie.Models.Result.ContaCorrenteCadastro;
using VDC.Integration.APIClient.Omie.Models.Result.EtapasFaturamento;
using VDC.Integration.APIClient.Omie.Models.Result.LocalEstoque;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Results;
using VDC.Integration.EntityFramework.Repositories;

namespace VDC.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrador,Suporte,Viewer,Lojista")]
    public class OmieController : BaseController<OmieController>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TenantRepository _tenantRepository;
        private readonly LogsAbandonMessageRepository _logsAbandonMessageRepository;

        public OmieController(ILogger<OmieController> logger,
                                IHttpClientFactory httpClientFactory,
                                TenantRepository tenantRepository,
                                LogsAbandonMessageRepository logsAbandonMessageRepository)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _tenantRepository = tenantRepository;
            _logsAbandonMessageRepository = logsAbandonMessageRepository;
        }

        [HttpGet("Teste")]
        public IActionResult Teste()
        {
            var omieErro = new OmieError()
            {
                faultstring = "ERROR: Esta requisi\u00e7\u00e3o j\u00e1 foi processada ou est\u00e1 sendo processada e voc\u00ea pode tentar novamente \u00e0s"
            };

            var log = _logsAbandonMessageRepository.GetByLogIdAsync(Guid.Parse("513A5D85-10CB-446D-BDF5-1D0AED2C4B13")).Result;

            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMessage>(log.Error).Error.Message;

            var index = message.IndexOf('{');

            var error = Newtonsoft.Json.JsonConvert.DeserializeObject<OmieError>(message.Substring(index));

            return Ok(error);
        }

        [HttpGet("{id}/GetAllLocalEstoque")]
        public async Task<IActionResult> GetAllLocalEstoque(long id)
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

                if (entity.Type == TenantType.Omie)
                {
                    var page = 0;
                    List<LocalEncontradoResult> values = new List<LocalEncontradoResult>();
                    ListarLocaisEstoqueOmieRequestOutput response = null;
                    do
                    {
                        page++;
                        var request = new ListarLocaisEstoqueOmieRequest(new ListarLocaisEstoqueOmieRequestInput { nPagina = page });
                        var client = request.CreateClient(_httpClientFactory, entity.OmieData.AppKey, entity.OmieData.AppSecret, null);
                        response = await client.Post(request);
                        values.AddRange(response.locaisEncontrados);
                    } while (response.nTotPaginas > page);

                    result.Value = values;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on OmieController GetAllLocalEstoque");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on OmieController GetAllLocalEstoque", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }


        [HttpGet("{id}/GetAllCategorias")]
        public async Task<IActionResult> GetAllCategorias(long id)
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

                if (entity.Type == TenantType.Omie)
                {
                    var page = 0;
                    List<CategoriaCadastroResult> values = new List<CategoriaCadastroResult>();
                    ListarCategoriasOmieRequestOutput response = null;
                    do
                    {
                        page++;
                        var request = new ListarCategoriasOmieRequest(new ListarCategoriasOmieRequestInput { pagina = page });
                        var client = request.CreateClient(_httpClientFactory, entity.OmieData.AppKey, entity.OmieData.AppSecret, null);
                        response = await client.Post(request);
                        values.AddRange(response.categoria_cadastro);
                    } while (response.total_de_paginas > page);

                    result.Value = values.Where(x => x.conta_inativa != "S" && x.nao_exibir != "S");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on OmieController GetAllCategorias");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on OmieController GetAllCategorias", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/GetAllContaCorrente")]
        public async Task<IActionResult> GetAllContaCorrente(long id)
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

                if (entity.Type == TenantType.Omie)
                {
                    var page = 0;
                    List<ContaCorrenteResult> values = new List<ContaCorrenteResult>();
                    ListarResumoContasCorrentesOmieRequestOutput response = null;
                    do
                    {
                        page++;
                        var request = new ListarResumoContasCorrentesOmieRequest(new ListarResumoContasCorrentesOmieRequestInput { pagina = page });
                        var client = request.CreateClient(_httpClientFactory, entity.OmieData.AppKey, entity.OmieData.AppSecret, null);
                        response = await client.Post(request);
                        values.AddRange(response.conta_corrente_lista);
                    } while (response.total_de_paginas > page);

                    result.Value = values;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on OmieController GetAllContaCorrente");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on OmieController GetAllContaCorrente", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }


        [HttpGet("{id}/GetAllEtapasFaturamento")]
        public async Task<IActionResult> GetAllEtapasFaturamento(long id)
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

                if (entity.Type == TenantType.Omie)
                {
                    var page = 0;
                    List<EtapasFaturamentoResult> values = new List<EtapasFaturamentoResult>();
                    ListarEtapasFaturamentoOmieRequestOutput response = null;
                    do
                    {
                        page++;
                        var request = new ListarEtapasFaturamentoOmieRequest(new ListarEtapasFaturamentoOmieRequestInput { pagina = page });
                        var client = request.CreateClient(_httpClientFactory, entity.OmieData.AppKey, entity.OmieData.AppSecret, null);
                        response = await client.Post(request);
                        values.AddRange(response.cadastros);
                    } while (response.total_de_paginas > page);

                    result.Value = values.Where(c => c.cCodOperacao == "11").SelectMany(c => c.etapas.Where(c => new List<string> { "00", "10", "60", "70", "80" }.Contains(c.cCodigo) == false));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on OmieController GetAllEtapasFaturamento");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on OmieController GetAllEtapasFaturamento", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/GetAllCenarioImposto")]
        public async Task<IActionResult> GetAllCenarioImposto(long id)
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

                if (entity.Type == TenantType.Omie)
                {
                    var page = 0;
                    List<CenarioEncontradoResult> values = new List<CenarioEncontradoResult>();
                    ListarCenariosOmieRequestOutput response = null;
                    do
                    {
                        page++;
                        var request = new ListarCenariosOmieRequest(new ListarCenariosOmieRequestInput { nPagina = page });
                        var client = request.CreateClient(_httpClientFactory, entity.OmieData.AppKey, entity.OmieData.AppSecret, null);
                        response = await client.Post(request);
                        values.AddRange(response.cenariosEncontrados);
                    } while (response.nTotPaginas > page);

                    result.Value = values;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on OmieController GetAllCenarioImposto");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on OmieController GetAllCenarioImposto", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/GetAllTransportadora")]
        public async Task<IActionResult> GetAllTransportadora(long id)
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

                if (entity.Type == TenantType.Omie)
                {
                    var page = 0;
                    List<ClienteCadastroResumoResult> values = new List<ClienteCadastroResumoResult>();
                    ListarClientesResumidoOmieRequestOutput response = null;
                    do
                    {
                        page++;
                        var request = new ListarClientesResumidoOmieRequest(new ListarClientesResumidoOmieRequestInput
                        {
                            pagina = page,
                            clientesFiltro = new ClientesFiltro { tags = new List<Tag> { new Tag { tag = "Transportadora" } } }
                        });
                        var client = request.CreateClient(_httpClientFactory, entity.OmieData.AppKey, entity.OmieData.AppSecret, null);
                        response = await client.Post(request);
                        values.AddRange(response.clientes_cadastro_resumido);
                    } while (response.total_de_paginas > page);

                    result.Value = values;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on OmieController GetAllTransportadora");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.AddError("Error on OmieController GetAllTransportadora", ex.Message, GetType().FullName);
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }
    }
}
