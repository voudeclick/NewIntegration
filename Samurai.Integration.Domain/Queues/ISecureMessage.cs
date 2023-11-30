using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Queues
{
    public interface ISecureMessage
    {
        public bool CanSend();
        public string ExternalMessageId { get; }
    }
}
