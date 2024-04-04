using System.Collections.Generic;
using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;

namespace VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto
{
    public class PedidoVendaResult
    {
        public Cabecalho cabecalho { get; set; }
        public List<Det> det { get; set; }
        public Frete frete { get; set; }
        public InfoCadastro infoCadastro { get; set; }
        public class Cabecalho
        {
            public string bloqueado { get; set; }
            public long codigo_cliente { get; set; }
            public string codigo_parcela { get; set; }
            public long codigo_pedido { get; set; }
            public string codigo_pedido_integracao { get; set; }
            public string data_previsao { get; set; }
            public string etapa { get; set; }
            public string numero_pedido { get; set; }
            public string origem_pedido { get; set; }
            public int qtde_parcelas { get; set; }
            public int quantidade_itens { get; set; }
        }

        public class Frete
        {
            public string codigo_rastreio { get; set; }
            public long codigo_transportadora { get; set; }
        }

        public class InfoCadastro
        {
            public string dInc { get; set; }
            public string hInc { get; set; }
            public string uInc { get; set; }
            public string dAlt { get; set; }
            public string hAlt { get; set; }
            public string uAlt { get; set; }
            public string dCan { get; set; }
            public string hCan { get; set; }
            public string uCan { get; set; }
            public string cancelado { get; set; }
            public string dFat { get; set; }
            public string hFat { get; set; }
            public string uFat { get; set; }
            public string faturado { get; set; }
            public string denegado { get; set; }
            public string autorizado { get; set; }
            public string cImpAPI { get; set; }
        }

        public class Det
        {
            public IncluirPedidoOmieRequestInput.Produto produto { get; set; }
        }
    }
}