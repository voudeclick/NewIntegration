using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Pier8
{
    public  class Transportadora
    {
        public string nome { get; set; }
        public string motorista { get; set; }
        public string motorista_rg { get; set; }
        public string veiculo { get; set; }
        public string veiculo_placa { get; set; }
        public string cte_numero { get; set; }
        public string cte_serie { get; set; }
        public Rastreador rastreador { get; set; } = new Rastreador();
        public class Rastreador 
        {
            public string codigo { get; set; }
            public object data { get; set; }
            public string link { get; set; }
        }
    }
}
