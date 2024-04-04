using System.Collections.Generic;

namespace VDC.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiGetListIdFotoResult
    {
        public List<Value> value { get; set; } = new List<Value>();
        public class Value
        {
            public long idfoto { get; set; }
            public int? ordem { get; set; }
            public string descricao { get; set; }
            public long produto { get; set; }
            public string cod_produto { get; set; }
            public int? cor { get; set; }
            public string cod_cor { get; set; }
            public int? estampa { get; set; }
            public string cod_est { get; set; }
            public string tamanho { get; set; }
        }
    }
}
