using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.Domain.Dtos;

namespace VDC.Integration.Domain.Infrastructure.Logs
{
    public interface ILogServices
    {
        Task<List<LogsAbandonMessageDto>> GetListByTenantIdAsync(LogsDTO logs);
        Task<List<LogsAbandonMessageDto>> GetListByTenantIdAndFilterAsync(LogsDTO logs);
        Task<LogsAbandonMessageDto> GetLogByLogIdAsync(LogsDTO logs);
    }
}
