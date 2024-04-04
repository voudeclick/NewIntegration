using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using VDC.Integration.Domain.Enums.Millennium;

namespace VDC.Integration.Domain.Entities.Database.TenantData
{
    public class MillenniumData : BaseEntity
    {
        public string LoginJson { get; set; }
        public string ExtraFieldConfigurationJson { get; set; }
        public string Url { get; set; }
        public string UrlExtraPaymentInformation { get; set; }
        public string ExcludedProductCharactersJson { get; set; }
        public long VitrineId { get; set; }
        public bool SplitEnabled { get; set; }
        public bool SaleWithoutStockEnabled { get; set; }
        public bool ProductIntegrationPrice { get; set; }
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
        public bool EnabledApprovedTransaction { get; set; }
        public SkuFieldType SkuFieldType { get; set; }
        public bool ControlStockByUpdateDate { get; set; }
        public bool ControlPriceByUpdateDate { get; set; }
        public bool ControlProductByUpdateDate { get; set; }
        public int NumberOfItensPerAPIQuery { get; set; }
        public bool EnableSaveProcessIntegrations { get; set; }
        public bool EnableSaveIntegrationInformations { get; set; }
        public bool EnableExtraPaymentInformation { get; set; }
        public bool HasZeroedPriceCase { get; set; }
        public MillenniumOperatorType OperatorType { get; set; }
        public virtual List<MillenniumTransId> TransIds { get; set; }
        public string StoreDomainByBrasPag { get; set; }
        public string SessionToken { get; set; }
        public bool EnableProductKit { get; set; }
        public bool EnableMaskedNSU { get; set; }
        public bool SendPaymentMethod { get; set; }
        public bool EnableProductDiscount { get; set; }

        public virtual MercadoPago MercadoPago { get; set; }


        public void SetExcludedProductCharacters(List<string> value)
        {
            ExcludedProductCharactersJson = JsonSerializer.Serialize(value);
        }
        public List<string> GetExcludedProductCharacters()
        {
            if (string.IsNullOrWhiteSpace(ExcludedProductCharactersJson))
                return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(ExcludedProductCharactersJson);
        }
        public List<MillenniumLogin> GetLogins()
        {

            if (string.IsNullOrWhiteSpace(LoginJson))
                return new List<MillenniumLogin>();
            return JsonSerializer.Deserialize<List<MillenniumLogin>>(LoginJson);
        }

        public void SetLogins(List<MillenniumLogin> value)
        {
            LoginJson = JsonSerializer.Serialize(value);
        }

        public List<MillenniumExtraFieldConfiguration> GetExtraFieldConfigurations()
        {
            if (string.IsNullOrWhiteSpace(ExtraFieldConfigurationJson))
                return new List<MillenniumExtraFieldConfiguration>();
            return JsonSerializer.Deserialize<List<MillenniumExtraFieldConfiguration>>(ExtraFieldConfigurationJson);
        }

        public void SetExtraFieldConfigurations(List<MillenniumExtraFieldConfiguration> value)
        {
            ExtraFieldConfigurationJson = JsonSerializer.Serialize(value);
        }

        public MillenniumTransId GetTransId(TransIdType type)
        {
            return TransIds.FirstOrDefault(x => x.Type == type) ?? new MillenniumTransId { Type = type, Value = 0 };
        }

        public void SetTransId(MillenniumTransId value)
        {
            var currentValue = TransIds.FirstOrDefault(x => x.Type == value.Type);

            if (currentValue == null)
                TransIds.Add(value);
            else
            {
                if (ControlStockByUpdateDate)
                    currentValue.MillenniumLastUpdateDate = currentValue.MillenniumLastUpdateDate;
                else
                    currentValue.Value = value.Value;
            }

        }

        public void HideSensitiveData()
        {
            LoginJson = string.Empty;
        }
    }

    public class MillenniumLogin
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
    public class MillenniumExtraFieldConfiguration
    {
        public string Key { get; set; }
        public string JSPath { get; set; }
    }
}
