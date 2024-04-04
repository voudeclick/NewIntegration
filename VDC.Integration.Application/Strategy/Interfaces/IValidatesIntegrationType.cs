using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.APIClient.ServiceBus;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Queues;

namespace VDC.Integration.Application.Strategy.Interfaces
{
    public interface IValidatesIntegrationType
    {
        bool IsTypeOf(Tenant tenant);

        Task<List<QueueOverflow>> GetQueueRuntime(Tenant tenant,
                                                      ServiceBusService serviceBusService,
                                                      Func<Tenant, string, string> GetQueueName,
                                                      Func<Tenant, string, QueueRuntimeInfo, QueueOverflow> Enqueue);
    }
}
