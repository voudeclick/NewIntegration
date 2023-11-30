using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Results;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class OmieDataViewModel : BaseViewModel
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
        public List<OmieListTextViewModel> ExtraNotaFiscalEmails { get; set; }
        public List<OmieListTextViewModel> VariantCaracteristicas { get; set; }
        public List<OmieListTextViewModel> CategoryCaracteristicas { get; set; }
        public List<OmieFreteMappingViewModel> FreteMappings { get; set; }
        public List<OmieCodigoParcelaMappingViewModel> CodigoParcelaMappings { get; set; }
        public bool EnableParcelaUnica { get; set; }
        public NameFieldOmieType NameField { get; set; }
        public string NormalizeProductName { get; set; }



        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(AppKey))
                result.AddError("Operação inválida", "AppKey inválida.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(AppSecret))
                result.AddError("Operação inválida", "AppSecret inválido.", GetType().FullName);

            if (ExtraNotaFiscalEmails?.Any() == true)
                ExtraNotaFiscalEmails.ForEach(x => result.Merge(x.IsValid()));

            if (VariantCaracteristicas?.Any() == true)
                VariantCaracteristicas.ForEach(x => result.Merge(x.IsValid()));

            if (CategoryCaracteristicas?.Any() == true)
                CategoryCaracteristicas.ForEach(x => result.Merge(x.IsValid()));

            if (FreteMappings?.Any() == true)
                FreteMappings.ForEach(x => result.Merge(x.IsValid()));

            return result;
        }

        public OmieDataViewModel()
        {
        }

        public OmieDataViewModel(OmieData entity)
        {
            AppKey = entity.AppKey;
            AppSecret = entity.AppSecret;
            OrderPrefix = entity.OrderPrefix;
            EscapeBodyPipe = entity.EscapeBodyPipe;
            AppendSkuCode = entity.AppendSkuCode;
            CodigoLocalEstoque = entity.CodigoLocalEstoque;
            CodigoCategoria = entity.CodigoCategoria;
            CodigoContaCorrente = entity.CodigoContaCorrente;
            CodigoCenarioImposto = entity.CodigoCenarioImposto;
            CodigoEtapaPaymentConfirmed = entity.CodigoEtapaPaymentConfirmed;
            SendNotaFiscalEmailToClient = entity.SendNotaFiscalEmailToClient;
            ExtraNotaFiscalEmails = entity.GetExtraNotaFiscalEmails()?.Select(x => new OmieListTextViewModel(x)).ToList();
            VariantCaracteristicas = entity.GetVariantCaracteristicas()?.Select(x => new OmieListTextViewModel(x)).ToList();
            CategoryCaracteristicas = entity.GetCategoryCaracteristicas()?.Select(x => new OmieListTextViewModel(x)).ToList();
            FreteMappings = entity.GetFreteMappings()?.Select(x => new OmieFreteMappingViewModel(x)).ToList();
            CodigoParcelaMappings = entity.GetCodigoParcelasMapping()?.Select(x => new OmieCodigoParcelaMappingViewModel(x)).ToList();
            EnableParcelaUnica = entity.ParcelaUnica;
            NameField = entity.NameField;
            NormalizeProductName = entity.NormalizeProductName;

        }

    }

    public class OmieListTextViewModel : BaseViewModel
    {
        public string Text { get; set; }
        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Text))
                result.AddError("Operação inválida", "valor inválido.", GetType().FullName);

            return result;
        }
        public OmieListTextViewModel()
        {
        }

        public OmieListTextViewModel(string value)
        {
            Text = value;
        }
    }

    public class OmieFreteMappingViewModel : BaseViewModel
    {
        public string ShopifyCarrierTitle { get; set; }
        public long OmieCodigoTransportadora { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(ShopifyCarrierTitle))
                result.AddError("Operação inválida", "serviço de entrega shopify inválido.", GetType().FullName);

            if (OmieCodigoTransportadora == 0)
                result.AddError("Operação inválida", "transportador omie inválida.", GetType().FullName);

            return result;
        }
        public OmieFreteMappingViewModel()
        {
        }

        public OmieFreteMappingViewModel(OmieFreteMapping entity)
        {
            ShopifyCarrierTitle = entity.ShopifyCarrierTitle;
            OmieCodigoTransportadora = entity.OmieCodigoTransportadora;
        }
    }

    public class OmieCodigoParcelaMappingViewModel : BaseViewModel
    {
        public string CodigoParcelaOmie { get; set; }
        public int QuantidadeParcela { get; set; }
        public int? QtdeDiasParaPagamento { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(CodigoParcelaOmie))
                result.AddError("Operação inválida", "Codigo parcela inválido.", GetType().FullName);

            return result;
        }
        public OmieCodigoParcelaMappingViewModel()
        {
        }

        public OmieCodigoParcelaMappingViewModel(OmieCodigoParcelaMapping entity)
        {
            CodigoParcelaOmie = entity.CodigoParcelaOmie;
            QuantidadeParcela = entity.QuantidadeParcela;
            QtdeDiasParaPagamento = entity.QtdeDiasParaPagamento;
        }
    }

    public static class OmieDataViewModelExtensions
    {
        public static void UpdateFrom(this OmieData entity, OmieDataViewModel viewModel)
        {
            entity.AppKey = viewModel.AppKey;
            entity.AppSecret = viewModel.AppSecret;
            entity.OrderPrefix = viewModel.OrderPrefix;
            entity.EscapeBodyPipe = viewModel.EscapeBodyPipe;
            entity.AppendSkuCode = viewModel.AppendSkuCode;
            entity.CodigoLocalEstoque = viewModel.CodigoLocalEstoque;
            entity.CodigoCategoria = viewModel.CodigoCategoria;
            entity.CodigoContaCorrente = viewModel.CodigoContaCorrente;
            entity.CodigoCenarioImposto = viewModel.CodigoCenarioImposto;
            entity.CodigoEtapaPaymentConfirmed = viewModel.CodigoEtapaPaymentConfirmed;
            entity.SendNotaFiscalEmailToClient = viewModel.SendNotaFiscalEmailToClient;
            entity.SetExtraNotaFiscalEmails(viewModel.ExtraNotaFiscalEmails.Select(x => x.Text).ToList());
            entity.SetVariantCaracteristicas(viewModel.VariantCaracteristicas.Select(x => x.Text).ToList());
            entity.SetCategoryCaracteristicas(viewModel.CategoryCaracteristicas.Select(x => x.Text).ToList());
            entity.SetFreteMappings(viewModel.FreteMappings?.Select(x => new OmieFreteMapping { ShopifyCarrierTitle = x.ShopifyCarrierTitle, OmieCodigoTransportadora = x.OmieCodigoTransportadora } ).ToList());
            entity.SetCodigoParcelasMapping(viewModel.CodigoParcelaMappings?.Select(x => new OmieCodigoParcelaMapping { CodigoParcelaOmie = x.CodigoParcelaOmie, QuantidadeParcela = x.QuantidadeParcela,
                QtdeDiasParaPagamento = x.QtdeDiasParaPagamento }).ToList());
            entity.ParcelaUnica = viewModel.EnableParcelaUnica;
            entity.NameField = viewModel.NameField;
            entity.NormalizeProductName = viewModel.NormalizeProductName;
        }
    }
}
