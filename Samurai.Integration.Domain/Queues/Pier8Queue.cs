using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Queues
{
    public class Pier8Queue
    {
        //Init
        public static string WebHookProcessQueue = "pier8-webhookprocessqueue";
        public static string ProcessUpdateTrackingQueue = "pier8-processupdatetrackingqueue";



        public static List<string> GetAllQueues()
        {
            return new List<string> {
                WebHookProcessQueue,
                ProcessUpdateTrackingQueue
            };
        }
        public struct Queues
        {
            public QueueClient WebHookProcessQueue { get; set; }
            public QueueClient ProcessUpdateTrackingQueue { get; set; }


        }

    }
}
