using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.APIClient.ServiceBus;
using VDC.Integration.Application.Strategy.Interfaces;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Queues;

namespace VDC.Integration.Application.Strategy.QueuesOverflow
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
                if (info.MessageCountDetails.ActiveMessageCount >= 500 || info.MessageCountDetails.DeadLetterMessageCount >= 100)
                    result.Add(Enqueue(tenant, item, info));
            }

            return result;
        }
    }
}
