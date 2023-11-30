using System.Text.RegularExpressions;

namespace Samurai.Integration.APIClient.Shopify.Models
{
    public static class Tags
    {
        public static string ProductExternalId = "ExtId";
        public static string ProductExternalCode = "ExtCode";
        public static string ProductGroupingReference = "GroupRef";
        public static string ProductCollectionId = "CollId";
        public static string ProductCollection = "Coll";
        public static string ProductCategoryName = "CatName";
        public static string ProductVendorId = "VendId";
        public static string ProductStatus = "Status";
        public static string SkuExternalCode = "ExtCode";
        public static string OrderExternalId = "ExtId";
        public static string OrderNumber = "Num";
        public static string OrderIsIntegrated = "IsIntg";
        public static string OrderIntegrationStatus = "IntgSt";
        public static string OrderProcessedStockKit = "ProcessedStockKit";
        public static string ProductKit = "KIT:SIM";
        public static string ProductKitSKU = "KITSKU:";
        public static string SkuOriginal = "SkuOriginal";
        public static string SufixoTag = "Intg";

        public static string ProductKitSkuWithStock(string sku, int qtdItem) => $"KITSKU:{sku}|{qtdItem}";
        public static string GetProductKitSku(string sku) => $"{ProductKitSKU}{sku}";

        public static string GetSkuInTagKit(string tag)
        {
            var documentoMatch = Regex.Match(tag, @"(?<=:).*(?=\|)");

            return documentoMatch.Value;
        }
    }
}
