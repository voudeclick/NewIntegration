using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.EtapasFaturamento
{
    public class ListarEtapasFaturamentoOmieRequestOutput : PaginatedOmieOutput
    {
        public List<EtapasFaturamentoResult> cadastros { get; set; }
    }
}
