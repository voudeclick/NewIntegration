using System.Collections.Generic;
using System.Linq;
using System.Net;
using VDC.Integration.Domain.Entities.Database.TenantData;
using VDC.Integration.Domain.Results;

namespace VDC.Integration.WebApi.ViewModels.TenantData
{
    public class ShopifyDataViewModel : ShopifyData
    {
        public ShopifyDataViewModel()
        {

        }

        public ShopifyDataViewModel(ShopifyData entity)
        {
            Id = entity.Id;
            ProductIntegrationStatus = entity.ProductIntegrationStatus;
            SetProductsAsUnpublished = entity.SetProductsAsUnpublished;
            BodyIntegrationType = entity.BodyIntegrationType;
            ProductGroupingEnabled = entity.ProductGroupingEnabled;
            ProductDescriptionIsHTML = entity.ProductDescriptionIsHTML;
            WriteCategoryNameTags = entity.WriteCategoryNameTags;
            ImageIntegrationEnabled = entity.ImageIntegrationEnabled;
            OrderIntegrationStatus = entity.OrderIntegrationStatus;
            DisableCustomerDocument = entity.DisableCustomerDocument;
            DisableAddressParse = entity.DisableAddressParse;
            ParsePhoneDDD = entity.ParsePhoneDDD;
            DaysToDelivery = entity.DaysToDelivery;
            MinOrderId = entity.MinOrderId;
            ShopifyStoreDomain = entity.ShopifyStoreDomain;
            ShopifyApps = entity.GetShopifyApps()?.Select(x => new ShopifyAppViewModel(x)).ToList();
            HasProductKit = entity.HasProductKit;
            EnableStockProductKit = entity.EnableStockProductKit;
            BlockFulfillmentNotificationPerShipmentService = entity.BlockFulfillmentNotificationPerShipmentService;
            ShipmentServicesForFulfillmentNotification = entity.ShipmentServicesForFulfillmentNotification;
            DelayProcessOfShopifyUpdateOrderMessages = entity.DelayProcessOfShopifyUpdateOrderMessages;
            EnableSaveIntegrationInformations = entity.EnableSaveIntegrationInformations;
            NotConsiderProductIfPriceIsZero = entity.NotConsiderProductIfPriceIsZero;
            EnableMaxVariantsQueryGraphQL = entity.EnableMaxVariantsQueryGraphQL;
            MaxVariantsQueryGraphQL = entity.MaxVariantsQueryGraphQL;
            SkuFieldType = entity.SkuFieldType;
            EnableScheduledNextHour = entity.EnableScheduledNextHour;
            EnableAuxiliaryCountry = entity.EnableAuxiliaryCountry;
            DisableUpdateProduct = entity.DisableUpdateProduct;
        }
        public List<ShopifyAppViewModel> ShopifyApps { get; set; }

    }
    public static class ShopifyDateViewModelExtensions
    {
        public static void UpdateFrom(this ShopifyData entity, ShopifyDataViewModel viewModel)
        {
            entity.ProductIntegrationStatus = viewModel.ProductIntegrationStatus;
            entity.SetProductsAsUnpublished = viewModel.SetProductsAsUnpublished;
            entity.BodyIntegrationType = viewModel.BodyIntegrationType;
            entity.ProductDescriptionIsHTML = viewModel.ProductDescriptionIsHTML;
            entity.WriteCategoryNameTags = viewModel.WriteCategoryNameTags;
            entity.ImageIntegrationEnabled = viewModel.ImageIntegrationEnabled;
            entity.ProductGroupingEnabled = viewModel.ProductGroupingEnabled;
            entity.OrderIntegrationStatus = viewModel.OrderIntegrationStatus;
            entity.DisableCustomerDocument = viewModel.DisableCustomerDocument;
            entity.DisableAddressParse = viewModel.DisableAddressParse;
            entity.ParsePhoneDDD = viewModel.ParsePhoneDDD;
            entity.ShopifyStoreDomain = viewModel.ShopifyStoreDomain;
            entity.DaysToDelivery = viewModel.DaysToDelivery;
            entity.MinOrderId = viewModel.MinOrderId;
            entity.SetShopifyApps(new List<ShopifyApp>() { new ShopifyApp()
                    { ShopifyKey = viewModel.ShopifyApps.Select(x => x.ShopifyKey).First(),
                      ShopifySecret = viewModel.ShopifyApps.Select(x => x.ShopifySecret).First(),
                      ShopifyPassword = viewModel.ShopifyApps.Select(x => x.ShopifyPassword).First(),
                      Webhook = true
                    } 
            });


            entity.HasProductKit = viewModel.HasProductKit;
            entity.EnableStockProductKit = viewModel.EnableStockProductKit;
            entity.BlockFulfillmentNotificationPerShipmentService = viewModel.BlockFulfillmentNotificationPerShipmentService;
            entity.ShipmentServicesForFulfillmentNotification = viewModel.ShipmentServicesForFulfillmentNotification;
            entity.DelayProcessOfShopifyUpdateOrderMessages = viewModel.DelayProcessOfShopifyUpdateOrderMessages;
            entity.EnableSaveIntegrationInformations = viewModel.EnableSaveIntegrationInformations;
            entity.NotConsiderProductIfPriceIsZero = viewModel.NotConsiderProductIfPriceIsZero;
            entity.EnableMaxVariantsQueryGraphQL = viewModel.EnableMaxVariantsQueryGraphQL;
            entity.MaxVariantsQueryGraphQL = viewModel.MaxVariantsQueryGraphQL;
            entity.SkuFieldType = viewModel.SkuFieldType;
            entity.EnableScheduledNextHour = viewModel.EnableScheduledNextHour;
            entity.EnableAuxiliaryCountry = viewModel.EnableAuxiliaryCountry;
            entity.DisableUpdateProduct = viewModel.DisableUpdateProduct;
        }
    }
    public class ShopifyAppViewModel : BaseViewModel
    {
        public string ShopifyKey { get; set; }
        public string ShopifyPassword { get; set; }
        public string ShopifySecret { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(ShopifyKey))
                result.AddError("Operação inválida", "chave inválida.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(ShopifyPassword))
                result.AddError("Operação inválida", "senha inválida.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(ShopifySecret))
                result.AddError("Operação inválida", "segredo inválido.", GetType().FullName);

            return result;
        }

        public ShopifyAppViewModel()
        {
        }

        public ShopifyAppViewModel(ShopifyApp entity)
        {
            ShopifyKey = entity.ShopifyKey;
            ShopifyPassword = entity.ShopifyPassword;
            ShopifySecret = entity.ShopifySecret;
        }
    }
}
