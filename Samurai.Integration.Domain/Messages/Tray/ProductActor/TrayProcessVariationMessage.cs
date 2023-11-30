using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using static Samurai.Integration.Domain.Messages.Tray.Models.Product;

namespace Samurai.Integration.Domain.Messages.Tray.ProductActor
{
    public class TrayProcessVariationMessage
    {
        public string StoreId { get; set; }
        public Guid AppTrayProductId { get; set; }
        public long TrayProductId { get; set; }
        public List<Variation> Variations { get; set; }
        public bool UpdateStock { get; set; }
        public bool UpdatePrice { get; set; }
    }
}
