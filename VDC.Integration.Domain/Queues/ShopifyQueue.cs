using System.Collections.Generic;

namespace VDC.Integration.Domain.Queues
{
    public static class ShopifyQueue
    {
        public static string UpdateFullProductQueue = "shopify-updatefullproductqueue";
        public static string UpdatePartialProductQueue = "shopify-updatepartialproductqueue";
        public static string UpdatePartialSkuQueue = "shopify-updatepartialskuqueue";
        public static string UpdatePriceQueue = "shopify-updatepricequeue";
        public static string UpdateStockQueue = "shopify-updatestockqueue";
        public static string ListOrderQueue = "shopify-listorderqueue";
        public static string UpdateOrderStatusQueue = "shopify-updateorderstatusqueue";
        public static string UpdateOrderNumberTagQueue = "shopify-updateordernumbertagqueue";
        public static string UpdateVendorQueue = "shopify-updatevendorqueue";
        public static string UpdateProductGroupingQueue = "shopify-updateproductgroupingqueue";
        public static string UpdateProductImagesQueue = "shopify-updateproductimagesqueue";
        public static string ListCategoryProductsToUpdateQueue = "shopify-listcategoryproductstoupdate";
        public static string UpdateAllCollectionsQueue = "shopify-updateallcollectionsqueue";
        public static string UpdateTrackingOrderQueue = "shopify-updatetrackingorderqueue";
        public static string UpdateStockKitQueue = "shopify-updatestockkitqueue";
        public static string UpdateProductKit = "shopify-updateproductkitqueue";


        public static List<string> GetAllQueues()
        {
            return new List<string> {
                UpdateFullProductQueue,
                UpdatePartialProductQueue,
                UpdatePartialSkuQueue,
                UpdatePriceQueue,
                UpdateStockQueue,
                ListOrderQueue,
                UpdateOrderStatusQueue,
                UpdateOrderNumberTagQueue,
                UpdateVendorQueue,
                UpdateProductGroupingQueue,
                UpdateProductImagesQueue,
                ListCategoryProductsToUpdateQueue,
                UpdateAllCollectionsQueue,
                UpdateTrackingOrderQueue,
                UpdateStockKitQueue,
                UpdateProductKit
            };
        }
    }
}
