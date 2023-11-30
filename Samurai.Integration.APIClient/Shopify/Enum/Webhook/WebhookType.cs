using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Shopify.Enum.Webhook
{
    public enum WebhookType
    {
        OrdersCreate = 1,
        OrdersUpdate = 2
    }

    public static class WebhookTypeExtension
    {
        public class WebhookTopic
        {
            public string header { get; set; }
            public string value { get; set; }
        }

        public static Dictionary<WebhookType, List<WebhookTopic>> mappings = new Dictionary<WebhookType, List<WebhookTopic>>
        {
            {
                WebhookType.OrdersCreate, new List<WebhookTopic>{
                    new WebhookTopic { header = "orders/create", value = "ORDERS_CREATE" }
                } 
            },
            {
                WebhookType.OrdersUpdate, new List<WebhookTopic>{
                    new WebhookTopic { header = "orders/cancelled", value = "ORDERS_CANCELLED" },
                    new WebhookTopic { header = "orders/fulfilled", value = "ORDERS_FULFILLED" },
                    new WebhookTopic { header = "orders/paid", value = "ORDERS_PAID" },
                    new WebhookTopic { header = "orders/partially_fulfilled", value = "ORDERS_PARTIALLY_FULFILLED" },
                    new WebhookTopic { header = "orders/updated", value = "ORDERS_UPDATED" },
                    new WebhookTopic { header ="orders/edited", value = "ORDERS_EDITED"},
                    new WebhookTopic { header ="refunds/create", value = "REFUNDS_CREATE"},
                }
            }
        };

        public static List<WebhookTopic> GetTopics(this WebhookType type)
        {
            return mappings[type];
        }
    }
}
