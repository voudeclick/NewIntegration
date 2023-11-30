using System;
using System.Collections.Generic;
using System.Text;
using static Samurai.Integration.Domain.Messages.Tray.Models.Product;

namespace Samurai.Integration.Domain.Messages.Tray.ProductActor
{
    public class TrayProcessCategoyMessage
    {
        public List<Category> Categories { get; set; }
    }
}
