using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Results;
using VDC.Integration.Domain.ValueObjects;
using VDC.Integration.EntityFramework.Repositories;

namespace VDC.Integration.Application.Services
{
    public class ParamService
    {
        private readonly ParamRepository _paramRepository;
        private readonly ILogger<ParamService> _logger;

        public ParamService(ParamRepository paramRepository,
            ILogger<ParamService> logger)
        {
            _paramRepository = paramRepository;
            _logger = logger;
        }

        public async Task<Result<Param>> GetAsync(string key)
        {
            var result = new Result<Param>() { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrEmpty(key))
            {
                result.AddError("Requerido", "Informe a key.", GetType().FullName);
            }

            var param = await _paramRepository.GetByKeyAsync(key);

            if (param == null)
            {
                result.AddError($"Parametro {key} não existe", string.Empty, param.GetType().FullName);
            }

            result.Value = param;

            return result;
        }

        public async Task<Result<Param>> AddAsync(Param param)
        {
            var result = new Result<Param>() { StatusCode = HttpStatusCode.OK };

            var exists = await _paramRepository.ExistsByKeyAsync(param.Key);

            if (exists)
            {
                result.AddError($"Parametro {param.Key} já existe", string.Empty, param.GetType().FullName);
                return result;
            }

            await _paramRepository.AddAsync(param);

            result.Value = param;

            return result;
        }

        public async Task<Result<Param>> UpdateAsync(string key, List<ParamValue> values)
        {
            var result = new Result<Param>() { StatusCode = HttpStatusCode.OK };

            var param = await _paramRepository.GetByKeyAsync(key);

            if (param == null)
            {
                result.AddError($"Parametro {key} não existe", string.Empty, param.GetType().FullName);
                return result;
            }

            values.ForEach(x => param.Update(x.Key, x.Value));

            await _paramRepository.UpdateAsync(param);

            result.Value = param;

            return result;
        }

        public async Task<Result<Param>> DeleteAsync(string key)
        {
            var result = new Result<Param>() { StatusCode = HttpStatusCode.OK };

            var param = await _paramRepository.GetByKeyAsync(key);

            if (param == null)
            {
                result.AddError($"Parametro {key} não existe", string.Empty, param.GetType().FullName);
                return result;
            }

            await _paramRepository.DeleteAsync(param);

            return result;
        }

        public async Task<Result<Param>> DeleteValueAsync(string keyParam, string keyValue)
        {
            var result = new Result<Param>() { StatusCode = HttpStatusCode.OK };

            var param = await _paramRepository.GetByKeyAsync(keyParam);

            if (param == null)
            {
                result.AddError($"Parametro {keyParam} não existe", string.Empty, param.GetType().FullName);
                return result;
            }

            if (!param.ExistsByKeyValue(keyValue))
            {
                result.AddError($"Parametro {keyValue} não existe em {keyParam}", string.Empty, param.GetType().FullName);
                return result;
            }

            param.RemoveByKeyValue(keyValue);

            await _paramRepository.UpdateAsync(param);

            result.Value = param;

            return result;
        }
    }
}
