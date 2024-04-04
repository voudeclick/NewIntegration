using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Entities.Database.MethodPayment;
using VDC.Integration.Domain.Entities.Database.TenantData;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Models.Millennium;
using VDC.Integration.Domain.Results;
using VDC.Integration.WebApi.ViewModels.TenantData;

namespace VDC.Integration.WebApi.ViewModels
{
    public class TenantViewModel : BaseViewModel
    {
        /// <summary>
        /// PrimaryKey da tabela Tenants
        /// </summary>
        public long Id { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public ShopifyDataViewModel ShopifyData { get; set; }
        /// <summary>
        /// Ativa ou Desativa Integracao
        /// </summary>
        public bool Status { get; set; }
        public bool MultiLocation { get; set; }
        public bool TenantLogging { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public IntegrationType IntegrationType { get; set; }
        public TenantType Type { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DeactivatedDate { get; set; }
        public MillenniumDataViewModel MilleniumData { get; set; }
        public OmieDataViewModel OmieData { get; set; }
        public LocationMapViewModel LocationMap { get; set; } = new LocationMapViewModel();
        public List<MethodPaymentTypeViewModel> MethodPayments { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(StoreName))
                result.AddError("Operação inválida", "Nome inválido.", GetType().FullName);

            if (IntegrationType == IntegrationType.Shopify)
            {
                if (string.IsNullOrWhiteSpace(ShopifyData.ShopifyStoreDomain))
                    result.AddError("Operação inválida", "url shopify inválida.", GetType().FullName);

                if (ShopifyData.ShopifyApps?.Any() != true)
                    result.AddError("Operação inválida", "app shopify inválida.", GetType().FullName);
                else
                    ShopifyData.ShopifyApps.ForEach(x => result.Merge(x.IsValid()));
            }

            if (Type == TenantType.Millennium)
            {
                if (MilleniumData == null)
                    result.AddError("Operação inválida", "Dados de configuração Millenium inválidos.", GetType().FullName);
                else
                    result.Merge(MilleniumData.IsValid());
            }

            if (Type == TenantType.Omie)
            {
                if (OmieData == null)
                    result.AddError("Operação inválida", "Dados de configuração Omie inválidos.", GetType().FullName);
                else
                    result.Merge(OmieData.IsValid());
            }

            return result;
        }

        public TenantViewModel()
        {
        }

        public TenantViewModel(Tenant entity)
        {
            if (entity.IntegrationType == IntegrationType.Shopify && entity.ShopifyData != null)
                ShopifyData = new ShopifyDataViewModel(entity.ShopifyData);

            Status = entity.Status;
            OrderIntegrationStatus = entity.OrderIntegrationStatus;
            ProductIntegrationStatus = entity.ProductIntegrationStatus;
            Id = entity.Id;
            StoreName = entity.StoreName;
            StoreHandle = entity.StoreHandle;
            IntegrationType = entity.IntegrationType;
            Type = entity.Type;
            MultiLocation = entity.MultiLocation;
            TenantLogging = entity.TenantLogging;
            CreationDate = entity.CreationDate;
            DeactivatedDate = entity.DeactivatedDate;

            if (Type == TenantType.Millennium && entity.MillenniumData != null)
            {
                MethodPayments = entity.MethodPayments.Select(s => new MethodPaymentTypeViewModel(s)).ToList();
                MilleniumData = new MillenniumDataViewModel(entity.MillenniumData);
            }

            if (Type == TenantType.Omie && entity.OmieData != null)
                OmieData = new OmieDataViewModel(entity.OmieData);

            if (entity.LocationMap != null)
                LocationMap = new LocationMapViewModel(entity.LocationMap);
        }
    }


    public static class TenantViewModelExtensions
    {
        public static void UpdateFrom(this Tenant entity, TenantViewModel viewModel)
        {
            if (entity.Id == 0)
                entity.StoreHandle = Regex.Replace(Regex.Replace(viewModel.StoreName, @"[\W]+", "-"), @"[-]+", "-").ToLowerInvariant();

            entity.Status = viewModel.Status;
            entity.Type = viewModel.Type; //define erp
            entity.IntegrationType = viewModel.IntegrationType; //define ecommerce
            entity.StoreName = viewModel.StoreName;
            entity.OrderIntegrationStatus = viewModel.OrderIntegrationStatus;
            entity.ProductIntegrationStatus = viewModel.ProductIntegrationStatus;
            entity.MultiLocation = viewModel.MultiLocation;
            entity.TenantLogging = viewModel.TenantLogging;

            if (entity.IntegrationType == IntegrationType.Shopify)
            {
                if (entity.ShopifyData == null)
                    entity.ShopifyData = new ShopifyData();
                entity.ImageIntegrationEnabled = viewModel.ShopifyData.ImageIntegrationEnabled;
                viewModel.ShopifyData.ProductIntegrationStatus = entity.ProductIntegrationStatus;
                viewModel.ShopifyData.OrderIntegrationStatus = entity.OrderIntegrationStatus;
                entity.ShopifyData.UpdateFrom(viewModel.ShopifyData);
            }

            if (entity.Type == TenantType.Millennium)
            {
                if (entity.MillenniumData == null)
                    entity.MillenniumData = new MillenniumData();

                entity.MillenniumData.EnableProductKit = viewModel.ShopifyData.HasProductKit;
                entity.ShopifyData.SkuFieldType = viewModel.MilleniumData.SkuFieldType;
                entity.MillenniumData.UpdateFrom(viewModel.MilleniumData);

                entity.MethodPayments = new List<MethodPayment>();

                if (viewModel.MethodPayments.Count > 0 && viewModel.MethodPayments.FirstOrDefault().PaymentTypeShopify != null)
                {
                    viewModel.MethodPayments.ForEach(delegate (MethodPaymentTypeViewModel methodPayment)
                    {
                        var methodPaymentType = new MethodPayment();
                        methodPaymentType.Up(methodPayment.PaymentTypeShopify, methodPayment.PaymentTypeMillenniun, entity.Id);
                        entity.MethodPayments.Add(methodPaymentType);
                    });
                }
            }

            if (entity.Type == TenantType.Omie)
            {
                if (entity.OmieData == null)
                    entity.OmieData = new OmieData();
                entity.OmieData.UpdateFrom(viewModel.OmieData);
            }

            if (entity.MultiLocation)
            {
                if (entity.LocationMap == null)
                    entity.LocationMap = new LocationMap();
                entity.LocationMap.UpdateFrom(viewModel.LocationMap);
            }
        }
    }
}
