using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.Domain.ValueObjects;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.WebApi.ServiceHangfire;
using Samurai.Integration.WebApi.ViewModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ParamController : BaseController<ParamController>
    {
        private readonly ParamRepository _paramRepository;        
        private readonly ParamService _paramService;

        public ParamController(ParamRepository paramRepository,ILogger<ParamController> logger,ParamService paramService) : base(logger)
        {
            _paramRepository = paramRepository;            
            _paramService = paramService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string key)
        {
            try
            {                
                var result = await _paramService.GetAsync(key);

                if(result.IsFailure)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }          

        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ParamViewModel viewModel)
        {
            var result = viewModel.IsValid();

            if (result.IsFailure)
            {
                return StatusCode((int)result.StatusCode, result.ToResult(viewModel));
            }

            try
            {
                var param = new Param(viewModel.Key);

                viewModel.Values.ForEach(x => param.Add(x.Key,x.Value.GetObject()));

                result = await _paramService.AddAsync(param);

                if (result.IsFailure)
                {
                    return BadRequest(result);
                }

                UpdateRecurringJobIfNecessary(param);

                return Ok(result.ToResult(viewModel));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);

            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ParamViewModel viewModel)
        {

            var result = viewModel.IsValid();

            if (result.IsFailure)
            {
                return StatusCode((int)result.StatusCode, result.ToResult(viewModel));
            }

            try
            {
                var values = viewModel.Values.Select(s => new ParamValue()
                {
                    Key = s.Key,
                    Value = s.Value.GetObject(),
                }).ToList();

                var resultUpdate = await _paramService.UpdateAsync(viewModel.Key, values);

                if (resultUpdate.IsFailure)
                {
                    return BadRequest(resultUpdate);
                }

                UpdateRecurringJobIfNecessary(resultUpdate.Value);

                return Ok(resultUpdate);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string key, string keyValue)
        {
            try
            {                               

                if (!string.IsNullOrEmpty(keyValue))
                {
                    var resultDeleteValue = await _paramService.DeleteValueAsync(key,keyValue);

                    if (resultDeleteValue.IsFailure)
                    {
                        return BadRequest(resultDeleteValue);
                    }

                    DeleteRecurringJobIfNecessary(key, keyValue);

                    return Ok(resultDeleteValue);
                }

                var result = await _paramService.DeleteAsync(key);

                if (result.IsFailure)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        private void UpdateRecurringJobIfNecessary(Param param)
        {
            if(param.Key == ParamConsts.IntegrationMonitorHangfire)
            {
                RecurringJob.AddOrUpdate<IntegrationMonitorHangfire>(x => x.Initialize(), 
                    param.GetValueBykey(IntegrationMonitorHangfireConsts.CronExpression).Value.ToString() ?? "0 */2 * * *");
            }
        }

        private void DeleteRecurringJobIfNecessary(string key, string keyValue)
        {
            if (key == ParamConsts.IntegrationMonitorHangfire &&
                keyValue == IntegrationMonitorHangfireConsts.CronExpression)
            {
                RecurringJob.RemoveIfExists($"{nameof(IntegrationMonitorHangfire)}.{ nameof(IntegrationMonitorHangfire.Initialize)}");
            }

        }
    }
}
