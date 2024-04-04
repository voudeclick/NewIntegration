using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDC.Integration.Application.Services;
using VDC.Integration.Domain.Consts;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Extensions;
using VDC.Integration.Domain.ValueObjects;
using VDC.Integration.EntityFramework.Repositories;
using VDC.Integration.WebApi.ServiceHangfire;
using VDC.Integration.WebApi.ViewModels;

namespace VDC.Integration.WebApi.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ParamController : BaseController<ParamController>
    {
        private readonly ParamRepository _paramRepository;
        private readonly ParamService _paramService;

        public ParamController(ParamRepository paramRepository, ILogger<ParamController> logger, ParamService paramService) : base(logger)
        {
            _paramRepository = paramRepository;
            _paramService = paramService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string key)
        {
            try
            {
                var result = await _paramService.GetAsync(key);

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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ParamViewModel viewModel)
        {
            var result = viewModel.IsValid();

            if (result.IsFailure)
            {
                return StatusCode((int)result.StatusCode, result.ToResult(viewModel));
            }

            try
            {
                var param = new Param(viewModel.Key);

                viewModel.Values.ForEach(x => param.Add(x.Key, x.Value.GetObject()));

                result = await _paramService.AddAsync(param);

                if (result.IsFailure)
                {
                    return BadRequest(result);
                }

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
                    var resultDeleteValue = await _paramService.DeleteValueAsync(key, keyValue);

                    if (resultDeleteValue.IsFailure)
                    {
                        return BadRequest(resultDeleteValue);
                    }

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
    }
}
