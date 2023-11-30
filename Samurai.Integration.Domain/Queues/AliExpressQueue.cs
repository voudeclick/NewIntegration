using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Queues
{
    public static class AliExpressQueue
    {
        public static string ListOrderQueue = "aliexpress-listorderqueue";

        public static List<string> GetAllQueues()
        {
            return new List<string>
            {
                ListOrderQueue
            };
        }
    }
}
