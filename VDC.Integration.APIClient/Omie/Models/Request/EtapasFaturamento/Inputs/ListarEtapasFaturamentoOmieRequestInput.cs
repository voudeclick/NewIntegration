namespace VDC.Integration.APIClient.Omie.Models.Request.EtapasFaturamento.Inputs
{
    public class ListarEtapasFaturamentoOmieRequestInput : BaseOmieInput
    {
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
    }
}
