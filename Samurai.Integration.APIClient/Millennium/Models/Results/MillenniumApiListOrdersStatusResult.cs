using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiListOrdersStatusResult
    {
        public List<Value> value { get; set; }

        public class Value
        {
            public long pedidov { get; set; }
            public string cod_pedidov { get; set; }
            public int status { get; set; }
            public string nfs { get; set; }
            public string numero_objeto { get; set; }
            public string desc_status { get; set; }
            public string data_atualizacao { get; set; }
        }
    }
}
