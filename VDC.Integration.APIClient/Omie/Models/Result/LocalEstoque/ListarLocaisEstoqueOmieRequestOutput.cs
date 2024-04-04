using System.Collections.Generic;
using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result.LocalEstoque
{

    public class ListarLocaisEstoqueOmieRequestOutput : BaseOmieOutput
    {
        public long nPagina { get; set; }
        public long nTotPaginas { get; set; }
        public long nRegistros { get; set; }
        public long nTotRegistros { get; set; }
        public List<LocalEncontradoResult> locaisEncontrados { get; set; }
    }
}
