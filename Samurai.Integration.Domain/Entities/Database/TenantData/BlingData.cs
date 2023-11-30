using Samurai.Integration.Domain.Enums.Bling;
using System;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class BlingData : BaseEntity
    {
        public string StoreHandle { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public string ApiBaseUrl { get; set; }
        public string APIKey { get; set; }
        public string OrderPrefix { get; set; }
        public string OrderStatusMapping { get; set; }
        public DateTime? LastProductUpdateDate { get; set; }
        public string CategoriaId { get; set; }
        public OrderFieldBlingType OrderField { get; set; }
        public int? OrderStatusId { get; set; }

        public void HideSensitiveData()
        {
            APIKey = string.Empty; 
        }

    }
}
