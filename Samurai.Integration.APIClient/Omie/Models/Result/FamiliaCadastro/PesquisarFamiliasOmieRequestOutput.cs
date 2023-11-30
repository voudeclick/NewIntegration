using Samurai.Integration.APIClient.Omie.Models.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.FamiliaCadastro
{
    public class PesquisarFamiliasOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ConsultarFamiliaOmieRequestOutput> famCadastro { get; set; }
    }
}
