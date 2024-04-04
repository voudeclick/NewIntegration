using VDC.Integration.APIClient.Omie.Models.Request.LocalEstoque.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.LocalEstoque;

namespace VDC.Integration.APIClient.Omie.Models.Request.LocalEstoque
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
