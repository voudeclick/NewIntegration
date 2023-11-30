using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Tray.OrderActor
{
    public class TrayListOrderMessage
    {
        public long OrderId { get; set; }
        public string ExternalId { get; set; }
    }
}
