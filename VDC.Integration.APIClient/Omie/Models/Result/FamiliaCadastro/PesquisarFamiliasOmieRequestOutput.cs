using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.FamiliaCadastro
{
    public class PesquisarFamiliasOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ConsultarFamiliaOmieRequestOutput> famCadastro { get; set; }
    }
}
