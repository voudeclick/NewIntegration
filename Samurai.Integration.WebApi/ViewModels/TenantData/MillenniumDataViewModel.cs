using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Results;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class MillenniumDataViewModel : BaseViewModel
    {
        public List<MillenniumLoginViewModel> Logins { get; set; }
        public List<MillenniumExtraFieldConfigurationViewModel> ExtraFieldConfigurations { get; set; }
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
        public bool CapitalizeProductName { get; set; }
        public bool NameSkuEnabled { get; set; }
        public string NameSkuField { get; set; }
        public bool EnabledStockMto { get; set; }
        public bool ControlStockByUpdateDate { get; set; }
        public bool ControlPriceByUpdateDate { get; set; }
        public bool ControlProductByUpdateDate { get; set; }
        public int NumberOfItensPerAPIQuery { get; set; }
        public bool EnabledApprovedTransaction { get; set; }
        public MillenniumOperatorType OperatorType { get; set; }
        public List<MilleniumListTextViewModel> ExcludedProductCharacters { get; set; }
        public SkuFieldType SkuFieldType { get; set; }
        public bool EnableSaveIntegrationInformations { get; set; }
        public bool EnableSaveProcessIntegrations { get; set; }
        public bool EnableExtraPaymentInformation { get; set; }
        public bool HasZeroedPriceCase { get; set; }
        public string StoreDomainByBrasPag { get; set; }
        public string SessionToken { get; set; }
        public string TransIdProd { get; set; }
        public string UpadateDateProd { get; set; }
        public string TransIdPrice { get; set; }
        public string UpadateDatePrice { get; set; }
        public string TransIdStock { get; set; }
        public string UpadateDateStock { get; set; }
        public bool EnableProductKit { get; set; }
        public bool EnableMaskedNSU { get; set; }
        public bool SendPaymentMethod { get; set; }
        public bool ProductIntegrationPrice { get; set; }
        public bool EnableProductDiscount { get; set; }

        public class MilleniumListTextViewModel : BaseViewModel
        {
            public string Text { get; set; }
            public override Result IsValid()
            {
                var result = new Result { StatusCode = HttpStatusCode.OK };

                if (string.IsNullOrWhiteSpace(Text))
                    result.AddError("Operação inválida", "valor inválido.", GetType().FullName);

                return result;
            }
            public MilleniumListTextViewModel()
            {
            }

            public MilleniumListTextViewModel(string value)
            {
                Text = value;
            }
        }


        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Url))
                result.AddError("Operação inválida", "Url inválida.", GetType().FullName);

            if (VitrineId < 0)
                result.AddError("Operação inválida", "vitrine inválida.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(NameField))
                result.AddError("Operação inválida", "campo nome inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(DescriptionField))
                result.AddError("Operação inválida", "campo descrição inválido.", GetType().FullName);

            if (NameSkuEnabled == true && string.IsNullOrWhiteSpace(NameSkuField))
                result.AddError("Operação inválida", "campo nome SKU inválido.", GetType().FullName);


            if (Logins?.Any() != true)
                result.AddError("Operação inválida", "login inválido.", GetType().FullName);
            else
                Logins.ForEach(x => result.Merge(x.IsValid()));

            if (ExtraFieldConfigurations?.Any() == true)
                ExtraFieldConfigurations.ForEach(x => result.Merge(x.IsValid()));

            return result;
        }

        public MillenniumDataViewModel()
        {
        }

        public MillenniumDataViewModel(MillenniumData entity)
        {
            Logins = entity.GetLogins()?.Select(x => new MillenniumLoginViewModel(x)).ToList();
            ExtraFieldConfigurations = entity.GetExtraFieldConfigurations()?.Select(x => new MillenniumExtraFieldConfigurationViewModel(x)).ToList();
            Url = entity.Url;
            VitrineId = entity.VitrineId;
            SplitEnabled = entity.SplitEnabled;
            SaleWithoutStockEnabled = entity.SaleWithoutStockEnabled;
            NameField = entity.NameField;
            DescriptionField = entity.DescriptionField;
            OrderPrefix = entity.OrderPrefix;
            CorDescription = entity.CorDescription;
            CorField = entity.CorField;
            SendDefaultCor = entity.SendDefaultCor;
            TamanhoDescription = entity.TamanhoDescription;
            TamanhoField = entity.TamanhoField;
            SendDefaultTamanho = entity.SendDefaultTamanho;
            EstampaDescription = entity.EstampaDescription;
            EstampaField = entity.EstampaField;
            SendDefaultEstampa = entity.SendDefaultEstampa;
            OperatorType = entity.OperatorType;
            ExcludedProductCharacters = entity.GetExcludedProductCharacters()?.Select(x => new MilleniumListTextViewModel(x)).ToList();
            CapitalizeProductName = entity.CapitalizeProductName;
            NameSkuField = entity.NameSkuField;
            NameSkuEnabled = entity.NameSkuEnabled;
            EnabledStockMto = entity.EnabledStockMto;
            SkuFieldType = entity.SkuFieldType;
            ControlStockByUpdateDate = entity.ControlStockByUpdateDate;
            ControlPriceByUpdateDate = entity.ControlPriceByUpdateDate;
            ControlProductByUpdateDate = entity.ControlProductByUpdateDate;
            NumberOfItensPerAPIQuery = entity.NumberOfItensPerAPIQuery;
            EnabledApprovedTransaction = entity.EnabledApprovedTransaction;
            EnableSaveIntegrationInformations = entity.EnableSaveIntegrationInformations;
            HasZeroedPriceCase = entity.HasZeroedPriceCase;
            EnableSaveProcessIntegrations = entity.EnableSaveProcessIntegrations;
            EnableExtraPaymentInformation = entity.EnableExtraPaymentInformation;
            UrlExtraPaymentInformation = entity.UrlExtraPaymentInformation;
            StoreDomainByBrasPag = entity.StoreDomainByBrasPag;
            SessionToken = entity.SessionToken;
            EnableProductKit = entity.EnableProductKit;
            EnableMaskedNSU = entity.EnableMaskedNSU;
            ProductIntegrationPrice = entity.ProductIntegrationPrice;
            SendPaymentMethod = entity.SendPaymentMethod;
            EnableProductDiscount = entity.EnableProductDiscount;   

            TransIdProd = entity.GetTransId(TransIdType.ListaVitrine)?.Value.ToString();
            UpadateDateProd = entity.GetTransId(TransIdType.ListaVitrine)?.MillenniumLastUpdateDate?.ToString("dd/MM/yyyy");
            TransIdPrice = entity.GetTransId(TransIdType.PrecoDeTabela)?.Value.ToString();
            UpadateDatePrice = entity.GetTransId(TransIdType.PrecoDeTabela)?.MillenniumLastUpdateDate?.ToString("dd/MM/yyyy");
            TransIdStock = entity.GetTransId(TransIdType.SaldoDeEstoque)?.Value.ToString();
            UpadateDateStock = entity.GetTransId(TransIdType.SaldoDeEstoque)?.MillenniumLastUpdateDate?.ToString("dd/MM/yyyy");
        }

    }

    public class MillenniumLoginViewModel : BaseViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Login))
                result.AddError("Operação inválida", "login inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(Password))
                result.AddError("Operação inválida", "senha inválida.", GetType().FullName);

            return result;
        }
        public MillenniumLoginViewModel()
        {
        }

        public MillenniumLoginViewModel(MillenniumLogin entity)
        {
            Login = entity.Login;
            Password = entity.Password;
        }
    }

    public class MillenniumExtraFieldConfigurationViewModel : BaseViewModel
    {
        public string Key { get; set; }
        public string JSPath { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Key))
                result.AddError("Operação inválida", "chave inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(JSPath))
                result.AddError("Operação inválida", "JSPath inválido.", GetType().FullName);

            return result;
        }
        public MillenniumExtraFieldConfigurationViewModel()
        {
        }

        public MillenniumExtraFieldConfigurationViewModel(MillenniumExtraFieldConfiguration entity)
        {
            Key = entity.Key;
            JSPath = entity.JSPath;
        }
    }

    public static class MillenniumDataViewModelExtensions
    {
        public static void UpdateFrom(this MillenniumData entity, MillenniumDataViewModel viewModel)
        {
            entity.SetLogins(viewModel.Logins.Select(x => new MillenniumLogin { Login = x.Login, Password = x.Password }).ToList());
            entity.SetExtraFieldConfigurations(viewModel.ExtraFieldConfigurations.Select(x => new MillenniumExtraFieldConfiguration { Key = x.Key, JSPath = x.JSPath }).ToList());
            entity.Url = viewModel.Url?.TrimEnd('/');
            entity.UrlExtraPaymentInformation = viewModel.UrlExtraPaymentInformation?.TrimEnd('/');
            entity.VitrineId = viewModel.VitrineId;
            entity.SplitEnabled = viewModel.SplitEnabled;
            entity.SaleWithoutStockEnabled = viewModel.SaleWithoutStockEnabled;
            entity.NameField = viewModel.NameField;
            entity.DescriptionField = viewModel.DescriptionField;
            entity.OrderPrefix = viewModel.OrderPrefix;
            entity.CorDescription = viewModel.CorDescription;
            entity.CorField = viewModel.CorField;
            entity.SendDefaultCor = viewModel.SendDefaultCor;
            entity.TamanhoDescription = viewModel.TamanhoDescription;
            entity.TamanhoField = viewModel.TamanhoField;
            entity.SendDefaultTamanho = viewModel.SendDefaultTamanho;
            entity.EstampaDescription = viewModel.EstampaDescription;
            entity.EstampaField = viewModel.EstampaField;
            entity.SendDefaultEstampa = viewModel.SendDefaultEstampa;
            entity.OperatorType = viewModel.OperatorType;
            entity.CapitalizeProductName = viewModel.CapitalizeProductName;
            entity.SetExcludedProductCharacters(viewModel.ExcludedProductCharacters.Select(x => x.Text).ToList());
            entity.NameSkuEnabled = viewModel.NameSkuEnabled;
            entity.NameSkuField = viewModel.NameSkuField;
            entity.EnabledStockMto = viewModel.EnabledStockMto;
            entity.SkuFieldType = viewModel.SkuFieldType;
            entity.ControlStockByUpdateDate = viewModel.ControlStockByUpdateDate;
            entity.ControlPriceByUpdateDate = viewModel.ControlPriceByUpdateDate;
            entity.ControlProductByUpdateDate = viewModel.ControlProductByUpdateDate;
            entity.NumberOfItensPerAPIQuery = viewModel.NumberOfItensPerAPIQuery;
            entity.EnabledApprovedTransaction = viewModel.EnabledApprovedTransaction;
            entity.EnableSaveIntegrationInformations = viewModel.EnableSaveIntegrationInformations;
            entity.EnableExtraPaymentInformation = viewModel.EnableExtraPaymentInformation;
            entity.EnableSaveProcessIntegrations = viewModel.EnableSaveProcessIntegrations;
            entity.HasZeroedPriceCase = viewModel.HasZeroedPriceCase;
            entity.StoreDomainByBrasPag = viewModel.StoreDomainByBrasPag;
            entity.SessionToken = viewModel.SessionToken;
            entity.EnableMaskedNSU = viewModel.EnableMaskedNSU;
            entity.ProductIntegrationPrice = viewModel.ProductIntegrationPrice;
            entity.SendPaymentMethod = viewModel.SendPaymentMethod;
            entity.EnableProductDiscount = viewModel.EnableProductDiscount;
        }
    }
}
