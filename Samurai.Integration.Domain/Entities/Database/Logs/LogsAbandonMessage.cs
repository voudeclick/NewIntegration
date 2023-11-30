using System;

namespace Samurai.Integration.Domain.Entities.Database.Logs
{
    public class LogsAbandonMessage
    {
        public Guid Id { get; private set; }
        public Guid LogId { get; private set; }
        public long TenantId { get; private set; }
        public string Method { get; private set; }
        public string Type { get; private set; }
        public string WebJob { get; set; }
        public string Error { get; private set; }
        public string Payload { get; private set; }
        public DateTime CreationDate { get; private set; }

        public LogsAbandonMessage() { }

        public LogsAbandonMessage(Guid logId, string webJob, long tenantId, string method, string type, string payload)
        {
            LogId = logId;
            WebJob = webJob;
            TenantId = tenantId;
            Method = method;
            Type = type;
            Payload = payload;
            CreationDate = DateTime.Now;
        }

        public void AddErrorInfo(object error)
        {
            Error = Newtonsoft.Json.JsonConvert.SerializeObject(error);
        }
    }
}