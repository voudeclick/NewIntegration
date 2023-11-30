using Samurai.Integration.Domain;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.WebApi.ViewModels.TenantData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Samurai.Integration.WebApi.ViewModels
{
    public class TenantViewModel : BaseViewModel
    {
        /// <summary>
        /// PrimaryKey da tabela Tenants
        /// </summary>
        public long Id { get; set; }
        public bool ProductIntegrationStatus { get; set; }       
        public bool OrderIntegrationStatus { get; set; }
        public bool EnablePier8Integration { get; set; }

        public ShopifyDataViewModel ShopifyData { get; set; }
        public SellerCenterDataViewModel SellerCenterData { get; set; }
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
        public NexaasDataViewModel NexaasData { get; set; }
        public OmieDataViewModel OmieData { get; set; }
        public Pier8DataViewModel Pier8Data { get; set; }
        public PluggToDataViewModel PluggToData { get; set; }
        public LocationMapViewModel LocationMap { get; set; } = new LocationMapViewModel();
        public BlingDataViewModel BlingData { get; set; }

        public List<MethodPaymentTypeViewModel> MethodPayments { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(StoreName))
                result.AddError("Operação inválida", "Nome inválido.", GetType().FullName);

            //TODO -> incluir validacao sellercenter
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

            if (Type == TenantType.Nexaas)
            {
                if (NexaasData == null)
                    result.AddError("Operação inválida", "Dados de configuração Nexaas inválidos.", GetType().FullName);
                else
                    result.Merge(NexaasData.IsValid());
            }

            if (Type == TenantType.Omie)
            {
                if (OmieData == null)
                    result.AddError("Operação inválida", "Dados de configuração Omie inválidos.", GetType().FullName);
                else
                    result.Merge(OmieData.IsValid());
            }

            if (Type == TenantType.PluggTo)
            {
                if (PluggToData == null)
                    result.AddError("Operação inválida", "Dados de configuração PluggTo inválidos.", GetType().FullName);
                else
                    result.Merge(PluggToData.IsValid());
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

            if (entity.IntegrationType == IntegrationType.SellerCenter && entity.SellerCenterData != null)
                SellerCenterData = new SellerCenterDataViewModel(entity.SellerCenterData);
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
            EnablePier8Integration = entity.EnablePier8Integration;            
            CreationDate = entity.CreationDate;
            DeactivatedDate = entity.DeactivatedDate;

            if (Type == TenantType.Millennium && entity.MillenniumData != null)
            {
                MethodPayments = entity.MethodPayments.Select(s => new MethodPaymentTypeViewModel(s)).ToList();
                MilleniumData = new MillenniumDataViewModel(entity.MillenniumData);
            }

            if (Type == TenantType.Nexaas && entity.NexaasData != null)
                NexaasData = new NexaasDataViewModel(entity.NexaasData);

            if (Type == TenantType.Omie && entity.OmieData != null)
                OmieData = new OmieDataViewModel(entity.OmieData);

            if (entity.LocationMap != null)
                LocationMap = new LocationMapViewModel(entity.LocationMap);

            if (entity.Pier8Data != null)
                Pier8Data = new Pier8DataViewModel(entity.Pier8Data);

            if (Type == TenantType.Bling && entity.BlingData != null)
                BlingData = new BlingDataViewModel(entity.BlingData);

            if (Type == TenantType.PluggTo && entity.PluggToData != null)
                PluggToData = new PluggToDataViewModel(entity.PluggToData);
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
            entity.EnablePier8Integration = viewModel.EnablePier8Integration;

            if (entity.IntegrationType == IntegrationType.Shopify)
            {
                if (entity.ShopifyData == null)
                    entity.ShopifyData = new ShopifyData();
                entity.ImageIntegrationEnabled = viewModel.ShopifyData.ImageIntegrationEnabled;
                viewModel.ShopifyData.ProductIntegrationStatus = entity.ProductIntegrationStatus;
                viewModel.ShopifyData.OrderIntegrationStatus = entity.OrderIntegrationStatus;
                entity.ShopifyData.UpdateFrom(viewModel.ShopifyData);
            }

            if (entity.IntegrationType == IntegrationType.SellerCenter)
            {
                if (entity.SellerCenterData == null)
                    entity.SellerCenterData = new SellerCenterData();
                entity.SellerCenterData.UpdateFrom(viewModel.SellerCenterData);
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

            if (entity.Type == TenantType.Nexaas)
            {
                if (entity.NexaasData == null)
                    entity.NexaasData = new NexaasData();
                entity.NexaasData.UpdateFrom(viewModel.NexaasData);
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

            if (entity.EnablePier8Integration)
            {
                if (entity.Pier8Data == null)
                    entity.Pier8Data = new Pier8Data();
                entity.Pier8Data.UpdateFrom(viewModel.Pier8Data);
            }

            if (entity.Type == TenantType.Bling)
            {
                if (entity.BlingData == null)
                    entity.BlingData = new BlingData();
                entity.BlingData.UpdateFrom(viewModel.BlingData);
            }

            if (entity.Type == TenantType.PluggTo)
            {
                if (entity.PluggToData == null)
                    entity.PluggToData = new PluggToData();
                entity.PluggToData.UpdateFrom(viewModel.PluggToData);
            }


        }
    }
}
