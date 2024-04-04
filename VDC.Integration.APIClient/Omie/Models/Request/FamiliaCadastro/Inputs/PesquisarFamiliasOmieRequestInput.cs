namespace VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro.Inputs
{
    public class PesquisarFamiliasOmieRequestInput : BaseOmieInput
    {
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
    }
}
