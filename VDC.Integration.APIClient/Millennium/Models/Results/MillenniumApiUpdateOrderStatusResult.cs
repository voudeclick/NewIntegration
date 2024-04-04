using System.Collections.Generic;

namespace VDC.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiUpdateOrderStatusResult
    {
        public List<Acoes> acoes { get; set; }

        public class Acoes
        {
            public string cod_pedidov { get; set; }
            public int acao { get; set; }
            public string erro { get; set; }
        }
    }
}
