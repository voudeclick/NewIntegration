﻿using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result.ConsultaEstoque
{

    public class PosicaoEstoqueOmieRequestOutput : BaseOmieOutput
    {
        public long codigo_local_estoque { get; set; }
        public string codigo_status { get; set; }
        public string descricao_status { get; set; }
        public decimal saldo { get; set; }
        public decimal pendente { get; set; }
        public decimal estoque_minimo { get; set; }
    }
}
