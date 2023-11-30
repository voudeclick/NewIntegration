using Samurai.Integration.APIClient.Omie.Models.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.ClienteCadastro
{
    public class ListarClientesOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ClienteCadastroResult> clientes_cadastro { get; set; }
    }
}
