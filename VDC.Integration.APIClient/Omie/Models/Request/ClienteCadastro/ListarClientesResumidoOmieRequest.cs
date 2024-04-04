using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro
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
