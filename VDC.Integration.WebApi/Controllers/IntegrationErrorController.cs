using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VDC.Integration.Application.Services;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.WebApi.ViewModels;

namespace VDC.Integration.WebApi.Controllers
{
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class IntegrationErrorController : BaseController<IntegrationErrorController>
    {
        private readonly IntegrationErrorService _integrationErrorService;

        public IntegrationErrorController(ILogger<IntegrationErrorController> logger,
            IntegrationErrorService integrationErrorService) : base(logger)
        {
            _integrationErrorService = integrationErrorService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _integrationErrorService.GetAllAsync();

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

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string tag)
        {
            try
            {
                var result = await _integrationErrorService.GetAsync(tag);

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
        public async Task<IActionResult> Post([FromBody] IntegrationErrorViewModel viewModel)
        {
            var result = viewModel.IsValid();

            if (result.IsFailure)
            {
                return StatusCode((int)result.StatusCode, result.ToResult(viewModel));
            }

            try
            {
                var integrationError = Create(viewModel);

                result = await _integrationErrorService.AddAsync(integrationError);

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
        public async Task<IActionResult> Put([FromBody] IntegrationErrorViewModel viewModel)
        {

            var result = viewModel.IsValid();

            if (result.IsFailure)
            {
                return StatusCode((int)result.StatusCode, result.ToResult(viewModel));
            }

            try
            {
                var integrationError = Create(viewModel);

                var resultUpdate = await _integrationErrorService.UpdateAsync(viewModel.Tag, integrationError);

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
        public async Task<IActionResult> Delete([FromQuery] string tag)
        {
            try
            {

                var resultDeleteValue = await _integrationErrorService.DeleteAsync(tag);

                if (resultDeleteValue.IsFailure)
                {
                    return BadRequest(resultDeleteValue);
                }

                return Ok(resultDeleteValue);

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static IntegrationError Create(IntegrationErrorViewModel viewModel)
        {
            return new IntegrationError
            {
                Tag = viewModel.Tag,
                MessagePattern = viewModel.MessagePattern,
                Description = viewModel.Description,
                Message = viewModel.Message,
                SourceId = viewModel.SourceId,
            };
        }

    }
}
