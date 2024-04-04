using VDC.Integration.APIClient.Omie.Models.Request.ProdutoCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.ProdutoCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.ProdutoCadastro
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
