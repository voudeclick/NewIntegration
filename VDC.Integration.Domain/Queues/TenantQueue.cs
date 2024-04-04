using System.Collections.Generic;

namespace VDC.Integration.Domain.Queues
{
    public static class TenantQueue
    {
        public static string UpdateShopifyWebjobQueue = "_shopify-updatewebjobqueue";
        public static string UpdateMillenniumWebjobQueue = "_millennium-updatewebjobqueue";
        public static string UpdateOmieWebjobQueue = "_omie-updatewebjobqueue";
        public static List<string> GetAllQueues()
        {
            return new List<string> {
                UpdateShopifyWebjobQueue,
                UpdateMillenniumWebjobQueue,
                UpdateOmieWebjobQueue
            };
        }
    }
}
