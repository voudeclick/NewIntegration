using Samurai.Integration.APIClient.Shopify.Models.Request.Inputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class WebhookDeleteMutation : BaseMutation<WebhookDeleteMutationInput, WebhookDeleteMutationOutput>
    {
        public WebhookDeleteMutation(WebhookDeleteMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation webhookSubscriptionDelete($id: ID!) {{
                  webhookSubscriptionDelete(id: $id) {{
                    deletedWebhookSubscriptionId
                    userErrors {{
                      field
                      message
                    }}
                  }}
                }}
            ";
        }
    }

    public class WebhookDeleteMutationInput : BaseMutationInput
    {
        public string id { get; set; }
    }

    public class WebhookDeleteMutationOutput : BaseMutationOutput
    {
        public Result webhookSubscriptionDelete { get; set; }

        public class Result
        {
            public string deletedWebhookSubscriptionId { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
