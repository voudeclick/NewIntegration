using System;
using System.Collections.Generic;
using System.Text;
using static Samurai.Integration.Domain.Messages.Tray.Models.Product;

namespace Samurai.Integration.Domain.Messages.Tray.ProductActor
{
    public class TrayProcessAttributesProductMessage
    {
        public List<ListAttributes> Attributes { get; set; }
    }
}
