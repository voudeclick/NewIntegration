using Samurai.Integration.APIClient.Omie.Models.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.EtapasFaturamento
{
    public class ListarEtapasFaturamentoOmieRequestOutput : PaginatedOmieOutput
    {
        public List<EtapasFaturamentoResult> cadastros { get; set; }
    }
}
