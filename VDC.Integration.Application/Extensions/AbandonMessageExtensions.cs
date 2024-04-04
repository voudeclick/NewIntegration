using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.Domain.Results.Logger;

namespace VDC.Integration.Application.Extensions
{
    public static class AbandonMessageExtensions
    {
        [Obsolete]
        public async static Task<string> AbandonMessageAsync(Message message, QueueClient queue, string tenantId,
            string method = "", string type = "", object request = null, object response = null, int maxDelieveryCount = 3) =>
                await Task.Run(async () =>
                    {
                        string log = default;
                        if (message.SystemProperties.DeliveryCount >= maxDelieveryCount)
                        {
                            var logId = Guid.NewGuid();

                            log = LoggerDescription.From(tenantId, type, method, request, response, logId);

                            await queue.DeadLetterAsync(message.SystemProperties.LockToken, new Dictionary<string, object> { { "LogId", logId } }).ConfigureAwait(false);
                        }
                        else
                        {
                            await queue.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                        }
                        return log;
                    }).ContinueWith((ex) =>
                    {
                        return $"Exception when abandon a event message from {message.SystemProperties.LockToken} of Azure Service Bus.";
                    }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
