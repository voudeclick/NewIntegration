using System.Collections.Generic;

namespace Samurai.Integration.Domain.Queues
{
    public static class NexaasQueue
    {
        public static string ListFullProductQueue = "nexxas-listfullproductqueue";
        public static string ListOrderQueue = "nexxas-listorderqueue";
        public static string UpdateOrderQueue = "nexxas-updateorderqueue";
        public static string ListAllProductsQueue = "nexxas-listallproductsqueue";
        public static string ListStockQueue = "nexxas-liststockqueue";
        public static string ListVendorQueue = "nexxas-listvendorqueue";
        public static string ListProductCategoriesQueue = "nexxas-listproductcategoriesqueue";
        public static string ListPartialProductQueue = "nexxas-listpartialproductqueue";

        public static List<string> GetAllQueues()
        {
            return new List<string> { 
                ListFullProductQueue, 
                ListOrderQueue,
                UpdateOrderQueue,
                ListAllProductsQueue,
                ListStockQueue,
                ListVendorQueue,
                ListProductCategoriesQueue,
                ListPartialProductQueue
            };
        }
    }
}
