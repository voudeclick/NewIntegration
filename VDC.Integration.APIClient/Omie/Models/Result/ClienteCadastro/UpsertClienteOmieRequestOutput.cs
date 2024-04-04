using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro
{
    public class UpsertClienteOmieRequestOutput : BaseOmieOutput
    {
        public long codigo_cliente_omie { get; set; }
        public string codigo_cliente_integracao { get; set; }
        public string codigo_status { get; set; }
        public string descricao_status { get; set; }
    }
}
