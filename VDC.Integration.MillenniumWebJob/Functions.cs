using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.ServiceBus;
using VDC.Integration.Application.Actors.Millennium;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Messages.Millennium;
using VDC.Integration.Domain.Messages.ServiceBus;
using VDC.Integration.Domain.Queues;
using VDC.Integration.EntityFramework.Repositories;

namespace VDC.Integration.MillenniumWebJob
{

    public class Functions
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusService _serviceBusService;
        private readonly ILogger<Functions> _logger;
        private IActorRef _webJobActor;
        private readonly MillenniumSessionToken _millenniumSessionToken;

        public Functions(IConfiguration configuration, IServiceProvider serviceProvider, ServiceBusService serviceBusService, MillenniumSessionToken millenniumSessionToken)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _serviceBusService = serviceBusService;
            _millenniumSessionToken = millenniumSessionToken;
            _logger = _serviceProvider.GetService<ILogger<Functions>>();
        }

        [NoAutomaticTrigger]
        public async Task MilleniumOrchestrator(CancellationToken cancellationToken)
        {
            ActorSystem system = ActorSystem.Create("Millennium", _configuration.GetSection("Akka")["Config"]);
            _webJobActor = system.ActorOf(MillenniumWebJobActor.Props(_serviceProvider, cancellationToken, _millenniumSessionToken));

            if (await _serviceBusService.QueueExists(TenantQueue.UpdateMillenniumWebjobQueue) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateMillenniumWebjobQueue);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };
            var _updateTenantQueue = _serviceBusService.GetQueueClient(TenantQueue.UpdateMillenniumWebjobQueue);
            _updateTenantQueue.RegisterMessageHandler(ProcessUpdateTenantMessageAsync, messageHandlerOptions);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _webJobActor.Ask(new OrchestrateMillenniumMessage(), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    //do nothing
                }

                await Task.Delay(int.Parse(_configuration.GetSection("Schedulle")["OrchestratorSleep"]), cancellationToken).ContinueWith(tsk => { }); //ignore exception
            }

            await _updateTenantQueue.CloseAsync();
            await system.Terminate();
            _logger.LogInformation("Task Canceled - WebJobMillennium -(Functions | MilleniumOrchestrator)");
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
