using Samurai.Integration.Domain.Dtos;
using System;

namespace Samurai.Integration.Domain.Models.ViewModels
{
    public class LogsAbandonMessageViewModel
    {
        public Guid Id { get;  set; }
        public Guid LogId { get;  set; }
        public long TenantId { get;  set; }
        public string Method { get;  set; }
        public string Type { get;  set; }
        public string WebJob { get; set; }
        public string Error { get;  set; }
        public string Payload { get;  set; }
        public DateTime CreationDate { get;  set; }

        public LogsAbandonMessageViewModel() { }
        public LogsAbandonMessageViewModel(LogsAbandonMessageDto logsDto)
        {
            Id = logsDto.Id;
            LogId = logsDto.LogId;
            TenantId = logsDto.TenantId;
            Method = logsDto.Method;
            Type = logsDto.Type;
            WebJob = logsDto.WebJob;
            Error = logsDto.Error;
            Payload = logsDto.Payload;
            CreationDate = logsDto.CreationDate;
        }
    }
}