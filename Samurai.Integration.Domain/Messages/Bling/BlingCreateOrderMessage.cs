using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;

using System;

namespace Samurai.Integration.Domain.Messages.Bling
{
    public class BlingCreateOrderMessage
    {
        public BlingCreateOrderMessage()
        {

        }

        public BlingCreateOrderMessage(Models.SellerCenter.API.Orders.Order obj)
        {
            Data = obj;
        }
        public Domain.Models.SellerCenter.API.Orders.Order Data { get; set; }

        public static object From(ProcessOrderMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
