using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.ProdutoCadastro
{
    public class ListarProdutosOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ProdutoCadastroResult> produto_servico_cadastro { get; set; }
    }
}
