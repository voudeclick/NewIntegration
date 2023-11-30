using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Queues
{
    public static class PluggToQueue
    {
        public static string ListAllProductsQueue = "pluggto-listallproductsqueue";
        public static string ListFullProductQueue = "pluggto-listfullproductqueue";

        public static string ListOrderQueue = "pluggto-listorderqueue";
        public static string GetPriceProductQueue = "pluggto-getpriceproductqueue";
        public static string CreateOrderQueue = "pluggto-createorderqueue";

        public static List<string> GetAllQueues()
        {
            return new List<string>
            {
                ListAllProductsQueue,
                ListFullProductQueue,
                ListOrderQueue,
                GetPriceProductQueue,
                CreateOrderQueue
            };
        }
    }
}
