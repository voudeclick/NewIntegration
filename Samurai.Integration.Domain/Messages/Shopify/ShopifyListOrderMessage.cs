using System;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyListOrderMessage
    {
        public long ShopifyId { get; set; }
        public int DeliveryCount { get; set; } = 0;

        //O corpo da solitação é passado para impedir que o ServiceBus considere como duplicada a msg caso tenha uma alteração real
        public object Body { get; set; }
    }
}
