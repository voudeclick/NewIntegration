using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using static Samurai.Integration.Domain.Messages.Tray.Models.Product;

namespace Samurai.Integration.Domain.Messages.Tray.ProductActor
{
    public class TrayProcessManufactureMessage
    {
        public List<Manufacture> Manufacturers { get; set; }
        public string Reference { get; set; }
    }
}
