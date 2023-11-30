using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.Actors.Omie;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Omie;
using Samurai.Integration.Domain.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.OmieWebJob
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
        public async Task OmieOrchestrator(
            ILogger logger, 
            CancellationToken cancellationToken)
        {
            _logger = logger;
            ActorSystem system = ActorSystem.Create("Omie", _configuration.GetSection("Akka")["Config"]);
            _webJobActor = system.ActorOf(OmieWebJobActor.Props(_serviceProvider, cancellationToken));

            if (await _serviceBusService.QueueExists(TenantQueue.UpdateOmieWebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateOmieWebjobQueue);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };
            var _updateTenantQueue = _serviceBusService.GetQueueClient(TenantQueue.UpdateOmieWebjobQueue);
            _updateTenantQueue.RegisterMessageHandler(ProcessUpdateTenantMessageAsync, messageHandlerOptions);

            _logger.LogInformation("starting OmieOrchestrator");
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("processing OmieOrchestrator");

                try
                {
                    await _webJobActor.Ask(new OrchestrateOmieMessage(), cancellationToken);
                }
                catch(TaskCanceledException ex)
                {
                    //do nothing
                }

                _logger.LogInformation("finishing OmieOrchestrator");
                await Task.Delay(int.Parse(_configuration.GetSection("Schedulle")["OrchestratorSleep"]), cancellationToken).ContinueWith(tsk => { }); //ignore exception
            }
            _logger.LogInformation("terminating OmieOrchestrator");
            await system.Terminate();
            _logger.LogInformation("OmieOrchestrator terminated");
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "ExceptionReceivedHandler encountered an exception");
            return Task.CompletedTask;
        }

        private async Task ProcessUpdateTenantMessageAsync(Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"ProcessUpdateTenantMessageAsync Received message {message.MessageId},: SequenceNumber:{message.SystemProperties.SequenceNumber}");

            var messageValue = new ServiceBusMessage(message.Body).GetValue();

            var result = await _webJobActor.Ask<ReturnMessage>(messageValue, cancellationToken);

            if (result.Result == Result.OK)
            {
                _logger.LogInformation($"ProcessUpdateTenantMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            }
            else
            {
                _logger.LogError(result.Error, $"ProcessUpdateTenantMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            }
        }
    }
}
