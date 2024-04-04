using VDC.Integration.APIClient.Omie.Models.Request.EtapasFaturamento.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.EtapasFaturamento;

namespace VDC.Integration.APIClient.Omie.Models.Request.EtapasFaturamento
{
    public class ListarEtapasFaturamentoOmieRequest : BaseEtapasFaturamentoOmieRequest<ListarEtapasFaturamentoOmieRequestInput, ListarEtapasFaturamentoOmieRequestOutput>
    {
        public override string Method => "ListarEtapasFaturamento";

        public ListarEtapasFaturamentoOmieRequest(ListarEtapasFaturamentoOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
