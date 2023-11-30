
using Samurai.Integration.APIClient.Omie.Models.Request.LocalEstoque.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.LocalEstoque;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.LocalEstoque
{
    public class ListarLocaisEstoqueOmieRequest : BaseLocalEstoqueOmieRequest<ListarLocaisEstoqueOmieRequestInput, ListarLocaisEstoqueOmieRequestOutput>
    {
        public override string Method => "ListarLocaisEstoque";

        public ListarLocaisEstoqueOmieRequest(ListarLocaisEstoqueOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
