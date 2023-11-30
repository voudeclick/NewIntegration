using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Queues
{
    public class QueueCount
    {
        public string QueueName { get; set; }
        public long MessageCount { get; set; }
        public long DeadLetterMessageCount { get; set; }
        public long ScheduledMessageCount { get; set; }
    }
}
