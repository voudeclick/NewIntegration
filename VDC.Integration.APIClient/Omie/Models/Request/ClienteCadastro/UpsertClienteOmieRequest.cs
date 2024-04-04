using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro
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
