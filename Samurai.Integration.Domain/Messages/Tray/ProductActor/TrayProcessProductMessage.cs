using Akka.Routing;
using Samurai.Integration.Domain.Messages.Tray.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Tray.ProductActor
{
    public class TrayProcessProductMessage
    {
        public Product.Info Product { get; set; }
        public bool Deleted { get; set; }
        public bool UpdateAvailable { get; set; }
        public bool UpdateStock { get; set; }
        public bool UpdatePrice { get; set; }
        public string StoreId { get; set; }
        public string Status { get; set; }
    }
}
