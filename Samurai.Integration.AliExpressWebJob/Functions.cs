﻿using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.Actors.AliExpress;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.AliExpressWebJob
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
        public async Task AliExpressOrchestrator(
            ILogger logger,
            CancellationToken cancellationToken)
        {
            _logger = logger;
            ActorSystem system = ActorSystem.Create("AliExpress", _configuration.GetSection("Akka")["Config"]);
            _webJobActor = system.ActorOf(AliExpressWebJobActor.Props(_serviceProvider, cancellationToken));

            if (await _serviceBusService.QueueExists(TenantQueue.UpdateAliExpressWebJobQueue, false) == false)
                await _serviceBusService.CreateQueue(TenantQueue.UpdateAliExpressWebJobQueue, false);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };

            var _updateTenantQueue = _serviceBusService.GetQueueClient(TenantQueue.UpdateAliExpressWebJobQueue, false);
            _updateTenantQueue.RegisterMessageHandler(ProcessUpdateTenantMessageAsync, messageHandlerOptions);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _webJobActor.Ask(new OrchestratorAliExpressMessage(), cancellationToken);
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
