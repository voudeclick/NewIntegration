using System;

namespace Samurai.Integration.Domain.Messages.Webjob
{
    public class UpdateTenantMessage
    {
        public long TenantId { get; set; }
        public DateTime TimerData { get; set; }
    }
}
