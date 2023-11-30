
using Samurai.Integration.APIClient.Omie.Models.Request.CenarioImposto.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.CenarioImposto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.CenarioImposto
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
