using Microsoft.Extensions.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

namespace Samurai.Integration.APIClient.ServiceBus
{
    public class ServiceBusService
    {
        private ManagementClient _managementClient;
        private ManagementClient _managementClientTrayApps;

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        public ServiceBusService(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<bool> QueueExists(string queueName, bool sbDefault = true)
        {
            var client = GetManagementClient();

            if (!sbDefault)
                client = GetManagementClientTrayApps();

            return await client.QueueExistsAsync(queueName);
        }

        public async Task CreateQueue(string queueName, bool sbDefault = true)
        {
            var client = GetManagementClient();

            if (!sbDefault)
                client = GetManagementClientTrayApps();

            await client.CreateQueueAsync(new QueueDescription(queueName)
            {
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(2),
                MaxDeliveryCount = 3

            });
        }
        public async Task CreateQueueWithLockDuration(string queueName, int lockDuration = 1, bool sbDefault = true)
        {
            var client = GetManagementClient();

            if (!sbDefault)
                client = GetManagementClientTrayApps();

            await client.CreateQueueAsync(new QueueDescription(queueName)
            {
                RequiresDuplicateDetection = _environment.IsEnvironment("Production") ? true : false,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(lockDuration),
                MaxDeliveryCount = 3,
                LockDuration = TimeSpan.FromMinutes(5)

            });
        }

        public async Task DeleteQueue(string queueName, bool sbDefault = true)
        {
            var client = GetManagementClient();

            if (!sbDefault)
                client = GetManagementClientTrayApps();

            await client.DeleteQueueAsync(queueName);
        }

        public Task<QueueRuntimeInfo> GetQueueRuntimeInfo(string queueName)
        {
            return GetManagementClient().GetQueueRuntimeInfoAsync(queueName);
        }

        public QueueClient GetQueueClient(string queueName, bool sbDefault = true)
        {
            var client = _configuration.GetSection("ServiceBus")["ConnectionString"];

            if (!sbDefault)
                client = _configuration.GetSection("ServiceBus")["TrayApps"];

            return new QueueClient(client, queueName);
        }

        private ManagementClient GetManagementClient()
        {
            return _managementClient ?? (_managementClient = new ManagementClient(_configuration.GetSection("ServiceBus")["ConnectionString"]));
        }
        private ManagementClient GetManagementClientTrayApps()
        {
            return _managementClientTrayApps ?? (_managementClientTrayApps = new ManagementClient(_configuration.GetSection("ServiceBus")["TrayApps"]));
        }
    }
}
