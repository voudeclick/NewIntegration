using System;

namespace Samurai.Integration.Domain.Entities.Database.Integrations.Millenium
{
    public class MillenniumUpdateOrderProcess
    {
		public Guid Id { get; set; }
		public long TenantId { get; set; }
		public long OrderId { get; set; }
        public string Payload { get; set; }
        public Guid? ShopifyListOrderProcessReferenceId { get; set; }
		public DateTime ProcessDate { get; set; }
		public string MillenniumResponse { get; set; }
		public string Exception { get; set; }
	}
}
