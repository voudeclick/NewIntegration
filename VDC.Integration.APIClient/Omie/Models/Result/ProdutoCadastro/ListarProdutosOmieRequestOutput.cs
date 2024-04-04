using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.ProdutoCadastro
{
    public class ListarProdutosOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ProdutoCadastroResult> produto_servico_cadastro { get; set; }
    }
}
