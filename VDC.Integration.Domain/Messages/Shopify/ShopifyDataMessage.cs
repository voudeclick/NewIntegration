using System.Collections.Generic;
using System.Linq;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Entities.Database.TenantData;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Queues;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyDataMessage : ShopifyData, IBaseQueue
    {
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public TenantType Type { get; set; }
        public List<ShopifyApp> ShopifyApps { get; set; }
        public Dictionary<TenantType, string> OrderPrefix { get; set; }
        public LocationMap LocationMap { get; set; }
        public bool EnabledMultiLocation { get; set; }
        public bool EnableTenantLogging { get; set; }
        public ShopifyDataMessage(Tenant tenant)
        {
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            SetProductsAsUnpublished = tenant.ShopifyData.SetProductsAsUnpublished;
            BodyIntegrationType = tenant.ShopifyData.BodyIntegrationType;
            ProductGroupingEnabled = tenant.ShopifyData.ProductGroupingEnabled;
            ProductDescriptionIsHTML = tenant.ShopifyData.ProductDescriptionIsHTML;
            WriteCategoryNameTags = tenant.ShopifyData.WriteCategoryNameTags;
            ImageIntegrationEnabled = tenant.ImageIntegrationEnabled;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;
            DisableCustomerDocument = tenant.ShopifyData.DisableCustomerDocument;
            DisableAddressParse = tenant.ShopifyData.DisableAddressParse;
            ParsePhoneDDD = tenant.ShopifyData.ParsePhoneDDD;
            ShopifyStoreDomain = tenant.ShopifyData.ShopifyStoreDomain;
            Type = tenant.Type;
            Type = tenant.Type;
            DaysToDelivery = tenant.ShopifyData.DaysToDelivery;
            MinOrderId = tenant.ShopifyData.MinOrderId;
            OrderPrefix = new Dictionary<TenantType, string>();

            if (Type == TenantType.Millennium)
            {
                OrderPrefix.Add(TenantType.Millennium, tenant.MillenniumData.OrderPrefix);
                SkuFieldType = tenant.MillenniumData.SkuFieldType;
            }

            if (Type == TenantType.Omie)
                OrderPrefix.Add(TenantType.Omie, tenant.OmieData.OrderPrefix);
            ShopifyApps = tenant.ShopifyData.GetShopifyApps().Select(x => new ShopifyApp(x)).ToList();
            HasProductKit = tenant.ShopifyData.HasProductKit;
            EnableStockProductKit = tenant.ShopifyData.EnableStockProductKit;
            LocationMap = tenant.LocationMap;
            EnabledMultiLocation = tenant.MultiLocation;
            EnableTenantLogging = tenant.TenantLogging;
            BlockFulfillmentNotificationPerShipmentService = tenant.ShopifyData.BlockFulfillmentNotificationPerShipmentService;
            ShipmentServicesForFulfillmentNotification = tenant.ShopifyData.ShipmentServicesForFulfillmentNotification;
            DelayProcessOfShopifyUpdateOrderMessages = tenant.ShopifyData.DelayProcessOfShopifyUpdateOrderMessages;
            EnableSaveIntegrationInformations = tenant.ShopifyData.EnableSaveIntegrationInformations;
            NotConsiderProductIfPriceIsZero = tenant.ShopifyData.NotConsiderProductIfPriceIsZero;
            EnableMaxVariantsQueryGraphQL = tenant.ShopifyData.EnableMaxVariantsQueryGraphQL;
            MaxVariantsQueryGraphQL = tenant.ShopifyData.MaxVariantsQueryGraphQL;
            EnableScheduledNextHour = tenant.ShopifyData.EnableScheduledNextHour;
            EnableAuxiliaryCountry = tenant.ShopifyData.EnableAuxiliaryCountry;
            DisableUpdateProduct = tenant.ShopifyData.DisableUpdateProduct;
        }

        public bool EqualsTo(ShopifyDataMessage data)
        {
            return
                Id == data.Id &&
                StoreName == data.StoreName &&
                StoreHandle == data.StoreHandle &&
                ProductIntegrationStatus == data.ProductIntegrationStatus &&
                SetProductsAsUnpublished == data.SetProductsAsUnpublished &&
                BodyIntegrationType == data.BodyIntegrationType &&
                ProductGroupingEnabled == data.ProductGroupingEnabled &&
                ProductDescriptionIsHTML == data.ProductDescriptionIsHTML &&
                WriteCategoryNameTags == data.WriteCategoryNameTags &&
                ImageIntegrationEnabled == data.ImageIntegrationEnabled &&
                OrderIntegrationStatus == data.OrderIntegrationStatus &&
                DisableCustomerDocument == data.DisableCustomerDocument &&
                DisableAddressParse == data.DisableAddressParse &&
                ParsePhoneDDD == data.ParsePhoneDDD &&
                ShopifyStoreDomain == data.ShopifyStoreDomain &&
                Type == data.Type &&
                DaysToDelivery == data.DaysToDelivery &&
                MinOrderId == data.MinOrderId &&
                OrderPrefix.Count == data.OrderPrefix.Count &&
                OrderPrefix.All(x =>
                    data.OrderPrefix.ContainsKey(x.Key) &&
                    x.Value == data.OrderPrefix[x.Key]
                ) &&
                ShopifyApps.Count == data.ShopifyApps.Count &&
                ShopifyApps.All(x =>
                {
                    var appData = data.ShopifyApps[ShopifyApps.IndexOf(x)];
                    return
                        x.ShopifyKey == appData.ShopifyKey &&
                        x.ShopifyPassword == appData.ShopifyPassword &&
                        x.ShopifySecret == appData.ShopifySecret;
                }) &&
                HasProductKit == data.HasProductKit &&
                LocationMap == data.LocationMap &&
                EnabledMultiLocation == data.EnabledMultiLocation &&
                EnableStockProductKit == data.EnableStockProductKit;

        }

        public bool CanMoveSku
        {
            get
            {
                return Type == TenantType.Omie;
            }
        }
    }
}
