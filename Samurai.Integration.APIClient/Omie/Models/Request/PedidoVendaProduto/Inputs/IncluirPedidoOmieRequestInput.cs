using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs
{
    public class IncluirPedidoOmieRequestInput : BaseOmieInput
    {
        public Cabecalho cabecalho { get; set; }
        public Frete frete { get; set; }
        public InformacoesAdicionais informacoes_adicionais { get; set; }
        public List<Item> det { get; set; }
        public ListaParcelas lista_parcelas { get; set; }

        public class Cabecalho
        {
            public string codigo_pedido_integracao { get; set; }
            public string origem_pedido { get; set; }
            public int quantidade_itens { get; set; }
            public long codigo_cliente { get; set; }
            public string etapa { get; set; }
            public string data_previsao { get; set; }
            public string codigo_parcela { get; set; }
            public int qtde_parcelas { get; set; }
            public long? codigo_cenario_impostos { get; set; }
        }

        public class Frete
        {
            public long codigo_transportadora { get; set; }
            /*
            '0' - Frete por conta do emitente.
            '1' - Frete por conta do destinatário.
            '2' - Frete por conta de terceiros.
            '9' - Sem frete.
           */
            public string modalidade { get; set; }
            public decimal valor_frete { get; set; }
            public string previsao_entrega { get; set; }
            public int? quantidade_volumes { get; set; }

        }

        public class InformacoesAdicionais
        {
            public string codigo_categoria { get; set; }
            public long codigo_conta_corrente { get; set; }
            public string numero_pedido_cliente { get; set; }
            public string consumidor_final { get; set; }
            public string enviar_email { get; set; }
            public string utilizar_emails { get; set; }
            public OutrosDetalhes outros_detalhes { get; set; }
        }

        public class OutrosDetalhes
        {
            public string cCnpjCpfOd { get; set; }
            public string cEnderecoOd { get; set; }
            public string cNumeroOd { get; set; }
            public string cBairroOd { get; set; }
            public string cComplementoOd { get; set; }
            public string cCEPOd { get; set; }
            public string cEstadoOd { get; set; }
            public string cCidadeOd { get; set; }
        }

        public class Item
        {
            public IDE ide { get; set; }
            public Produto produto { get; set; }
            public InformacoesAdicionaisItem inf_adic { get; set; }
        }

        public class IDE
        {
            public string codigo_item_integracao { get; set; }
            public string acao_item { get; set; }
        }

        public class Produto
        {
            public long codigo_produto { get; set; }
            public string codigo { get; set; }
            public string descricao { get; set; }
            public decimal? quantidade { get; set; }
            public decimal valor_unitario { get; set; }
            public decimal? valor_desconto { get; set; }
            public string tipo_desconto { get; set; }
        }

        public class InformacoesAdicionaisItem
        {
            public long? codigo_local_estoque { get; set; }
            public string nao_gerar_financeiro { get; set; }
        }

        public class ListaParcelas
        {
            public List<Parcela> parcela { get; set; }
        }

        public class Parcela
        {
            public int numero_parcela { get; set; }
            public decimal valor { get; set; }
            public decimal percentual { get; set; }
            public string data_vencimento { get; set; }
            public string meio_pagamento { get; set; }
            public string nao_gerar_boleto { get; set; }
        }

    }
}
