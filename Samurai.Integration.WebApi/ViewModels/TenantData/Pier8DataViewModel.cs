using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Results;
using System;
using System.Linq;
using System.Net;


namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class Pier8DataViewModel : BaseViewModel
    {
        public string ApiKey { get; set; }
        public string Token { get; set; }


        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(ApiKey))
                result.AddError("Operação inválida", "ApiKey inválido.", GetType().FullName);

            if (string.IsNullOrWhiteSpace(Token))
                result.AddError("Operação inválida", "Token inválido.", GetType().FullName);


            return result;
        }
        public Pier8DataViewModel()
        {

        }
        public Pier8DataViewModel(Pier8Data data)
        {
            ApiKey = data.ApiKey;
            Token = data.Token;
        }

    }
    public static class Pier8DataViewModelExtensions
    {
        public static void UpdateFrom(this Pier8Data entity, Pier8DataViewModel viewModel)
        {
            entity.UpdateDate = DateTime.UtcNow;
            entity.Token = viewModel.Token;
            entity.ApiKey = viewModel.ApiKey;

        }
    }
}
