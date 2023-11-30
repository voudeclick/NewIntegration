using Newtonsoft.Json;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using static Samurai.Integration.Domain.Messages.PluggTo.PluggToData;

namespace Samurai.Integration.APIClient.PluggTo.Models.Requests
{
    public class PluggToApiUpdateOrderRequest
    {
        public Order Order { get; set; }
        public static PluggToApiUpdateOrderRequest From(OrderSellerDto orderSeller, PluggToDataOrderStatusMapping statusMapping)
        {
            return new PluggToApiUpdateOrderRequest
            {
                Order = new Order()
                {
                    id = orderSeller?.ClientId,
                    status = statusMapping.PluggToSituacaoNome
                }
            };
        }
    }

    public class Order
    {
        public string id { get; set; }

        public string status { get; set; }
      
    }
}
