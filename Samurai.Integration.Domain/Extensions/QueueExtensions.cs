using Microsoft.Azure.ServiceBus;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Queues;
using System;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Extensions
{
    public static class QueueExtensions
    {
        public static async Task<QueueClient> CloseAsyncSafe(this QueueClient queue)
        {

            if (queue != null && !queue.IsClosedOrClosing)
                await queue.CloseAsync();
            queue = null;
            return queue;
        }

        public static async Task SendAsyncSafe<T>(this QueueClient queue, T message) 
            where T: ISecureMessage
        {
            if (message.CanSend())
            {
                var serviceBusMessage = new ServiceBusMessage(message);
                await queue.SendAsync(serviceBusMessage.GetMessage(message.ExternalMessageId));
            }
        }
        public static async Task ScheduleMessageAsyncSafe<T>(this QueueClient queue, T message, DateTimeOffset datetime)
            where T : ISecureMessage
        {
            if (message.CanSend())
            {
                var serviceBusMessage = new ServiceBusMessage(message);
                await queue.ScheduleMessageAsync(serviceBusMessage.GetMessage(message.ExternalMessageId), datetime);
            }
        }
    }
}
