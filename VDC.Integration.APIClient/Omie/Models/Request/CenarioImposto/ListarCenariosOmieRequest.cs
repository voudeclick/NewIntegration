using VDC.Integration.APIClient.Omie.Models.Request.CenarioImposto.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.CenarioImposto;

namespace VDC.Integration.APIClient.Omie.Models.Request.CenarioImposto
{
    public class ListarCenariosOmieRequest : BaseCenarioImpostoOmieRequest<ListarCenariosOmieRequestInput, ListarCenariosOmieRequestOutput>
    {
        public override string Method => "ListarCenarios";

        public ListarCenariosOmieRequest(ListarCenariosOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
