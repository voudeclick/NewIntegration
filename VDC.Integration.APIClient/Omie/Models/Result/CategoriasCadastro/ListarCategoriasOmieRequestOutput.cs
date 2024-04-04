using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.CategoriasCadastro
{
    public class ListarCategoriasOmieRequestOutput : PaginatedOmieOutput
    {
        public List<CategoriaCadastroResult> categoria_cadastro { get; set; }
    }
}
