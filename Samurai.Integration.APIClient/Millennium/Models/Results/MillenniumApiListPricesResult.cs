using Samurai.Integration.Domain.Models.Millennium;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiListPricesResult
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
            public long produto { get; set; }
            public int cor { get; set; }
            public int estampa { get; set; }
            public string tamanho { get; set; }
            public decimal? preco1 { get; set; }
            public decimal? preco2 { get; set; }
            public bool? bloqueado_preco { get; set; }
            public bool? ativo { get; set; }
            public int trans_id { get; set; }
            public decimal? preco_custo { get; set; }
            public int vitrine { get; set; }
            public string cod_produto { get; set; }
            public string data_atualizacao { get; set; }
            public string id_externo { get; set; }

        }
    }
}
