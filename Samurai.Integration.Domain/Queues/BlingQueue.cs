using System.Collections.Generic;

namespace Samurai.Integration.Domain.Queues
{
    public static class BlingQueue
    {
        public static string ListProductsUpdatedQueue = "bling-listproductsupdatedqueue";
        public static string ListFullProductQueue = "bling-listfullproductqueue";
        public static string ListAllProductsQueue = "bling-listallproductsqueue";
        public static string ListOrderQueue = "bling-listorderqueue";
        public static string GetPriceProductQueue = "bling-getpriceproductqueue";
        public static string CreateOrderQueue = "bling-createorderqueue";
        public static string UpdateOrderQueue = "bling-updateorderqueue";


        public static List<string> GetAllQueues()
        {
            return new List<string>
            {
                ListProductsUpdatedQueue,
                ListFullProductQueue,
                ListAllProductsQueue,
                ListOrderQueue,
                GetPriceProductQueue,
                CreateOrderQueue,
                UpdateOrderQueue
            };
        }
    }
}
