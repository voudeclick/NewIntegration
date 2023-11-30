using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.Actors.Tray;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Queues;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.TrayWebJob
{
    public class Functions
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusService _serviceBusService;
        private readonly ILogger<Functions> _logger;
        private IActorRef _webJobActor;

        public Functions(IConfiguration configuration, IServiceProvider serviceProvider, ServiceBusService serviceBusService)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _serviceBusService = serviceBusService;

            using (var scope = _serviceProvider.CreateScope())
            {
                _logger = _serviceProvider.GetService<ILogger<Functions>>();
            }
        }

        [NoAutomaticTrigger]
        public async Task TrayOrchestrator(CancellationToken cancellationToken)
        {

            ActorSystem system = ActorSystem.Create("TrayOrchestrator", _configuration.GetSection("Akka")["Config"]);
            _webJobActor = system.ActorOf(TrayWebJobActor.Props(_serviceProvider, cancellationToken));

            if (await _serviceBusService.QueueExists(TenantQueue.UpdateTrayWebjobQueue, false) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateTrayWebjobQueue, false);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };

            var _updateTenantQueue = _serviceBusService.GetQueueClient(TenantQueue.UpdateTrayWebjobQueue, false);
            _updateTenantQueue.RegisterMessageHandler(ProcessUpdateTenantMessageAsync, messageHandlerOptions);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _webJobActor.Ask(new OrchestrateTrayMessage(), cancellationToken);
                }
                catch (TaskCanceledException ex)
                {
                    //do nothing
                }

                await Task.Delay(int.Parse(_configuration.GetSection("Schedulle")["OrchestratorSleep"]), cancellationToken).ContinueWith(tsk => { }); //ignore exception
            }

            await _updateTenantQueue.CloseAsync();
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
