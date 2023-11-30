using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Entities.Database.Logs;
using Samurai.Integration.Domain.Infrastructure.Logs;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public class LogServices : ILogServices
    {
        private readonly LogsAbandonMessageRepository _logsAbandonMessageRepository;

        public LogServices(LogsAbandonMessageRepository logsAbandonMessageRepository)
        {
            _logsAbandonMessageRepository = logsAbandonMessageRepository;
        }

        public async Task<List<LogsAbandonMessageDto>> GetListByTenantIdAsync(LogsDTO logs)
        {           
            var result = await _logsAbandonMessageRepository.GetByTenantIdAsync(logs.TenantId);
            result = result.OrderByDescending(s => s.CreationDate).ToList();

            return result.Select(s => new LogsAbandonMessageDto(s)).ToList();             
        }

        public async Task<List<LogsAbandonMessageDto>> GetListByTenantIdAndFilterAsync(LogsDTO logs)
        {
            var result = await _logsAbandonMessageRepository.GetByFilterAsync(logs);
            result = result.OrderByDescending(s => s.CreationDate).ToList();

            return result;
        }

        public async Task<LogsAbandonMessageDto> GetLogByLogIdAsync(LogsDTO logs)
        {
            var result = await _logsAbandonMessageRepository.GetByLogIdAsync((Guid)logs.LogId);
            return new LogsAbandonMessageDto(result);                        
        }        
    }
}
