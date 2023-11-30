using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Helpers;
using Samurai.Integration.Domain.Messages.Tray;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Queues
{
    public static class TrayQueue
    {
        public static string ProcessProductQueue = "tray-processproductqueue";
        public static string UpdateStatusOrderQueue = "tray-updatestatusorderqueue";
        public static string TrayAppReturnMessage = "tray-trayappreturnmessage";
        public static string TrayAppWebhook = "tray-webhook";    
        public static List<string> GetAllQueues()
        {
            return new List<string> {
                ProcessProductQueue,
                UpdateStatusOrderQueue,
                TrayAppReturnMessage,
                TrayAppWebhook
            };
        }

        public struct Queues
        {
            public QueueClient ProcessProductQueue { get; set; }
            public QueueClient UpdateStatusOrderQueue { get; set; }
            public QueueClient TrayAppReturnMessage { get; set; }
            public QueueClient TrayAppWebhook { get; set; }

            public async Task SendEncryptedMessage<T>(params (QueueClient queue, T message, bool hasItens)[] obj) where T : class
            {
                if (obj is null) return;

                for (int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].hasItens)
                    {
                        var serviceBusMessage = new ServiceBusMessage(obj[i].message);

                        var sendMessage = serviceBusMessage.GetMessage(Guid.NewGuid().ToString());

                        var payload = EncryptedMessage.Compress(Encoding.UTF8.GetString(sendMessage.Body));

                        sendMessage.UserProperties.Add("Compressed", true);
                        sendMessage.Body = Encoding.UTF8.GetBytes(payload);

                        await obj[i].queue.SendAsync(sendMessage);
                    }
                }
            }

            public async Task SendMessages<T>(params (QueueClient queue, T message, bool hasItens)[] obj) where T : class
            {
                if (obj is null) return;

                for (int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].hasItens)
                    {
                        var serviceBusMessage = new ServiceBusMessage(obj[i].message);
                        await obj[i].queue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));
                    }
                }
            }

            public async Task SendScheduleMessages<T>(params (QueueClient queue, T message, DateTimeOffset time, bool hasItens)[] obj) where T : class
            {
                if (obj is null) return;

                for (int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].hasItens)
                    {
                        var serviceBusMessage = new ServiceBusMessage(obj[i].message);
                        await obj[i].queue.ScheduleMessageAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()), obj[i].time);
                    }
                }
            }
        }
    }
}
