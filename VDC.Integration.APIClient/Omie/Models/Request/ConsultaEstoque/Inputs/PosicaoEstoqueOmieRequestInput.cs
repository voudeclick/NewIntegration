﻿using System;

namespace VDC.Integration.APIClient.Omie.Models.Request.ConsultaEstoque.Inputs
{
    public class PosicaoEstoqueOmieRequestInput : BaseOmieInput
    {
        public long? codigo_local_estoque { get; set; }
        public long id_prod { get; set; }
        public string data { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
    }
}
