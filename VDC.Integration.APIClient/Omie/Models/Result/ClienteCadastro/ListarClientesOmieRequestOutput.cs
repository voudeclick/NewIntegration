using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro
{
    public class ListarClientesOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ClienteCadastroResult> clientes_cadastro { get; set; }
    }
}
