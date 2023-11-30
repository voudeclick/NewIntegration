using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Pier8
{
    public class Pedido
    {
        public string nroPedido { get; set; }
        public object dataPedido { get; set; }
        public string emergencial { get; set; }
        public int he { get; set; }
        public int ht { get; set; }
    }
}
