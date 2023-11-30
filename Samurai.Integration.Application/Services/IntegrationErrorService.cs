using Microsoft.Extensions.Logging;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public class IntegrationErrorService
    {
        private readonly IntegrationErrorRepository _integrationErrorRepository;
        private readonly ILogger<IntegrationErrorService> _logger;

        public IntegrationErrorService(IntegrationErrorRepository integrationErrorRepository, ILogger<IntegrationErrorService> logger)
        {
            _integrationErrorRepository = integrationErrorRepository;
            _logger = logger;
        }

        public async Task<Result<List<IntegrationError>>> GetAllAsync()
        {
            var result = new Result<List<IntegrationError>>() { StatusCode = HttpStatusCode.OK };

            result.Value = await _integrationErrorRepository.GetAllAsync();

            return result;
        }

        public async Task<Result<IntegrationError>> GetAsync(string tag)
        {
            var result = new Result<IntegrationError>() { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrEmpty(tag))
            {
                result.AddError("Requerido", "Informe a tag.", GetType().FullName);
            }

            var integrationError = await _integrationErrorRepository.GetByTagAsync(tag);

            if (integrationError == null)
            {
                result.AddError($"Erro de integração {tag} não existe", string.Empty, integrationError.GetType().FullName);
            }

            result.Value = integrationError;

            return result;
        }

        public async Task<Result<IntegrationError>> AddAsync(IntegrationError integrationError)
        {
            var result = new Result<IntegrationError>() { StatusCode = HttpStatusCode.OK };

            var exists = await _integrationErrorRepository.ExistsByTagAsync(integrationError.Tag);

            if (exists)
            {
                result.AddError($"Erro de integração {integrationError.Tag} já existe", string.Empty, integrationError.GetType().FullName);
                return result;
            }

            await _integrationErrorRepository.AddAsync(integrationError);

            result.Value = integrationError;

            return result;
        }

        public async Task<Result<IntegrationError>> UpdateAsync(string tag, IntegrationError integrationError)
        {
            var result = new Result<IntegrationError>() { StatusCode = HttpStatusCode.OK };

            var integrationErrorSaved = await _integrationErrorRepository.GetByTagAsync(tag);

            if (integrationErrorSaved == null)
            {
                result.AddError($"Erro de integração {tag} não existe", string.Empty, integrationError.GetType().FullName);
                return result;
            }

            integrationErrorSaved.Update(integrationError);
            
            await _integrationErrorRepository.UpdateAsync(integrationErrorSaved);

            result.Value = integrationErrorSaved;

            return result;
        }

        public async Task<Result<IntegrationError>> DeleteAsync(string tag)
        {
            var result = new Result<IntegrationError>() { StatusCode = HttpStatusCode.OK };

            var integrationError = await _integrationErrorRepository.GetByTagAsync(tag);

            if (integrationError == null)
            {
                result.AddError($"Erro de integração {tag} não existe", string.Empty, integrationError.GetType().FullName);
                return result;
            }

            await _integrationErrorRepository.DeleteAsync(integrationError);

            result.Value = integrationError;

            return result;
        }

        public async Task<string> GenerateTagError(string message, IntegrationErrorSource integrationErrorSource, Func<bool> IsErpException)
        {
            var integrationErrors = await _integrationErrorRepository.GetAllAsync();

            if (IsErpException())
            {
                var erpErrors = integrationErrors.Where(x => x.SourceId == integrationErrorSource);
                var erpError999 = integrationErrors.FirstOrDefault(x => x.Tag == $"Erro-{integrationErrorSource.ToString().ToLower()}-999");

                if (message == null)
                {
                    return erpError999.Tag;
                }

                var erpError = erpErrors.FirstOrDefault(x => message.IsMatchRegex(x.MessagePattern));

                if (erpError != null)
                {
                    return erpError.Tag;
                }

                return erpError999.Tag;
            }

            var errorsSourceOrder = new List<IntegrationErrorSource>() { IntegrationErrorSource.Lojista, IntegrationErrorSource.Integration };

            foreach (var errorSource in errorsSourceOrder)
            {
                var errors = integrationErrors.Where(x => x.SourceId == errorSource).ToList();

                var error = errors.FirstOrDefault(x => message.IsMatchRegex(x.MessagePattern));

                if (error != null)
                {
                    return error.Tag;
                }

            }

            var error999 = integrationErrors.FirstOrDefault(x => x.Tag == "Erro-999");

            return error999.Tag;
        }
    }
}
