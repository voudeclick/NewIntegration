﻿using Samurai.Integration.APIClient.Shopify.Models.Request.Inputs;
using Samurai.Integration.Domain.Shopify.Models.Results;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class WebhookCreateMutation : BaseMutation<WebhookCreateMutationInput, WebhookCreateMutationOutput>
    {
        public WebhookCreateMutation(WebhookCreateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation webhookSubscriptionCreate($topic: WebhookSubscriptionTopic!, $webhookSubscription: WebhookSubscriptionInput!) {{
                  webhookSubscriptionCreate(topic: $topic, webhookSubscription: $webhookSubscription) {{
                    userErrors {{
                      field
                      message
                    }}
                    webhookSubscription {{
                      id
                    }}
                  }}
                }}
            ";
        }
    }

    public class WebhookCreateMutationInput : BaseMutationInput
    {
        public string topic { get; set; }
        public WebhookSubscription webhookSubscription { get; set; }
    }

    public class WebhookCreateMutationOutput : BaseMutationOutput
    {
        public Result webhookSubscriptionCreate { get; set; }

        public class Result
        {
            public WebhookSubscriptonResult webhookSubscription { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
