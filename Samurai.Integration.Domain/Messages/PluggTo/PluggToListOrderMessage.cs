using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.PluggTo
{
    public class PluggToListOrderMessage
    {
        public string ExternalOrderId { get; set; }
        public string Order { get; set; }
    }
}
