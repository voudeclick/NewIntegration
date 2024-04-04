using VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.FamiliaCadastro;

namespace VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro
{
    public class ConsultarFamiliaOmieRequest : BaseFamiliaCadastroOmieRequest<ConsultarFamiliaOmieRequestInput, ConsultarFamiliaOmieRequestOutput>
    {
        public override string Method => "ConsultarFamilia";

        public ConsultarFamiliaOmieRequest(ConsultarFamiliaOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
