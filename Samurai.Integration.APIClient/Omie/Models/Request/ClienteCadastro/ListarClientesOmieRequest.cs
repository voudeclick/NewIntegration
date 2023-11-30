using Samurai.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.ClienteCadastro;
using Samurai.Integration.APIClient.Shopify.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ClienteCadastro
{
    public class ListarClientesOmieRequest : BaseClienteCadastroOmieRequest<ListarClientesOmieRequestInput, ListarClientesOmieRequestOutput>
    {
        public override string Method => "ListarClientes";

        public ListarClientesOmieRequest(ListarClientesOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
