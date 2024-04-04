using System;
using VDC.Integration.Domain.Enums;

namespace VDC.Integration.Domain.Entities.Database.Integrations.Millenium
{
    public class MillenniumProductIntegration
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public string ProductId { get; set; }
        public string Payload { get; set; }
        public IntegrationStatus Status { get; set; }
        public DateTime IntegrationDate { get; set; }
        public Guid? MillenniumNewProductProcessId { get; set; }
    }
}
