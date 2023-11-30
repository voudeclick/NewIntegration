using Samurai.Integration.Domain.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Infrastructure.Logs
{
    public interface ILogServices
    {
        Task<List<LogsAbandonMessageDto>> GetListByTenantIdAsync(LogsDTO logs);
        Task<List<LogsAbandonMessageDto>> GetListByTenantIdAndFilterAsync(LogsDTO logs);
        Task<LogsAbandonMessageDto> GetLogByLogIdAsync(LogsDTO logs);
    } 
}
