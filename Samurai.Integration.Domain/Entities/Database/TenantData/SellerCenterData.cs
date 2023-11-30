using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Enums.SellerCenter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class SellerCenterData : BaseEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TenantId { get; set; }
        public OrderIntegrationStatusEnum OrderIntegrationStatus { get; set; }
        public string SellerId { get; set; }
        public bool SellWithoutStock { get; set; }
        public string SellerWarehouseId { get; set; }
        public virtual List<SellerCenterTransId> TransIds { get; set; }
        public bool DisableUpdateProduct { get; set; }

        public SellerCenterTransId GetTransId(TransIdType type)
        {
            if (TransIds == null || !TransIds.Any(x => x.Type == type))
                return new SellerCenterTransId
                {
                    Type = type,
                    LastProcessedDate = new DateTimeOffset(DateTime.Now)
                };

            return TransIds.FirstOrDefault(x => x.Type == type);
        }
        public void SetTransId(SellerCenterTransId value)
        {
            var currentValue = TransIds.FirstOrDefault(x => x.Type == value.Type);
            if (currentValue == null)
                TransIds.Add(value);
        }

        public void HideSensitiveData()
        {
            Password = string.Empty;
        }
    }
}
