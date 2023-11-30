
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Bling;

namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class BlingDataViewModel
    {
        public string ApiBaseUrl { get; set; }
        public string APIKey { get; set; }
        public string OrderPrefix { get; set; }
        public string OrderStatusMapping { get; set; }
        public string CategoriaId { get; set; }
        public OrderFieldBlingType OrderField { get; set; }
        public int? OrderStatusId { get; set; }


        public BlingDataViewModel()
        {
        }

        public BlingDataViewModel(BlingData blingData)
        {
            ApiBaseUrl = blingData.ApiBaseUrl;
            APIKey = blingData.APIKey;
            OrderPrefix = blingData.OrderPrefix;
            OrderStatusMapping = blingData.OrderStatusMapping;
            CategoriaId = blingData.CategoriaId;
            OrderField = blingData.OrderField;
            OrderStatusId = blingData.OrderStatusId;
        }
    }

    public static class BlingDataViewModelExtensions
    {
        public static void UpdateFrom(this BlingData entity, BlingDataViewModel viewModel)
        {
            entity.ApiBaseUrl = viewModel.ApiBaseUrl;
            entity.APIKey = viewModel.APIKey;
            entity.OrderPrefix = viewModel.OrderPrefix;
            entity.OrderStatusMapping = viewModel.OrderStatusMapping;
            entity.CategoriaId = viewModel.CategoriaId;
            entity.OrderField = viewModel.OrderField;
            entity.OrderStatusId = viewModel.OrderStatusId;
        }
    }
}
