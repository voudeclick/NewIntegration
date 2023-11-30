using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;

using System.Xml.Serialization;

namespace Samurai.Integration.APIClient.Bling.Models.Requests
{
    [XmlRoot(ElementName = "pedido")]
    public class BlingApiUpdateOrderRequest
    {
        [XmlIgnore]
        public string NumeroPedido { get; set; }

        [XmlElement(ElementName = "idSituacao")]
        public int IdSituacao { get; set; }

        public static BlingApiUpdateOrderRequest From(OrderSellerDto orderSeller, BlingDataOrderStatusMapping statusMapping)
        {
            return new BlingApiUpdateOrderRequest
            {
                NumeroPedido = orderSeller?.ClientId?.Trim(),
                IdSituacao = statusMapping.BlingSituacaoId
            };
        }
    }
}
