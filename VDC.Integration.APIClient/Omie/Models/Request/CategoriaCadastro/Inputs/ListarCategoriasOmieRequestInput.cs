﻿namespace VDC.Integration.APIClient.Omie.Models.Request.CategoriaCadastro.Inputs
{
    public class ListarCategoriasOmieRequestInput : BaseOmieInput
    {
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
    }
}
