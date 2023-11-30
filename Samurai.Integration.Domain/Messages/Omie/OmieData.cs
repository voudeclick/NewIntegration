using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Queues;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.Domain.Messages.Omie
{
    public class OmieData : IBaseQueue
    {
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public bool DisableCustomerDocument { get; set; }
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
        public List<string> ExtraNotaFiscalEmails { get; set; }
        public List<string> VariantCaracteristicas { get; set; }
        public List<string> CategoryCaracteristicas { get; set; }
        public List<OmieFreteMapping> FreteMapping { get; set; }
        public List<OmieCodigoParcelaMapping> CodigoParcelaMapping { get; set; }
        public bool ParcelaUnica { get; set; }
        public bool NaoGerarFinanceiro { get; set; }
        public NameFieldOmieType NameField { get; set; }
        public bool CodigoParcelaFixa { get; set; }
        public string NormalizeProductName { get; set; }




        public OmieData(Tenant tenant)
        {
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;
            DisableCustomerDocument = tenant.DisableCustomerDocument;
            AppKey = tenant.OmieData.AppKey;
            AppSecret = tenant.OmieData.AppSecret;
            OrderPrefix = tenant.OmieData.OrderPrefix;
            CodigoLocalEstoque = tenant.OmieData.CodigoLocalEstoque;
            EscapeBodyPipe = tenant.OmieData.EscapeBodyPipe;
            AppendSkuCode = tenant.OmieData.AppendSkuCode;
            CodigoCategoria = tenant.OmieData.CodigoCategoria;
            CodigoContaCorrente = tenant.OmieData.CodigoContaCorrente;
            CodigoCenarioImposto = tenant.OmieData.CodigoCenarioImposto;
            CodigoEtapaPaymentConfirmed = tenant.OmieData.CodigoEtapaPaymentConfirmed;
            SendNotaFiscalEmailToClient = tenant.OmieData.SendNotaFiscalEmailToClient;
            ExtraNotaFiscalEmails = tenant.OmieData.GetExtraNotaFiscalEmails();
            VariantCaracteristicas = tenant.OmieData.GetVariantCaracteristicas();
            CategoryCaracteristicas = tenant.OmieData.GetCategoryCaracteristicas();
            FreteMapping = tenant.OmieData.GetFreteMappings()?.Select(x => new OmieFreteMapping(x)).ToList();
            CodigoParcelaMapping = tenant.OmieData.GetCodigoParcelasMapping()?.Select(x => new OmieCodigoParcelaMapping(x)).ToList();
            ParcelaUnica = tenant.OmieData.ParcelaUnica;
            NaoGerarFinanceiro = tenant.OmieData.NaoGerarFinanceiro;
            NameField = tenant.OmieData.NameField;
            CodigoParcelaFixa = tenant.OmieData.CodigoParcelaFixa;
            NormalizeProductName = tenant.OmieData.NormalizeProductName;

        }

        public bool EqualsTo(OmieData data)
        {
            return
                Id == data.Id &&
                StoreName == data.StoreName &&
                StoreHandle == data.StoreHandle &&
                ProductIntegrationStatus == data.ProductIntegrationStatus &&
                OrderIntegrationStatus == data.OrderIntegrationStatus &&
                DisableCustomerDocument == data.DisableCustomerDocument &&
                AppKey == data.AppKey &&
                AppSecret == data.AppSecret &&
                OrderPrefix == data.OrderPrefix &&
                EscapeBodyPipe == data.EscapeBodyPipe &&
                AppendSkuCode == data.AppendSkuCode &&
                CodigoLocalEstoque == data.CodigoLocalEstoque &&
                CodigoCategoria == data.CodigoCategoria &&
                CodigoCategoria == data.CodigoCategoria &&
                CodigoContaCorrente == data.CodigoContaCorrente &&
                CodigoCenarioImposto == data.CodigoCenarioImposto &&
                CodigoEtapaPaymentConfirmed == data.CodigoEtapaPaymentConfirmed &&
                SendNotaFiscalEmailToClient == data.SendNotaFiscalEmailToClient &&
                ExtraNotaFiscalEmails.Count == data.ExtraNotaFiscalEmails.Count &&
                ExtraNotaFiscalEmails.All(x =>
                {
                    var oldData = data.ExtraNotaFiscalEmails[ExtraNotaFiscalEmails.IndexOf(x)];
                    return x == oldData;
                }) &&
                VariantCaracteristicas.Count == data.VariantCaracteristicas.Count &&
                VariantCaracteristicas.All(x =>
                {
                    var oldData = data.VariantCaracteristicas[VariantCaracteristicas.IndexOf(x)];
                    return x == oldData;
                }) &&
                CategoryCaracteristicas.Count == data.CategoryCaracteristicas.Count &&
                CategoryCaracteristicas.All(x =>
                {
                    var oldData = data.CategoryCaracteristicas[CategoryCaracteristicas.IndexOf(x)];
                    return x == oldData;
                }) &&
                ParcelaUnica == data.ParcelaUnica &&
                NormalizeProductName == data.NormalizeProductName &&
                NameField == data.NameField;
                //FreteMapping.Count == data.FreteMapping.Count &&
                //FreteMapping.All(x =>
                //{
                //    var oldData = data.FreteMapping[FreteMapping.IndexOf(x)];
                //    return x.ShopifyCarrierTitle == oldData.ShopifyCarrierTitle &&
                //           x.OmieCodigoTransportadora == oldData.OmieCodigoTransportadora;
                //});
        }
        public int? GetQtdDiasVencimentoParcela(int qtdParcelas)
            => CodigoParcelaMapping.Where(x => x.QuantidadeParcela == qtdParcelas).FirstOrDefault()?.QtdeDiasParaPagamento;
        
    }

    public class OmieFreteMapping
    {
        public string ShopifyCarrierTitle { get; set; }
        public long OmieCodigoTransportadora { get; set; }

        public OmieFreteMapping(Entities.Database.TenantData.OmieFreteMapping mapping)
        {
            ShopifyCarrierTitle = mapping.ShopifyCarrierTitle;
            OmieCodigoTransportadora = mapping.OmieCodigoTransportadora;
        }
    }

    public class OmieCodigoParcelaMapping
    {
        public string CodigoParcela { get; set; }
        public int QuantidadeParcela { get; set; }
        public int? QtdeDiasParaPagamento { get; set; }

        public OmieCodigoParcelaMapping(Entities.Database.TenantData.OmieCodigoParcelaMapping mapping)
        {
            CodigoParcela = mapping.CodigoParcelaOmie;
            QuantidadeParcela = mapping.QuantidadeParcela;
            QtdeDiasParaPagamento = mapping.QtdeDiasParaPagamento;
        }
    }
}
