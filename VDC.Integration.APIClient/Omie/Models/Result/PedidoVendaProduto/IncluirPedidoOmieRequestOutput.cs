using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto
{
    public class IncluirPedidoOmieRequestOutput : BaseOmieOutput
    {
        public long codigo_pedido { get; set; }
        public string codigo_pedido_integracao { get; set; }
        public string codigo_status { get; set; }
        public string descricao_status { get; set; }
        public string numero_pedido { get; set; }
    }
}
