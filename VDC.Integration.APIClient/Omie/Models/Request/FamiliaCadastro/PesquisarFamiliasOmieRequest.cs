using VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.FamiliaCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro
{
    public class PesquisarFamiliasOmieRequest : BaseFamiliaCadastroOmieRequest<PesquisarFamiliasOmieRequestInput, PesquisarFamiliasOmieRequestOutput>
    {
        public override string Method => "PesquisarFamilias";

        public PesquisarFamiliasOmieRequest(PesquisarFamiliasOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
