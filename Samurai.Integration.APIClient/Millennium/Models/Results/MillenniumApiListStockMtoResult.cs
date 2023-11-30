using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiListStockMtoResult
    {
      
        public List<Value> value { get; set; }

        public class Value
        {
            public long produto { get; set; }
            public string cod_produto { get; set; }
            public string descricao { get; set; }
            public long trans_id { get; set; }
            public List<SkuInfo> sku { get; set; }


        }
        public class SkuInfo 
        {
            public string sku { get; set; }
            public long produto { get; set; }
            public int cor { get; set; }
            public int estampa { get; set; }
            public string tamanho { get; set; }
            public List<CapacidadeProducao> capacidade_producao { get; set; }
        }
        public class CapacidadeProducao
        {
            public int filial { get; set; }
            public string cod_filial { get; set; }
            public int perc_estoque_disp { get; set; }
            public int capacidade_de_producao { get; set; }
            public int capacidade_disponivel { get; set; }
        }

    }
}
