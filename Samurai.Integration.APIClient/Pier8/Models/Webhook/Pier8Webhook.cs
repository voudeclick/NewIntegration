using Microsoft.AspNetCore.Mvc;
using Samurai.Integration.Domain.Models.Pier8;
using System.Collections.Generic;
using System.Text.Json;

namespace Samurai.Integration.APIClient.Pier8.Models.Webhook
{
    public class Pier8Webhook
    {
        [FromForm(Name = "pedido")]
        public string Raw { get; set; }

        private Item _pedido;

        public Item pedido {
            get 
            {
                _pedido = _pedido ?? JsonSerializer.Deserialize<Item>(Raw);
                return _pedido; 
            }
        }
        public class Item {
            public string token { get; set; }
            public string status { get; set; }
            public int statusCodigo { get; set; }
            public int idPier { get; set; }
            public List<Erro> erros { get; set; }
            public Pedido pedido { get; set; }
            public Destinatario destinatario { get; set; }
            public Transportadora transportadora { get; set; }
        }
        
    }
}
