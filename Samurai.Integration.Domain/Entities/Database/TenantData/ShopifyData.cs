using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Enums.Millennium;
using System.Collections.Generic;
using System.Text.Json;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class ShopifyData : BaseEntity
    {
        public bool ProductIntegrationStatus { get; set; }
        public bool SetProductsAsUnpublished { get; set; }
        public BodyIntegrationType BodyIntegrationType { get; set; }
        public bool ProductGroupingEnabled { get; set; }
        public bool ProductDescriptionIsHTML { get; set; }
        public bool WriteCategoryNameTags { get; set; }
        public bool ImageIntegrationEnabled { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public bool DisableCustomerDocument { get; set; }
        public bool DisableAddressParse { get; set; }
        public bool ParsePhoneDDD { get; set; }
        public string ShopifyStoreDomain { get; set; }
        public string ShopifyAppJson { get; set; }
        public int DaysToDelivery { get; set; }
        public long MinOrderId { get; set; }
        public bool HasProductKit { get; set; }
        public bool EnableStockProductKit { get; set; }
        public bool BlockFulfillmentNotificationPerShipmentService { get; set; }
        public string ShipmentServicesForFulfillmentNotification { get; set; }
        public int DelayProcessOfShopifyUpdateOrderMessages { get; set; }
        public bool EnableSaveIntegrationInformations { get; set; }
        public bool NotConsiderProductIfPriceIsZero { get; set; }
        public SkuFieldType SkuFieldType { get; set; }
        public bool EnableMaxVariantsQueryGraphQL { get; set; }
        public string MaxVariantsQueryGraphQL { get; set; }
        public bool EnableScheduledNextHour { get; set; }
        public bool EnableAuxiliaryCountry { get; set; }
        public bool DisableUpdateProduct { get; set; }

        public List<ShopifyApp> GetShopifyApps()
        {            
            if (string.IsNullOrWhiteSpace(ShopifyAppJson))
                return new List<ShopifyApp>();
            return JsonSerializer.Deserialize<List<ShopifyApp>>(ShopifyAppJson);
        }

        public void SetShopifyApps(List<ShopifyApp> value)
        {
            ShopifyAppJson = JsonSerializer.Serialize(value);
        }

        public void HideSensitiveData()
        {
            ShopifyAppJson = string.Empty;
        }
    }

    public class ShopifyApp
    {
        public ShopifyApp() { }
        public ShopifyApp(ShopifyApp app)
        {
            ShopifyKey = app.ShopifyKey;
            ShopifyPassword = app.ShopifyPassword;
            ShopifySecret = app.ShopifySecret;
        }
        public string ShopifyKey { get; set; }
        public string ShopifyPassword { get; set; }
        public string ShopifySecret { get; set; }
        public bool Webhook { get; set; }
    }
}
