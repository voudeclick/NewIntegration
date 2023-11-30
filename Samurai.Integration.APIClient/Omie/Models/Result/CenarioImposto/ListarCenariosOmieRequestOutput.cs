using Samurai.Integration.APIClient.Omie.Models.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.CenarioImposto
{
    public class ListarCenariosOmieRequestOutput : BaseOmieOutput
    {
        public long nPagina { get; set; }
        public long nTotPaginas { get; set; }
        public long nRegistros { get; set; }
        public long nTotRegistros { get; set; }
        public List<CenarioEncontradoResult> cenariosEncontrados { get; set; }
    }
}
