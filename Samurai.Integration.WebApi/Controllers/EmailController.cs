using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Infrastructure.Email;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.WebApi.ServiceHangfire;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Controllers
{
    [Authorize(Roles = "Administrador,Suporte")]
    public class EmailController : BaseController<EmailController>
    {
                
        private readonly IEmailClientSmtp _emailClientSmtp;
        private readonly IntegrationMonitorHangfire _integrationMonitorHangfire;
        public EmailController(ILogger<EmailController> logger,
            IEmailClientSmtp emailClientSmtp,
            IntegrationMonitorHangfire integrationMonitorHangfire) : base(logger)
        {
            
            _emailClientSmtp = emailClientSmtp;
            _integrationMonitorHangfire = integrationMonitorHangfire;
        }


        [HttpPost("Send")]
        public async Task<IActionResult> SendAsync([FromBody]EmailDto emailDto)
        {
            try
            {
                var result = new Result<string>() { StatusCode = HttpStatusCode.OK };

                await _emailClientSmtp.SendAsync(emailDto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost("SendLostedOrdersToSamuraiTeam")]
        public async Task<IActionResult> SendLostedOrdersToSamuraiTeamAsync()
        {
            try
            {
                var result = new Result<string>() { StatusCode = HttpStatusCode.OK };

                await _integrationMonitorHangfire.Initialize();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        
    }
}
