using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class WebhookQuery : BaseQuery<WebhookQueryOutput>
    {
        public override string GetQuery()
        {
            return $@"
                query webhookQuery {{
                  webhookSubscriptions(first: 100) {{
                    edges {{
                      node {{
                        id,
                        topic,
                        endpoint {{
                                __typename... on WebhookHttpEndpoint  {{
                                  callbackUrl
                                }}
                        }}
                      }}
                    }}
                  }}
                }}
            ";
        }
    }

    public class WebhookQueryOutput : BaseQueryOutput
    {
        public Connection<WebhookResult> webhookSubscriptions { get; set; }

        public class WebhookResult
        {
            public string id { get; set; }
            public string topic { get; set; }
            public Endpoint endpoint { get; set; }

        }
        public class Endpoint
        {
            public string __typename { get; set; }
            public string callbackUrl { get; set; }
        }
    }
}
