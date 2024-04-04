namespace VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento.Inputs
{
    public class CancelarPedidoVendaOmieRequestInput : BaseOmieInput
    {
        public long? nCodPed { get; set; }
        public string cCodIntPed { get; set; }
    }
}
