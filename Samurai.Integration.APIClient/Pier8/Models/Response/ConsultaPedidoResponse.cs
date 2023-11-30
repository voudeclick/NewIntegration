using Samurai.Integration.Domain.Models.Pier8;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Pier8.Models.Response
{
    public class ConsultaPedidoResponse
    {
        public string status { get; set; }
        public int statusCodigo { get; set; }
        public int idPier { get; set; }
        public List<Erro> erros { get; set; }
        public Pedido pedido { get; set; }
        public Destinatario destinatario { get; set; }
        public Transportadora transportadora { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string CodigoRastreamento
        {
            get 
            {
                if (!string.IsNullOrEmpty(transportadora.rastreador.codigo))
                    return transportadora.rastreador.codigo;

                if (!string.IsNullOrEmpty(transportadora.cte_numero))
                    return $"{transportadora.cte_numero}-{transportadora.cte_serie}";

                return string.Empty;
            }
            
        }
    }
}
