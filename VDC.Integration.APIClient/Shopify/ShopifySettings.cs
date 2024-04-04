namespace VDC.Integration.APIClient.Shopify
{
    public class ShopifySettings
    {
        public string BaseShopUrl { get; set; }
        public string Version { get; set; }
        public string CarrierServiceEndpoint { get; set; }

        public string GetStoreUrl(string shopifyStoreDomain) => string.Format(BaseShopUrl, shopifyStoreDomain);

        public string GetStoreAdmin(string shopifyStoreDomain)
            => string.Format($"{GetStoreUrl(shopifyStoreDomain)}/admin");
        public string GetStoreAdminOrderUrl(string shopifyStoreDomain, string orderId)
            => string.Format($"{GetStoreAdmin(shopifyStoreDomain)}/orders/{{0}}", orderId);
    }
}
