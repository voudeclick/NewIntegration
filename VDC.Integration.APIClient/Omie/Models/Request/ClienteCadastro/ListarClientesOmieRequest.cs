using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro
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
