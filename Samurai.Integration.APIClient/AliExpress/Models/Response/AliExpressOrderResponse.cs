using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.AliExpress.Models.Response
{
    public class AliExpressOrderResponse
    {
        [JsonProperty("aliexpress_trade_ds_order_get_response")]
        public AliExpressOrderResult Order { get; set; }

        public class AliExpressOrderResult
        {
            [JsonProperty("result")]
            public AliExpressOrderResultDetail Result { get; set; }
        }

        public class AliExpressOrderResultDetail
        {
            [JsonProperty("gmt_create")]
            public string OrderDate { get; set; }

            [JsonProperty("order_status")]
            public string OrderStatus { get; set; }

            [JsonProperty("logistics_status")]
            public string LogisticsStatus { get; set; }

            [JsonProperty("order_amount")]
            public OrderAmount Amount { get; set; }

            [JsonProperty("child_order_list")]
            public AliExpressOrderChildList Items { get; set; }

            [JsonProperty("logistics_info_list")]
            public AliExpressOrderLogistic Logistic { get; set; }


        }

        public class AliExpressOrderChildList
        {
            [JsonProperty("aeop_child_order_info")]
            public List<AliExpressOrderChildInfo> Products { get; set; }

            public class AliExpressOrderChildInfo
            {
                [JsonProperty("product_id")]
                public long ProductId { get; set; }

                [JsonProperty("product_name")]
                public string ProductName { get; set; }

                [JsonProperty("product_count")]
                public string ProductCount { get; set; }

                [JsonProperty("product_price")]
                public OrderAmount Price { get; set; }
            }
        }

        public class AliExpressOrderLogistic
        {
            [JsonProperty("aeop_order_logistics_info")]
            public List<AliExpressOrderLogisticInfo> OrderLogistic { get; set; }

            public class AliExpressOrderLogisticInfo
            {
                [JsonProperty("logistics_no")]
                public string TrackingCode { get; set; }

                [JsonProperty("logistics_service")]
                public string ServiceName { get; set; }
            }
        }

        public class OrderAmount
        {
            [JsonProperty("amount")]
            public string Amount { get; set; }

            [JsonProperty("currency_code")]
            public string CurrencyCode { get; set; }
        }
    }
}