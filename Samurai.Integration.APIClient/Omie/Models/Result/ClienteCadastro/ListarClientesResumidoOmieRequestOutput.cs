using Samurai.Integration.APIClient.Omie.Models.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.ClienteCadastro
{
    public class ListarClientesResumidoOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ClienteCadastroResumoResult> clientes_cadastro_resumido { get; set; }
    }
}
