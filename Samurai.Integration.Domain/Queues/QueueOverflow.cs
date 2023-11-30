using System.Collections.Generic;

namespace Samurai.Integration.Domain.Queues
{
    public class OfOverflow
    {
        public string TanantName { get; set; }
        public List<QueueOverflow> QueuesOverflow { get; set; }
       

        public OfOverflow()
        {
            QueuesOverflow = new List<QueueOverflow>();
        }
    }

    public class QueueOverflow
    {
        public string QueueName { get; set; }
        public long MessageCount { get; set; }
        public long DeadLetterMessageCount { get; set; }
    }
}
