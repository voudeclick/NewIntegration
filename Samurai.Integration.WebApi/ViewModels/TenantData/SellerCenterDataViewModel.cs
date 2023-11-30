using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Results;
using System.Net;

namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class SellerCenterDataViewModel : BaseViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TenantId { get; set; }
        public string SellerId { get; set; }
        public string SellerWarehouseId { get; set; }
        public bool SellWithoutStock { get; set; }
        public bool DisableUpdateProduct { get; set; }

        public OrderIntegrationStatusEnum OrderIntegrationStatus { get; set; }

        public SellerCenterDataViewModel()
        {

        }
        public SellerCenterDataViewModel(SellerCenterData entity)
        {
            Username = entity.Username;
            Password = entity.Password;
            TenantId = entity.TenantId;
            SellWithoutStock = entity.SellWithoutStock;
            SellerId = entity.SellerId;
            SellerWarehouseId = entity.SellerWarehouseId;
            OrderIntegrationStatus = entity.OrderIntegrationStatus;
            DisableUpdateProduct = entity.DisableUpdateProduct;

        }
        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Username))
                result.AddError("Operação inválida", "usuarios inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(Password))
                result.AddError("Operação inválida", "senha inválida.", GetType().FullName);

            return result;
        }
    }
    public static class SellerCenterDataViewModelExtensions
    {
        public static void UpdateFrom(this SellerCenterData entity, SellerCenterDataViewModel viewModel)
        {
            entity.Username = viewModel.Username;
            entity.Password = viewModel.Password;
            entity.TenantId = viewModel.TenantId;
            entity.SellerId = viewModel.SellerId;
            entity.SellerWarehouseId = viewModel.SellerWarehouseId;
            entity.SellWithoutStock = viewModel.SellWithoutStock;
            entity.DisableUpdateProduct = viewModel.DisableUpdateProduct;
            entity.OrderIntegrationStatus = viewModel.OrderIntegrationStatus;
        }
    }
}
