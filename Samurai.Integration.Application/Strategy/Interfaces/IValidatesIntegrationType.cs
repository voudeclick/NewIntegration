using Microsoft.Azure.ServiceBus.Management;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Strategy.Interfaces
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
