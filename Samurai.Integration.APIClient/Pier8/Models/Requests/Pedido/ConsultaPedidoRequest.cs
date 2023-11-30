namespace Samurai.Integration.APIClient.Pier8.Models.Requests.Pedido
{
    public class ConsultaPedidoRequest : IBodyXml
    {
        public string pedido { get; set; }

        public string ToXml()
        {
            return $"<pedido>{pedido}</pedido>";
        }

    }
   
}
