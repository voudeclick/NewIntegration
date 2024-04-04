using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDC.Integration.APIClient.ServiceBus;
using VDC.Integration.Application.Strategy.Interfaces;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Queues;

namespace VDC.Integration.Application.Strategy.QueuesOverflow
{
    public class ValidatesIntegrationTypes
    {
        readonly List<IValidatesIntegrationType> _integrationType;

        public ValidatesIntegrationTypes(List<IValidatesIntegrationType> integrationType)
        {
            _integrationType = integrationType;
        }

        public async Task<List<QueueOverflow>> ReturnQueues(Tenant tenant,
                                                            ServiceBusService serviceBusService,
                                                            Func<Tenant, string, string> GetQueueName,
                                                            Func<Tenant, string, QueueRuntimeInfo, QueueOverflow> Enqueue)
        {
            var integrationValid = _integrationType.Where(x => x.IsTypeOf(tenant));
            var queueOverflow = new List<QueueOverflow>();

            foreach (var integration in integrationValid)
            {
                queueOverflow.AddRange(await integration.GetQueueRuntime(tenant, serviceBusService, GetQueueName, Enqueue));
            }
            return queueOverflow;
        }
    }
}
