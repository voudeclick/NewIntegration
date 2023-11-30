using Samurai.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.ClienteCadastro;
using Samurai.Integration.APIClient.Shopify.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ClienteCadastro
{
    public class ListarClientesResumidoOmieRequest : BaseClienteCadastroOmieRequest<ListarClientesResumidoOmieRequestInput, ListarClientesResumidoOmieRequestOutput>
    {
        public override string Method => "ListarClientesResumido";

        public ListarClientesResumidoOmieRequest(ListarClientesResumidoOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
