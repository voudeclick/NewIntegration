using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.Actors.PluggTo;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.PluggToWebJob
{
    public class Functions
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusService _serviceBusService;

        private ILogger _logger;
        private IActorRef _webJobActor;

        public Functions(IConfiguration configuration, IServiceProvider serviceProvider, ServiceBusService serviceBusService)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _serviceBusService = serviceBusService;
        }

        [NoAutomaticTrigger]
        public async Task PluggToOrchestrator(ILogger logger, CancellationToken cancellationToken)
        {
            _logger = logger;

            ActorSystem system = ActorSystem.Create("PluggTo", _configuration.GetSection("Akka")["Config"]);
            _webJobActor = system.ActorOf(PluggToWebJobActor.Props(_serviceProvider, cancellationToken));

            if (!await _serviceBusService.QueueExists(TenantQueue.UpdatePluggToWebjobQueue))
                await _serviceBusService.CreateQueue(TenantQueue.UpdatePluggToWebjobQueue);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = 1, AutoComplete = true };

            var _updateTenantQueue = _serviceBusService.GetQueueClient(TenantQueue.UpdatePluggToWebjobQueue);
            _updateTenantQueue.RegisterMessageHandler(ProcessUpdateTenantMessageAsync, messageHandlerOptions);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _webJobActor.Ask(new OrchestratePluggToMessage(), cancellationToken);
                }
                catch { }

                await Task.Delay(int.Parse(_configuration.GetSection("Schedulle")["OrchestratorSleep"]), cancellationToken).ContinueWith(tsk => { }); //ignore exception
            }

            await _updateTenantQueue.CloseAsync();
            await system.Terminate();

        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "Handler in PluggToWebJob encountered an exception", new { MethodName = nameof(ExceptionReceivedHandler)});
            return Task.CompletedTask;
        }
        private async Task ProcessUpdateTenantMessageAsync(Message message, CancellationToken cancellationToken)
        { 
            var messageValue = new ServiceBusMessage(message.Body).GetValue();

            var result = await _webJobActor.Ask<ReturnMessage>(messageValue, cancellationToken);

            if (result.Result == Result.OK)
            {
                _logger.LogInformation($"Finished message", 
                    new { MethodName = nameof(ProcessUpdateTenantMessageAsync), MessageId = message.MessageId, SequenceNumber = message.SystemProperties.SequenceNumber});
            }
            else
            {
                _logger.LogError(result.Error, $"Error during message: {result.Error.Message}",
                    new { MethodName = nameof(ProcessUpdateTenantMessageAsync), MessageId = message.MessageId, SequenceNumber = message.SystemProperties.SequenceNumber });
            }
        }
    }
}
