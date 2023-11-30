using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Enum
{
    public enum TrayWebhookType
    {
        Order = 1
    }

    public static class WebhookTypeExtension
    {
        public class WebhookTopic
        {
            public string value { get; set; }
        }

        public static Dictionary<TrayWebhookType, List<WebhookTopic>> mappings = new Dictionary<TrayWebhookType, List<WebhookTopic>>
        {
            {
                TrayWebhookType.Order, new List<WebhookTopic>{
                    new WebhookTopic { value = "order_insert" },
                    new WebhookTopic { value = "order_update" },
                    new WebhookTopic { value = "order_delete"},
                }
            }
        };

        public static List<WebhookTopic> GetTopics(this TrayWebhookType type)
        {
            return mappings[type];
        }
    }
}
