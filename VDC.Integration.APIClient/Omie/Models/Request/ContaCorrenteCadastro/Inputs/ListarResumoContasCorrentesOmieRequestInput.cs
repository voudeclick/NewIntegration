﻿namespace VDC.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro.Inputs
{
    public class ListarResumoContasCorrentesOmieRequestInput : BaseOmieInput
    {
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
    }
}
