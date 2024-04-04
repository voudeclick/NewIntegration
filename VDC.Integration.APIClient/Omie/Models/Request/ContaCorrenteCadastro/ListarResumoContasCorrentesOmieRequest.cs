using VDC.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.ContaCorrenteCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro
{
    public class ListarResumoContasCorrentesOmieRequest : BaseContaCorrenteCadastroOmieRequest<ListarResumoContasCorrentesOmieRequestInput, ListarResumoContasCorrentesOmieRequestOutput>
    {
        public override string Method => "ListarResumoContasCorrentes";

        public ListarResumoContasCorrentesOmieRequest(ListarResumoContasCorrentesOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
