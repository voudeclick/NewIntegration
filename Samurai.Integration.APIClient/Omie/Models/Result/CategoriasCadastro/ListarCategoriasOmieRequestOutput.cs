using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.CategoriaCadastro
{
    public class ListarCategoriasOmieRequestOutput : PaginatedOmieOutput
    {
        public List<CategoriaCadastroResult> categoria_cadastro { get; set; }
    }
}
