using VDC.Integration.APIClient.Omie.Models.Request.CategoriaCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.CategoriasCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.CategoriaCadastro
{
    public class ListarCategoriasOmieRequest : BaseCategoriaCadastroOmieRequest<ListarCategoriasOmieRequestInput, ListarCategoriasOmieRequestOutput>
    {
        public override string Method => "ListarCategorias";

        public ListarCategoriasOmieRequest(ListarCategoriasOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
