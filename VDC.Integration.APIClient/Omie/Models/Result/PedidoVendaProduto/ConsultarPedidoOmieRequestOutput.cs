using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto
{
    public class ConsultarPedidoOmieRequestOutput : BaseOmieOutput
    {
        public PedidoVendaResult pedido_venda_produto { get; set; }
    }
}
