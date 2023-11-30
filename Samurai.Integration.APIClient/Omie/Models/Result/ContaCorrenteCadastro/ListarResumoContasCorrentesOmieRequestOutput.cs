using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.ContaCorrenteCadastro
{
    public class ListarResumoContasCorrentesOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ContaCorrenteResult> conta_corrente_lista { get; set; }
    }
}
