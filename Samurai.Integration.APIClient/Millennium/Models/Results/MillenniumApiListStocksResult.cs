using Samurai.Integration.Domain.Models.Millennium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiListStocksResult
    {
        public List<Value> value { get; set; }

        public List<Value> GetValues()
        {
            return value.Select(x =>
            {
                var stringValue = System.Text.Json.JsonSerializer.Serialize(x);
                Value value = System.Text.Json.JsonSerializer.Deserialize<Value>(stringValue);
                return value;
            }).ToList();
        }
        public class Value : IFieldSku
        {
            public string sku { get; set; }
            public int saldo { get; set; }
            public int empenho { get; set; }
            public long produto { get; set; }
            public int cor { get; set; }
            public int estampa { get; set; }
            public string tamanho { get; set; }
            public string data_atualizacao { get; set; }
            public string vitrine_produto_sku { get; set; }
            public int reserva_vitrine { get; set; }
            public int reserva_naovitrine { get; set; }
            public int saldo_vitrine { get; set; }
            public int saldo_naovitrine { get; set; }
            public int saldo_compra { get; set; }
            public int estoque_min { get; set; }
            public string id { get; set; }
            public int filial { get; set; }
            public bool kit { get; set; }
            public bool componente_kit { get; set; }
            public string desc_produto { get; set; }
            public int trans_id { get; set; }
            public string permite_pedido_sem_estoque { get; set; }
            public int saldo_vitrine_com_reserva { get; set; }
            public int saldo_vitrine_sem_reserva { get; set; }
            public int vitrine { get; set; }
            public string cod_produto { get; set; }
            public string cod_filial { get; set; }
            public string pre_venda { get; set; }
            public string id_externo { get; set; }

        }

    }
}
