using Samurai.Integration.Domain.Enums.Millennium;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class NexaasData : BaseEntity
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public long OrganizationId { get; set; }
        public long SaleChannelId { get; set; }
        public long StockId { get; set; }
        public string OrderPrefix { get; set; }
        public bool IsPickupPointEnabled { get; set; }
        public string ServiceNameTemplate { get; set; }
        public string DeliveryTimeTemplate { get; set; }

        public void HideSensitiveData()
        {
            Token = string.Empty;
        }
    }
}
