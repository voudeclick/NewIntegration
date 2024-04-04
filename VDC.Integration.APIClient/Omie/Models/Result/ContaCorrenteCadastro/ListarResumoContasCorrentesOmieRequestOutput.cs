using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.ContaCorrenteCadastro
{
    public class ListarResumoContasCorrentesOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ContaCorrenteResult> conta_corrente_lista { get; set; }
    }
}
