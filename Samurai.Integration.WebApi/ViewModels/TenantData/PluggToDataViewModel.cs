using Samurai.Integration.Domain.Entities.Database.TenantData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Samurai.Integration.Domain.Results;

namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class PluggToDataViewModel : BaseViewModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AccountUserId { get; set; }
        public string AccountSellerId { get; set; }
        public string OrderStatusMapping { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(ClientId))
                result.AddError("Operação inválida", "ClientId inválida.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(ClientSecret))
                result.AddError("Operação inválida", "ClientSecret inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(Username))
                result.AddError("Operação inválida", "Username inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(Password))
                result.AddError("Operação inválida", "Password inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(AccountUserId))
                result.AddError("Operação inválida", "AccountUserId inválido.", GetType().FullName);

            return result;
        }

        public PluggToDataViewModel()
        {
        }

        public PluggToDataViewModel(PluggToData pluggToData)
        {
            ClientId = pluggToData.ClientId;
            ClientSecret = pluggToData.ClientSecret;
            Username = pluggToData.Username;
            Password = pluggToData.Password;
            AccountUserId = pluggToData.AccountUserId;
            AccountSellerId = pluggToData.AccountSellerId;
            OrderStatusMapping = pluggToData.OrderStatusMapping;
        }
    }

    public static class PluggToDataViewModelExtensions
    {
        public static void UpdateFrom(this PluggToData entity, PluggToDataViewModel viewModel)
        {
            entity.ClientId = viewModel.ClientId;
            entity.ClientSecret = viewModel.ClientSecret;
            entity.Username = viewModel.Username;
            entity.Password = viewModel.Password;
            entity.AccountUserId = viewModel.AccountUserId;
            entity.AccountSellerId = viewModel.AccountSellerId;
            entity.OrderStatusMapping = viewModel.OrderStatusMapping;
        }
    }
}
