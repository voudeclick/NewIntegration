using Samurai.Integration.Domain.Models.Pier8;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Pier8.OrderActor
{
    public class WebhookOrderProcessMessage
    {
        public string status { get; set; }
        public int statusCodigo { get; set; }
        public int idPier { get; set; }
        public List<Erro> erros { get; set; }
        public Pedido pedido { get; set; }
        public Destinatario destinatario { get; set; }
        public Transportadora transportadora { get; set; } = new Transportadora();
    }
}
