using Microsoft.Azure.ServiceBus.Management;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.Strategy.Interfaces;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Strategy.QueuesOverflow
{
    public class Millennium : IValidatesIntegrationType
    {
        public bool IsTypeOf(Tenant tenant)
        {
            return tenant.Type == TenantType.Millennium;
        }

        public async Task<List<QueueOverflow>> GetQueueRuntime(Tenant tenant,
                                                                   ServiceBusService serviceBusService,
                                                                   Func<Tenant, string, string> GetQueueName,
                                                                   Func<Tenant, string, QueueRuntimeInfo, QueueOverflow> Enqueue)
        {
            var result = new List<QueueOverflow>();
            foreach (var item in MillenniumQueue.GetAllQueues())
            {
                var info = await serviceBusService.GetQueueRuntimeInfo(GetQueueName(tenant, item));
                if(info.MessageCountDetails.ActiveMessageCount >= 500 || info.MessageCountDetails.DeadLetterMessageCount >= 100)
                    result.Add(Enqueue(tenant, item, info));               
            }

            return result;
        }
    }
}
