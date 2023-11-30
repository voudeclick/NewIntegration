using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
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
