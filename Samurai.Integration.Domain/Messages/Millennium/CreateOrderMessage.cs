using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Millennium
{
    public class CreateOrderMessage
    {
        public CreateOrderMessage()
        {

        }

        public CreateOrderMessage(Models.SellerCenter.API.Orders.Order obj)
        {
            Data = obj;
        }
        public Domain.Models.SellerCenter.API.Orders.Order Data { get; set; }
    }
}
