using Samurai.Integration.Domain.Models.ViewModels;
using System;

namespace Samurai.Integration.Domain.Dtos
{
    public class LogsDTO
    {
        public long TenantId { get; private set; }
        public string Filter { get; private set; }
        public string Method { get; private set; }
        public string Type { get; private set; }
        public string Webjob { get; private set; }        
        public int Page { get; private set; }
        public Guid? LogId { get; private set; }
        public int PageSize { get; private set; }

        public LogsDTO(LogsFilterViewModel logs)
        {
            TenantId = logs.TenantId;
            Filter = logs.Filter;
            Method = logs.Method;
            Type = logs.Type;
            Webjob = logs.Webjob;          
            Page = logs.Page;

            if(!string.IsNullOrWhiteSpace(logs.LogId))
                LogId = new Guid(logs.LogId);
        }
    }  
}
