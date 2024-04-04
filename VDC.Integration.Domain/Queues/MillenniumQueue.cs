using System.Collections.Generic;

namespace VDC.Integration.Domain.Queues
{
    public static class MillenniumQueue
    {
        public static string ListFullProductQueue = "millennium-listfullproductqueue";
        public static string ListOrderQueue = "millennium-listorderqueue";
        public static string UpdateOrderQueue = "millennium-updateorderqueue";
        public static string GetPriceProductQueue = "millennium-getpriceproductqueue";
        public static string CreateOrderQueue = "millennium-createorderqueue";
        public static string ProcessProductImageQueue = "millennium-processproductimagequeue";



        public static List<string> GetAllQueues()
        {
            return new List<string> { ListFullProductQueue, ListOrderQueue, UpdateOrderQueue, GetPriceProductQueue, CreateOrderQueue, ProcessProductImageQueue };
        }
    }
}
