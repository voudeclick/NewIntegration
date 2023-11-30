using System.Collections.Generic;

namespace Samurai.Integration.Domain.Queues
{
    public static class OmieQueue
    {
        public static string ListFullProductQueue = "omie-listfullproductqueue";
        public static string ListOrderQueue = "omie-listorderqueue";
        public static string UpdateOrderQueue = "omie-updateorderqueue";
        public static string ListAllProductsQueue = "omie-listallproductsqueue";
        public static string ListStockQueue = "omie-liststockqueue";
        public static string ListPartialProductQueue = "omie-listpartialproductqueue";

        public static List<string> GetAllQueues()
        {
            return new List<string> { 
                ListFullProductQueue, 
                ListOrderQueue,
                UpdateOrderQueue,
                ListAllProductsQueue,
                ListStockQueue,
                ListPartialProductQueue
            };
        }
    }
}
