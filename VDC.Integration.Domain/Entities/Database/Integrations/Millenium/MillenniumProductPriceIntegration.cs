using System;
using VDC.Integration.Domain.Enums;

namespace VDC.Integration.Domain.Entities.Database.Integrations.Millenium
{
    public class MillenniumProductPriceIntegration
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public string ProductId { get; set; }
        public string ProductSku { get; set; }
        public string Payload { get; set; }
        public IntegrationStatus Status { get; set; }
        public DateTime IntegrationDate { get; set; }
        public Guid? MillenniumNewPriceProcessId { get; set; }
    }
}
