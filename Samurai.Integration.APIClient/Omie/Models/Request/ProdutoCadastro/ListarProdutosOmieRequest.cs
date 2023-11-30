
using Samurai.Integration.APIClient.Omie.Models.Request.ProdutoCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.ProdutoCadastro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ProdutoCadastro
{
    public class ListarProdutosOmieRequest : BaseProdutoCadastroOmieRequest<ListarProdutosOmieRequestInput, ListarProdutosOmieRequestOutput>
    {
        public override string Method => "ListarProdutos";

        public ListarProdutosOmieRequest(ListarProdutosOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
