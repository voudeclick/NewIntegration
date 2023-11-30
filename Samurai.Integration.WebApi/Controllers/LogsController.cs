using Canducci.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Infrastructure.Logs;
using Samurai.Integration.Domain.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Controllers
{
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class LogsController : BaseController<LogsController>
    {
        private readonly ILogServices _logServices;
        private const int pageSize = 8;

        public LogsController(ILogger<LogsController> logger, ILogServices logServices) : base(logger)
        {
            _logServices = logServices;
        }


        [HttpGet("GetList")]
        public async Task<ActionResult> GetListByTenantIdAsync([FromQuery] LogsFilterViewModel logs)
        {
            try
            {
                if (logs.Page < 1) logs.Page = 1;

                var logsDto = new LogsDTO(logs);
                var result = await _logServices.GetListByTenantIdAsync(logsDto);

                return Ok(await result.Select(s =>
                new { s.Id, s.TenantId, s.LogId, s.WebJob, s.Type, CreationDate = s.CreationDate.ToString("dd/MM/yy HH:mm:ss")}).ToPaginatedRestAsync(logs.Page, pageSize));
            } 
            catch (System.Exception ex)
            {
                return BadRequest(InternalServerError(ex));
            }
        }

        [HttpGet("GetByFilter")]
        public async Task<ActionResult> GetListByTenantIdAndFilterAsync([FromQuery] LogsFilterViewModel logs)
        {
            try
            {
                if (logs.Page < 1) logs.Page = 1;

                var logsDto = new LogsDTO(logs);
                var result = await _logServices.GetListByTenantIdAndFilterAsync(logsDto);                

                return Ok(await result.Select(s => new LogsAbandonMessageViewModel(s)).ToList().ToPaginatedRestAsync(logs.Page, pageSize));
            }
            catch (System.Exception ex)
            {
                return BadRequest(InternalServerError(ex));
            }
        }

        [HttpGet("GetLog")]
        public async Task<ActionResult> GetLogByLogIdAsync([FromQuery] LogsFilterViewModel logs)
        {
            try
            {   
                var logsDto = new LogsDTO(logs);
                var result = await _logServices.GetLogByLogIdAsync(logsDto);

                return Ok(result.Error);
            }
            catch (System.Exception ex)
            {
                return BadRequest(InternalServerError(ex));
            }
        }

        [HttpGet("GetPayload")]
        public async Task<ActionResult> GetPayloadByLogIdAsync([FromQuery] LogsFilterViewModel logs)
        {
            try
            {
                var logsDto = new LogsDTO(logs);
                var result = await _logServices.GetLogByLogIdAsync(logsDto);

                return Ok(new { Payload = result.Payload });
            }
            catch (System.Exception ex)
            {
                return BadRequest(InternalServerError(ex));
            }
        }
    }
}
