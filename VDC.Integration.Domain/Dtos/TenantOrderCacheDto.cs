using System;

namespace VDC.Integration.Domain.Dtos
{
    public class TenantOrderCacheDto
    {
        public long TenantId { get; set; }
        public long ShopifyOrderId { get; set; }
        public DateTime? LastMessageDateUtc { get; set; }


        public string GetKey()
        {
            return $"{TenantId}-{ShopifyOrderId}";
        }
    }
}
