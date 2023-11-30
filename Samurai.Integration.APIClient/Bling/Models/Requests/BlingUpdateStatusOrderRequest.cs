using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Samurai.Integration.APIClient.Bling.Models.Requests
{
    [XmlRoot(ElementName = "pedido")]
    public class BlingUpdateStatusOrderRequest
    {
        [XmlElement(ElementName = "idSituacao")]
        public int IdSituacao { get; set; }

        [XmlIgnore]
        public string IdPedido { get; set; }
    }
}
