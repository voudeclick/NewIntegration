using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using VDC.Integration.Domain.Results;

namespace VDC.Integration.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public abstract class BaseController<ControllerLogger> : ControllerBase
    {
        protected readonly ILogger<ControllerLogger> _logger;


        public BaseController(ILogger<ControllerLogger> logger)
        {
            _logger = logger;
        }

        protected bool CanAcessTenantInformation(long tenantId)
        {
            //if(User.IsInRole("Lojista"))
            //{
            //    return User.Claims.Any(
            //        x => x.Type == ClaimTypeConsts.TenantId &&
            //        x.Value == tenantId.ToString()
            //    );
            //}

            return true;
        }

        protected ObjectResult InternalServerError(Exception ex)
        {
            _logger.LogError(ex, $"Error on {GetType().FullName}");
            var result = new Result() { StatusCode = HttpStatusCode.InternalServerError };
            result.AddError("Ops", "Algo inesperado ocorreu", GetType().FullName);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
