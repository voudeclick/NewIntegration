using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Results;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class NexaasDataViewModel : BaseViewModel
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public long OrganizationId { get; set; }
        public long SaleChannelId { get; set; }
        public long StockId { get; set; }
        public string OrderPrefix { get; set; }
        public bool IsPickupPointEnabled { get; set; }
        public string ServiceNameTemplate { get; set; }
        public string DeliveryTimeTemplate { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Url))
                result.AddError("Operação inválida", "Url inválida.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(Token))
                result.AddError("Operação inválida", "Token inválido.", GetType().FullName);

            return result;
        }

        public NexaasDataViewModel()
        {
        }

        public NexaasDataViewModel(NexaasData entity)
        {
            Url = entity.Url;
            Token = entity.Token;
            OrganizationId = entity.OrganizationId;
            SaleChannelId = entity.SaleChannelId;
            StockId = entity.StockId;
            OrderPrefix = entity.OrderPrefix;
            IsPickupPointEnabled = entity.IsPickupPointEnabled;
            ServiceNameTemplate = entity.ServiceNameTemplate;
            DeliveryTimeTemplate = entity.DeliveryTimeTemplate;
        }

    }

    public static class NexaasDataViewModelExtensions
    {
        public static void UpdateFrom(this NexaasData entity, NexaasDataViewModel viewModel)
        {
            entity.Url = viewModel.Url?.TrimEnd('/');
            entity.Token = viewModel.Token;
            entity.OrganizationId = viewModel.OrganizationId;
            entity.SaleChannelId = viewModel.SaleChannelId;
            entity.StockId = viewModel.StockId;
            entity.OrderPrefix = viewModel.OrderPrefix;
            entity.IsPickupPointEnabled = viewModel.IsPickupPointEnabled;
            if (viewModel.IsPickupPointEnabled)
            {
                entity.ServiceNameTemplate = viewModel.ServiceNameTemplate;
                entity.DeliveryTimeTemplate = viewModel.DeliveryTimeTemplate;
            }
            else
            {
                entity.ServiceNameTemplate = null;
                entity.DeliveryTimeTemplate = null;
            }
        }
    }
}
