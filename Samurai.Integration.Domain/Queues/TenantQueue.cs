using System.Collections.Generic;

namespace Samurai.Integration.Domain.Queues
{
    public static class TenantQueue
    {
        public static string UpdateShopifyWebjobQueue = "_shopify-updatewebjobqueue";
        public static string UpdateMillenniumWebjobQueue = "_millennium-updatewebjobqueue";
        public static string UpdateNexasWebjobQueue = "_nexas-updatewebjobqueue";
        public static string UpdateOmieWebjobQueue = "_omie-updatewebjobqueue";
        public static string UpdateSellerCenterWebjobQueue = "_sellercenter-updatewebjobqueue";
        public static string UpdatePier8WebjobQueue = "_pier8-updatewebjobqueue";
        public static string UpdateBlingWebjobQueue = "_bling-updatewebjobqueue";
        public static string UpdatePluggToWebjobQueue = "_pluggto-updatewebjobqueue";

        public static string UpdateTrayWebjobQueue = "_tray-updatewebjobqueue";
        public static string UpdateAliExpressWebJobQueue = "_aliexpress-updatewebjobqueue";

        public static List<string> GetAllQueues()
        {
            return new List<string> {
                UpdateShopifyWebjobQueue,
                UpdateMillenniumWebjobQueue,
                UpdateNexasWebjobQueue,
                UpdateOmieWebjobQueue,
                UpdateSellerCenterWebjobQueue,
                UpdatePier8WebjobQueue,
                UpdateBlingWebjobQueue,
                UpdatePluggToWebjobQueue
            };
        }
        public static List<string> GetQueuesTray()
        {
            return new List<string> {
                UpdateTrayWebjobQueue,
                UpdateAliExpressWebJobQueue
            };
        }
    }
}
