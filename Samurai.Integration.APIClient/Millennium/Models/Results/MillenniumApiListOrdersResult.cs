using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiListOrdersResult
    {
        public List<Value> value { get; set; }


        public class Value
        {
            public long pedidov { get; set; }
            public string cod_pedidov { get; set; }
            public string data_emissao { get; set; }
            public string data_entrega { get; set; }
            public string data_atualizacao { get; set; }
            public bool aprovado { get; set; }
            public bool? cancelado { get; set; }
            public decimal? quantidade { get; set; }
            public decimal? total { get; set; }
            public decimal? v_desconto { get; set; }
            public decimal? v_frete { get; set; }
            public decimal? valor_final { get; set; }
            public string nome_transportadora { get; set; }
            public string desc_tipo_frete { get; set; }
            public string cod_tipo_frete { get; set; }
            public bool? efetuado { get; set; }
            public long trans_id { get; set; }
            public string n_pedido_cliente { get; set; }
            public int status { get; set; }
            public string desc_status { get; set; }
            public string data_digitacao { get; set; }
            public string nome_cliente { get; set; }
            public string nome_vendedor { get; set; }
            public string nf { get; set; }
            public List<Desconto> descontos { get; set; }
            public int vitrine { get; set; }
            public string status_workflow_desc { get; set; }
            public List<Retiradas_Autorizadas> retiradas_autorizadas { get; set; }
            public string cod_filial { get; set; }
            public string desc_tipo_pedido { get; set; }
        }

        public class Desconto
        {
            public string descricao { get; set; }
            public int? tipo_desc { get; set; }
            public string id_externo_desconto { get; set; }
            public float desconto { get; set; }
            public string sku { get; set; }
            public string item { get; set; }
            public string barra { get; set; }
            public string nome { get; set; }
        }

        public class Retiradas_Autorizadas
        {
            public string cpf { get; set; }
            public string nome { get; set; }
            public string grau_relacionamento { get; set; }
        }
    }
}
