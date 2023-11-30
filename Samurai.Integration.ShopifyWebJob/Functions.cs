using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.Actors.Shopify;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.ShopifyWebJob
{
    public class Functions
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusService _serviceBusService;
        private ILogger<Functions> _logger;
        private IActorRef _webJobActor;

        public Functions(IConfiguration configuration, IServiceProvider serviceProvider, ServiceBusService serviceBusService)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _serviceBusService = serviceBusService;

            _logger = _serviceProvider.GetService<ILogger<Functions>>();
        }

        [NoAutomaticTrigger]
        public async Task ShopifyOrchestrator(CancellationToken cancellationToken)
        {
            ActorSystem system = ActorSystem.Create("Shopify", _configuration.GetSection("Akka")["Config"]);
            _webJobActor = system.ActorOf(ShopifyWebJobActor.Props(_serviceProvider, cancellationToken));

            if (await _serviceBusService.QueueExists(TenantQueue.UpdateShopifyWebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateShopifyWebjobQueue);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };
            var _updateTenantQueue = _serviceBusService.GetQueueClient(TenantQueue.UpdateShopifyWebjobQueue);
            _updateTenantQueue.RegisterMessageHandler(ProcessUpdateTenantMessageAsync, messageHandlerOptions);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _webJobActor.Ask(new OrchestrateShopifyMessage(), cancellationToken);
                }
                catch (TaskCanceledException ex)
                {
                    //do nothing
                }

                await Task.Delay(int.Parse(_configuration.GetSection("Schedulle")["OrchestratorSleep"]), cancellationToken).ContinueWith(tsk => { }); //ignore exception
            }

            await system.Terminate();
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "ExceptionReceivedHandler encountered an exception");
            return Task.CompletedTask;
        }

        private async Task ProcessUpdateTenantMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var messageValue = new ServiceBusMessage(message.Body).GetValue();

            var result = await _webJobActor.Ask<ReturnMessage>(messageValue, cancellationToken);

            if (result.Result == Result.Error)
                _logger.LogError(result.Error, $"ProcessUpdateTenantMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
        }
    }
}
