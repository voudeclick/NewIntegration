using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Tray
{
    public class TrayAppReturnMessage
    {
        public string Type { get; set; } //product/productvariation/order/ordertracking
        public bool Success { get; set; }
        public string Message { get; set; }
        public string StoreId { get; set; }
        public ProductIntegration Product { get; set; }
        public OrderIntegration Order { get; set; }

        public class ProductIntegration
        {
            public Guid Id { get; set; }
            public long TrayProductId { get; set; }
            public string Status { get; set; }
            public bool Available { get; set; }

            public List<ProductVariationIntegration> Variations { get; set; }

            public class ProductVariationIntegration
            {
                public Guid Id { get; set; }
                public long TrayProductVariationId { get; set; }
            }
        }

        public class OrderIntegration
        {
            public long TrayOrderId { get; set; }
            public string AliExpressOrderId { get; set; }
            public string Status { get; set; }
            public string TrackingCode { get; set; }
            public string LogisticServiceName { get; set; }
            public string CurrencyCode { get; set; }
            public string AmountPaid { get; set; }
            public List<OrderAliExpressItems> Items { get; set; }

            public class OrderAliExpressItems
            {
                public long ProductId { get; set; }
                public string Price { get; set; }
            }
        }
    }
}
