using Samurai.Integration.Domain.Entities;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Queues;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Messages.Millennium
{
    public class MillenniumData : IBaseQueue
    {
        public IntegrationType IntegrationType { get; set; }
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool ProductIntegrationPrice { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public bool DisableCustomerDocument { get; set; }
        public string Url { get; set; }
        public string UrlExtraPaymentInformation { get; set; }
        public long VitrineId { get; set; }
        public bool SplitEnabled { get; set; }
        public bool SaleWithoutStockEnabled { get; set; }
        public string NameField { get; set; }
        public string DescriptionField { get; set; }
        public string OrderPrefix { get; set; }
        public string CorDescription { get; set; }
        public string CorField { get; set; }
        public bool SendDefaultCor { get; set; }
        public string TamanhoDescription { get; set; }
        public string TamanhoField { get; set; }
        public bool SendDefaultTamanho { get; set; }
        public string EstampaDescription { get; set; }
        public string EstampaField { get; set; }
        public bool SendDefaultEstampa { get; set; }
        public MillenniumOperatorType OperatorType { get; set; }
        public List<MillenniumLogin> Logins { get; set; }
        public List<MillenniumExtraFieldConfiguration> ExtraFieldConfigurations { get; set; }
        public bool CapitalizeProductName { get; set; }
        public List<string> ExcludedProductCharacters { get; set; }
        public bool HasMultiLocation { get; set; }
        public bool HasTenantLogging { get; set; }
        public bool HasZeroedPriceCase { get; set; }
        public bool NameSkuEnabled { get; set; }
        public string NameSkuField { get; set; }
        public bool Retry { get; set; } = false;
        public bool ProductImageIntegration { get; set; }
        public bool EnabledStockMto { get; set; }
        public LocationMap LocationMap { get; set; }
        public SkuFieldType SkuFieldType { get; set; }
        public bool ControlStockByUpdateDate { get; set; }
        public bool ControlPriceByUpdateDate { get; set; }
        public bool ControlProductByUpdateDate { get; set; }
        public int NumberOfItensPerAPIQuery { get; set; }
        public bool EnabledApprovedTransaction { get; set; }
        public bool EnableSaveIntegrationInformations { get; set; }
        public bool EnableSaveProcessIntegrations { get; set; }
        public bool EnableExtraPaymentInformation { get; set; }
        public string StoreDomainByBrasPag { get; set; }
        public string SessionToken { get; set; }
        public bool EnableProductKit { get; set; }
        public bool EnableMaskedNSU { get; set; }
        public bool SendPaymentMethod { get; set; }
        public bool EnableProductDiscount { get; set; }
        public MercadoPago MercadoPago { get; set; }




        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string TenantCompositeKey { get => $"{Id}_{StoreHandle}"; }

        public MillenniumData()
        {

        }

        public MillenniumData(Tenant tenant)
        {
            IntegrationType = tenant.IntegrationType;
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;
            DisableCustomerDocument = tenant.DisableCustomerDocument;
            Url = tenant.MillenniumData.Url;
            VitrineId = tenant.MillenniumData.VitrineId;
            SplitEnabled = tenant.MillenniumData.SplitEnabled;
            SaleWithoutStockEnabled = tenant.MillenniumData.SaleWithoutStockEnabled;
            NameField = tenant.MillenniumData.NameField;
            DescriptionField = tenant.MillenniumData.DescriptionField;
            OrderPrefix = tenant.MillenniumData.OrderPrefix;
            CorDescription = tenant.MillenniumData.CorDescription;
            CorField = tenant.MillenniumData.CorField;
            SendDefaultCor = tenant.MillenniumData.SendDefaultCor;
            TamanhoDescription = tenant.MillenniumData.TamanhoDescription;
            TamanhoField = tenant.MillenniumData.TamanhoField;
            SendDefaultTamanho = tenant.MillenniumData.SendDefaultTamanho;
            EstampaDescription = tenant.MillenniumData.EstampaDescription;
            EstampaField = tenant.MillenniumData.EstampaField;
            SendDefaultEstampa = tenant.MillenniumData.SendDefaultEstampa;
            OperatorType = tenant.MillenniumData.OperatorType;
            Logins = tenant.MillenniumData.GetLogins().Select(x => new MillenniumLogin(x)).ToList();
            ExtraFieldConfigurations = tenant.MillenniumData.GetExtraFieldConfigurations().Select(x => new MillenniumExtraFieldConfiguration(x)).ToList();
            CapitalizeProductName = tenant.MillenniumData.CapitalizeProductName;
            ExcludedProductCharacters = tenant.MillenniumData.GetExcludedProductCharacters();
            HasMultiLocation = tenant.MultiLocation;
            NameSkuEnabled = tenant.MillenniumData.NameSkuEnabled;
            NameSkuField = tenant.MillenniumData.NameSkuField;
            ProductImageIntegration = tenant.ImageIntegrationEnabled;
            EnabledStockMto = tenant.MillenniumData.EnabledStockMto;
            LocationMap = tenant.LocationMap;
            SkuFieldType = tenant.MillenniumData.SkuFieldType;
            HasTenantLogging = tenant.TenantLogging;
            HasZeroedPriceCase = tenant.MillenniumData.HasZeroedPriceCase;
            ControlStockByUpdateDate = tenant.MillenniumData.ControlStockByUpdateDate;
            ControlPriceByUpdateDate = tenant.MillenniumData.ControlPriceByUpdateDate;
            ControlProductByUpdateDate = tenant.MillenniumData.ControlProductByUpdateDate;
            NumberOfItensPerAPIQuery = tenant.MillenniumData.NumberOfItensPerAPIQuery;
            EnabledApprovedTransaction = tenant.MillenniumData.EnabledApprovedTransaction;
            EnableSaveIntegrationInformations = tenant.MillenniumData.EnableSaveIntegrationInformations;
            EnableSaveProcessIntegrations = tenant.MillenniumData.EnableSaveProcessIntegrations;
            EnableExtraPaymentInformation = tenant.MillenniumData.EnableExtraPaymentInformation;
            UrlExtraPaymentInformation = tenant.MillenniumData.UrlExtraPaymentInformation;
            StoreDomainByBrasPag = tenant.MillenniumData.StoreDomainByBrasPag;
            SessionToken = tenant.MillenniumData.SessionToken;
            EnableProductKit = tenant.MillenniumData.EnableProductKit;
            EnableMaskedNSU = tenant.MillenniumData.EnableMaskedNSU;
            ProductIntegrationPrice = tenant.MillenniumData.ProductIntegrationPrice;
            SendPaymentMethod = tenant.MillenniumData.SendPaymentMethod;
            EnableProductDiscount = tenant.MillenniumData.EnableProductDiscount;
            MercadoPago = tenant.MillenniumData.MercadoPago;
            
        }

        public string GetFlagEncomenda()
        {
            return HasMultiLocation ? "R" : null;
        }

        public bool EqualsTo(MillenniumData data)
        {
            return
                Id == data.Id &&
                StoreName == data.StoreName &&
                StoreHandle == data.StoreHandle &&
                ProductIntegrationStatus == data.ProductIntegrationStatus &&
                OrderIntegrationStatus == data.OrderIntegrationStatus &&
                DisableCustomerDocument == data.DisableCustomerDocument &&
                Url == data.Url &&
                VitrineId == data.VitrineId &&
                SplitEnabled == data.SplitEnabled &&
                SaleWithoutStockEnabled == data.SaleWithoutStockEnabled &&
                NameField == data.NameField &&
                DescriptionField == data.DescriptionField &&
                OrderPrefix == data.OrderPrefix &&
                CorDescription == data.CorDescription &&
                CorField == data.CorField &&
                SendDefaultCor == data.SendDefaultCor &&
                TamanhoDescription == data.TamanhoDescription &&
                TamanhoField == data.TamanhoField &&
                SendDefaultTamanho == data.SendDefaultTamanho &&
                EstampaDescription == data.EstampaDescription &&
                EstampaField == data.EstampaField &&
                SendDefaultEstampa == data.SendDefaultEstampa &&
                OperatorType == data.OperatorType &&
                CapitalizeProductName == data.CapitalizeProductName &&
                NameSkuEnabled == data.NameSkuEnabled &&
                NameSkuField == data.NameSkuField &&
                ExcludedProductCharacters.All(x =>
                {
                    var oldData = data.ExcludedProductCharacters[ExcludedProductCharacters.IndexOf(x)];
                    return x == oldData;
                }) &&
                Logins.Count == data.Logins.Count &&
                Logins.All(x =>
                {
                    var loginData = data.Logins[Logins.IndexOf(x)];
                    return
                        x.Login == loginData.Login &&
                        x.Password == loginData.Password;
                }) &&
                ExtraFieldConfigurations.Count == data.ExtraFieldConfigurations.Count &&
                ProductImageIntegration == data.ProductImageIntegration &&
                EnabledStockMto == data.EnabledStockMto &&
                ControlStockByUpdateDate == data.ControlStockByUpdateDate &&
                ControlPriceByUpdateDate == data.ControlPriceByUpdateDate &&
                ControlProductByUpdateDate == data.ControlProductByUpdateDate &&
                NumberOfItensPerAPIQuery == data.NumberOfItensPerAPIQuery &&
                ExtraFieldConfigurations.All(x =>
                {
                    var configurationData = data.ExtraFieldConfigurations[ExtraFieldConfigurations.IndexOf(x)];
                    return
                        x.Key == configurationData.Key &&
                        x.JSPath == configurationData.JSPath;
                }) &&
                LocationMap == data.LocationMap &&
                SkuFieldType == data.SkuFieldType
                && EnabledApprovedTransaction == data.EnabledApprovedTransaction
                && EnableSaveIntegrationInformations == data.EnableSaveIntegrationInformations
                && EnableSaveProcessIntegrations == data.EnableSaveProcessIntegrations
                && EnableExtraPaymentInformation == data.EnableExtraPaymentInformation
                && UrlExtraPaymentInformation == data.UrlExtraPaymentInformation
                && StoreDomainByBrasPag == data.StoreDomainByBrasPag
                && SessionToken == data.SessionToken
                && EnableProductKit == data.EnableProductKit
                && EnableMaskedNSU == data.EnableMaskedNSU
                && ProductIntegrationPrice == data.ProductIntegrationPrice
                && SendPaymentMethod == data.SendPaymentMethod
                && EnableProductDiscount == data.EnableProductDiscount;

        }
    }

    public class MillenniumLogin
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public MillenniumLogin(Entities.Database.TenantData.MillenniumLogin login)
        {
            Login = login.Login;
            Password = login.Password;
        }
    }
    public class MillenniumExtraFieldConfiguration
    {
        public string Key { get; set; }
        public string JSPath { get; set; }

        public MillenniumExtraFieldConfiguration(Entities.Database.TenantData.MillenniumExtraFieldConfiguration configuration)
        {
            Key = configuration.Key;
            JSPath = configuration.JSPath;
        }
    }
}
