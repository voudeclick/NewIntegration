using Samurai.Integration.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class OmieData : BaseEntity
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string OrderPrefix { get; set; }
        public bool EscapeBodyPipe { get; set; }
        public bool AppendSkuCode { get; set; }
        public long? CodigoLocalEstoque { get; set; }
        public string CodigoCategoria { get; set; }
        public long? CodigoContaCorrente { get; set; }
        public long? CodigoCenarioImposto { get; set; }
        public string CodigoEtapaPaymentConfirmed { get; set; }
        public bool SendNotaFiscalEmailToClient { get; set; }
        public string ExtraNotaFiscalEmailsJson { get; set; }
        public string CodigoParcelaMappingJson { get; set; }
        public bool ParcelaUnica { get; set; }
        public bool NaoGerarFinanceiro { get; set; }
        public NameFieldOmieType NameField { get; set; }
        public bool CodigoParcelaFixa { get; set; }
        public string NormalizeProductName { get; set; }


        public List<OmieCodigoParcelaMapping> GetCodigoParcelasMapping()
        {
            if (string.IsNullOrWhiteSpace(CodigoParcelaMappingJson))
                return new List<OmieCodigoParcelaMapping>();
            return JsonSerializer.Deserialize<List<OmieCodigoParcelaMapping>>(CodigoParcelaMappingJson);
        }
        public void SetCodigoParcelasMapping(List<OmieCodigoParcelaMapping> value)
        {
            CodigoParcelaMappingJson = JsonSerializer.Serialize(value);
        }
        public List<string> GetExtraNotaFiscalEmails()
        {
            if (string.IsNullOrWhiteSpace(ExtraNotaFiscalEmailsJson))
                return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(ExtraNotaFiscalEmailsJson);
        }
        public void SetExtraNotaFiscalEmails(List<string> value)
        {
            ExtraNotaFiscalEmailsJson = JsonSerializer.Serialize(value);
        }

        public string VariantCaracteristicasJson { get; set; }
        public List<string> GetVariantCaracteristicas()
        {
            if (string.IsNullOrWhiteSpace(VariantCaracteristicasJson))
                return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(VariantCaracteristicasJson);
        }
        public void SetVariantCaracteristicas(List<string> value)
        {
            VariantCaracteristicasJson = JsonSerializer.Serialize(value);
        }

        public string CategoryCaracteristicasJson { get; set; }
        public List<string> GetCategoryCaracteristicas()
        {
            if (string.IsNullOrWhiteSpace(CategoryCaracteristicasJson))
                return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(CategoryCaracteristicasJson);
        }
        public void SetCategoryCaracteristicas(List<string> value)
        {
            CategoryCaracteristicasJson = JsonSerializer.Serialize(value);
        }

        public string FreteMappingJson { get; set; }
        public List<OmieFreteMapping> GetFreteMappings()
        {
            if (string.IsNullOrWhiteSpace(FreteMappingJson))
                return new List<OmieFreteMapping>();
            return JsonSerializer.Deserialize<List<OmieFreteMapping>>(FreteMappingJson);
        }
        public void SetFreteMappings(List<OmieFreteMapping> value)
        {
            FreteMappingJson = JsonSerializer.Serialize(value);
        }

        public void HideSensitiveData()
        {
            AppKey = string.Empty;
            AppSecret = string.Empty;
        }
    }

    public class OmieFreteMapping
    {
        public string ShopifyCarrierTitle { get; set; }
        public long OmieCodigoTransportadora { get; set; }
    }
    public class OmieCodigoParcelaMapping 
    {
        public string CodigoParcelaOmie { get; set; }
        public int QuantidadeParcela { get; set; }
        public int? QtdeDiasParaPagamento { get; set; }

    }
}