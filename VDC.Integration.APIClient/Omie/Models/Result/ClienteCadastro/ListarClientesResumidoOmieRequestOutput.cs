using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro
{
    public class ListarClientesResumidoOmieRequestOutput : PaginatedOmieOutput
    {
        public List<ClienteCadastroResumoResult> clientes_cadastro_resumido { get; set; }
    }
}
