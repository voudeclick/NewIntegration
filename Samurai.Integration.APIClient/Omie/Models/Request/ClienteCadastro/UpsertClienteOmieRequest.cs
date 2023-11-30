using Samurai.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.ClienteCadastro;
using Samurai.Integration.APIClient.Shopify.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ClienteCadastro
{
    public class UpsertClienteOmieRequest : BaseClienteCadastroOmieRequest<UpsertClienteOmieRequestInput, UpsertClienteOmieRequestOutput>
    {
        public override string Method => "UpsertCliente";

        public UpsertClienteOmieRequest(UpsertClienteOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
