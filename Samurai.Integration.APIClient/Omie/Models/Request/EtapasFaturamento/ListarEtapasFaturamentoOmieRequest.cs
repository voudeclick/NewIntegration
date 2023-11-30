
using Samurai.Integration.APIClient.Omie.Models.Request.EtapasFaturamento.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.EtapasFaturamento;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.EtapasFaturamento
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
