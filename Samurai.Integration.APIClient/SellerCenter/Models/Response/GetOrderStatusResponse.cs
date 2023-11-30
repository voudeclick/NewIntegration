using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class GetOrderStatusResponse
    {
        public List<StatusItem> Value { get; set; }

        public class StatusItem
        {
            public Guid Id { get; set; }
            public Guid TenantId { get; set; }
            public string Name { get; set; }
            public StatusLevel Level { get; set; }
            public SystemStatus SystemStatus { get; set; }


            public enum StatusLevel
            {
                [Display(Name = "Pedido")]
                Order,
                [Display(Name = "Pedido do Vendedor")]
                OrderSeller,
                [Display(Name = "Item do Pedido")]
                Item
            }
        }
        public StatusItem GetStatusByLevelAndSystemStatus(StatusItem.StatusLevel level, SystemStatus systemStatus) => Value.Where(x => x.Level == level && x.SystemStatus == systemStatus).FirstOrDefault();
    }
}
