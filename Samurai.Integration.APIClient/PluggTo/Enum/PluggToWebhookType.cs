using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Enum
{
    public enum PluggToWebhookType
    {
        orders,
        products
    }

    public enum PluggToWebhookAction
    {
        created,
        updated
    }
}
