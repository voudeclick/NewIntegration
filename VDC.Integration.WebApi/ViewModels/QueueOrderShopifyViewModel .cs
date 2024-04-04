using System;

namespace VDC.Integration.WebApi.ViewModels
{
    public class QueueOrderShopifyViewModel
    {
        public long TenantId { get; set; }
        public string OrderId { get; set; }
    }

    public class QueueOrderShopifyFilterDateViewModel
    {
        public long TenantId { get; set; }
        public DateTime DateMin { get; set; }
        public DateTime DateMax { get; set; }
    }
}
