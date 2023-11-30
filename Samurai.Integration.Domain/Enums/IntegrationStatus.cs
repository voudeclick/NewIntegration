using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Enums
{
    public enum IntegrationStatus
    {
        Received,
        SendendToShopifyQueue,
        Processed
    }
}
