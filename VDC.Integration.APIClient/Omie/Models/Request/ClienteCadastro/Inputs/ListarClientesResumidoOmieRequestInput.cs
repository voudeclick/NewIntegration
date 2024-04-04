namespace VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs
{
    public class ListarClientesResumidoOmieRequestInput : BaseOmieInput
    {
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
        public ClientesFiltro clientesFiltro { get; set; }
    }
}
