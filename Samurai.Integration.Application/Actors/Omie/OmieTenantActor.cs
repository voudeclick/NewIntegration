using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Omie.Models.Request.FamiliaCadastro;
using Samurai.Integration.APIClient.Omie.Models.Request.FamiliaCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result;
using Samurai.Integration.APIClient.Omie.Models.Result.FamiliaCadastro;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Entities.Database.Logs;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Omie;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Omie
{
    public class OmieTenantActor : BaseOmieTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;
        private readonly int _maximumRetryCount = 3;

        #region Actors
        private IActorRef _apiActorGroup;
        private IActorRef _productActor;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private QueueClient _listFullProductQueue;
        private QueueClient _listAllProductsQueue;
        private QueueClient _listPartialProductQueue;
        private QueueClient _listStockQueue;
        private QueueClient _listOrderQueue;
        private QueueClient _updateOrderQueue;
        private QueueClient _shopifyListOrderQueueClient;
        private QueueClient _shopifyUpdateOrderNumberTagQueueClient;
        #endregion

        #region Tasks
        private Task _listAllProductsTask;
        #endregion

        public OmieTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            : base("OmieTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializeOmieTenantMessage, Task>)(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        Initialize(message.Data, scope);
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in InitializeOmieTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync<UpdateOmieTenantMessage>(async message =>
            {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_omieData.EqualsTo(message.Data) == false)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            await Stop();
                            Initialize(message.Data, scope);
                        }
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in UpdateOmieTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopOmieTenantMessage, Task>)(async message =>
            {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in StopOmieTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task Stop()
        {
            if (_omieData != null && _taskCancellationTokenSource != null)
            {
                LogInfo($"Stoping Tasks");
                #region Tasks

                _taskCancellationTokenSource.Cancel();

                if (_omieData.ProductIntegrationStatus == true)
                {

                }

                if (_omieData.OrderIntegrationStatus == true)
                {

                }

                #endregion

                LogInfo($"Stoping QueueClients");
                #region QueueClients

                if (_omieData.ProductIntegrationStatus == true)
                {
                    if (_listFullProductQueue != null && !_listFullProductQueue.IsClosedOrClosing)
                        await _listFullProductQueue.CloseAsync();
                    _listFullProductQueue = null;

                    if (_listAllProductsQueue != null && !_listAllProductsQueue.IsClosedOrClosing)
                        await _listAllProductsQueue.CloseAsync();
                    _listAllProductsQueue = null;

                    if (_listPartialProductQueue != null && !_listPartialProductQueue.IsClosedOrClosing)
                        await _listPartialProductQueue.CloseAsync();
                    _listPartialProductQueue = null;

                    if (_listStockQueue != null && !_listStockQueue.IsClosedOrClosing)
                        await _listStockQueue.CloseAsync();
                    _listStockQueue = null;
                }

                if (_omieData.OrderIntegrationStatus == true)
                {
                    if (_updateOrderQueue != null && !_updateOrderQueue.IsClosedOrClosing)
                        await _updateOrderQueue.CloseAsync();
                    _updateOrderQueue = null;

                    if (_listOrderQueue != null && !_listOrderQueue.IsClosedOrClosing)
                        await _listOrderQueue.CloseAsync();
                    _listOrderQueue = null;

                    if (_shopifyListOrderQueueClient != null && !_shopifyListOrderQueueClient.IsClosedOrClosing)
                        await _shopifyListOrderQueueClient.CloseAsync();
                    _shopifyListOrderQueueClient = null;

                    if (_shopifyUpdateOrderNumberTagQueueClient != null && !_shopifyUpdateOrderNumberTagQueueClient.IsClosedOrClosing)
                        await _shopifyUpdateOrderNumberTagQueueClient.CloseAsync();
                    _shopifyUpdateOrderNumberTagQueueClient = null;
                }

                #endregion

                LogInfo($"Stoping Actors");
                #region Actors

                if (_omieData.ProductIntegrationStatus == true)
                {
                    if (_productActor != null)
                        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _productActor = null;
                }

                if (_omieData.OrderIntegrationStatus == true)
                {
                    if (_orderActor != null)
                        await _orderActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _orderActor = null;
                }

                if (_apiActorGroup != null)
                    await _apiActorGroup.GracefulStop(TimeSpan.FromSeconds(30));
                _apiActorGroup = null;

                #endregion

                _taskCancellationTokenSource.Dispose();
                _taskCancellationTokenSource = null;
            }
        }

        private void Initialize(OmieData data, IServiceScope scope)
        {
            var maxConcurrency = 1;
            _omieData = data;
            LogInfo($"Initializing");
            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            LogInfo($"Initializing Actors");
            #region Actors
            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(OmieApiActor.Props(_serviceProvider, _webJobCancellationToken, _omieData)));

            if (_omieData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                    .Props(OmieProductActor.Props(_serviceProvider, _webJobCancellationToken, _omieData, _apiActorGroup)));
            }

            if (_omieData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(OmieOrderActor.Props(_serviceProvider, _webJobCancellationToken, _omieData, _apiActorGroup)));
            }

            #endregion

            LogInfo($"Initializing QueueClients");
            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };

            if (_omieData.ProductIntegrationStatus == true)
            {
                _listAllProductsTask = ProcessListAllProductsMessageAsync();

                //_listFullProductQueue = _tenantService.GetQueueClient(_omieData, OmieQueue.ListFullProductQueue);
                //_listFullProductQueue.RegisterMessageHandler(ProcessListFullProductMessageAsync, messageHandlerOptions);

                //_listAllProductsQueue = _tenantService.GetQueueClient(_omieData, OmieQueue.ListAllProductsQueue);
                //_listAllProductsQueue.RegisterMessageHandler(ProcessListAllProductsMessageAsync, new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                //{
                //    MaxConcurrentCalls = maxConcurrency,
                //    MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                //    AutoComplete = false
                //});

                //_listPartialProductQueue = _tenantService.GetQueueClient(_omieData, OmieQueue.ListPartialProductQueue);
                //_listPartialProductQueue.RegisterMessageHandler(ProcessListPartialProductMessageAsync, messageHandlerOptions);

                //_listStockQueue = _tenantService.GetQueueClient(_omieData, OmieQueue.ListStockQueue);
                //_listStockQueue.RegisterMessageHandler(ProcessListStockMessageAsync, messageHandlerOptions);
            }

            if (_omieData.OrderIntegrationStatus == true)
            {
                _updateOrderQueue = _tenantService.GetQueueClient(_omieData, OmieQueue.UpdateOrderQueue);
                _updateOrderQueue.RegisterMessageHandler(ProcessUpdateOrderMessageAsync, messageHandlerOptions);

                _listOrderQueue = _tenantService.GetQueueClient(_omieData, OmieQueue.ListOrderQueue);
                _listOrderQueue.RegisterMessageHandler(ProcessListOrderMessageAsync, messageHandlerOptions);

                _shopifyListOrderQueueClient = _tenantService.GetQueueClient(_omieData, ShopifyQueue.ListOrderQueue);

                _shopifyUpdateOrderNumberTagQueueClient = _tenantService.GetQueueClient(_omieData, ShopifyQueue.UpdateOrderNumberTagQueue);
            }

            #endregion

            LogInfo($"Initializing Tasks");
            #region Tasks

            if (_omieData.ProductIntegrationStatus == true)
            {
            }

            if (_omieData.OrderIntegrationStatus == true)
            {
            }

            #endregion
        }

        private async Task ProcessListFullProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListFullProductMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListFullProductMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listFullProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    await AbandonMessageAsync(message, _listFullProductQueue, "ProcessListFullProductMessageAsync", "product", messageValue, result);
                }
            }
        }

        private async Task ProcessListAllProductsMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListAllProductsMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<OmieListAllProductsMessage>();
                var allIds = new List<long>();
                var page = 0;
                ReturnMessage<PesquisarFamiliasOmieRequestOutput> result = null;

                do
                {
                    page++;
                    result = await _productActor.Ask<ReturnMessage<PesquisarFamiliasOmieRequestOutput>>(new PesquisarFamiliasOmieRequest(new PesquisarFamiliasOmieRequestInput
                    {
                        pagina = page,
                        registros_por_pagina = 50
                    }), _webJobCancellationToken);

                    if (result.Result == Result.Error)
                    {
                        await AbandonMessageAsync(message, _listAllProductsQueue, "ProcessListAllProductsMessageAsync", "product", messageValue, result);
                        return;
                    }
                    allIds.AddRange(result.Data.famCadastro.Select(p => p.codigo));
                } while (result.Data.total_de_paginas > page);

                foreach (var ids in allIds.Chunk(10))
                {
                    var enqueueResult = await _productActor.Ask<ReturnMessage>(new OmieEnqueueFullProductsMessage { ProductsIds = ids.ToList() }, _webJobCancellationToken);
                    if (enqueueResult.Result == Result.Error)
                    {
                        await AbandonMessageAsync(message, _listAllProductsQueue, "ProcessListAllProductsMessageAsync", "product", messageValue, result);
                        return;
                    }
                }
                LogInfo($"ProcessListAllProductsMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                await _listAllProductsQueue.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        private async Task ProcessListAllProductsMessageAsync()
        {
            var allIds = new List<long>();
            var page = 0;
            ReturnMessage<PesquisarFamiliasOmieRequestOutput> result = null;

            do
            {
                page++;
                result = await _productActor.Ask<ReturnMessage<PesquisarFamiliasOmieRequestOutput>>(new PesquisarFamiliasOmieRequest(new PesquisarFamiliasOmieRequestInput
                {
                    pagina = page,
                    registros_por_pagina = 50
                }));

                if (result.Result == Result.Error)
                    return;

                allIds.AddRange(result.Data.famCadastro.Select(p => p.codigo));
            } while (result.Data.total_de_paginas > page);

            foreach (var ids in allIds.Chunk(10))
            {
                var enqueueResult = await _productActor.Ask<ReturnMessage>(new OmieEnqueueFullProductsMessage { ProductsIds = ids.ToList() }, _webJobCancellationToken);
                if (enqueueResult.Result == Result.Error)
                    return;
            }
        }

        private async Task ProcessListPartialProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListPartialProductMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListPartialProductMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listPartialProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    await AbandonMessageAsync(message, _listPartialProductQueue, "ProcessListPartialProductMessageAsync", "product", messageValue, result);
                }
            }
        }

        private async Task ProcessListStockMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListStockMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListStockMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listStockQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    await AbandonMessageAsync(message, _listStockQueue, "ProcessListStockMessageAsync", "stock", messageValue, result);
                }
            }
        }

        private async Task ProcessUpdateOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessUpdateOrderMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifySendOrderToERPMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessUpdateOrderMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _updateOrderQueue.CompleteAsync(message.SystemProperties.LockToken);

                    var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
                    {
                        ShopifyId = messageValue.ID,
                        IntegrationStatus = messageValue.GetOrderStatus(),
                        OrderExternalId = messageValue.ExternalID,
                        OrderNumber = messageValue.Number.ToString()
                    });
                    await _shopifyUpdateOrderNumberTagQueueClient.SendAsync(serviceBusMessage.GetMessage(messageValue.ID));
                }
                else
                {
                    await AbandonMessageAsync(message, _updateOrderQueue, "ProcessUpdateOrderMessageAsync", "order", messageValue, result);
                }
            }
        }

        private async Task ProcessListOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListOrderMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListOrderMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    await AbandonMessageAsync(message, _listOrderQueue, "ProcessListOrderMessageAsync", "order", messageValue, result);
                }
            }
        }

        private async Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError(arg.Exception, "Error in Omie Queue");
        }


        protected override void PostStop()
        {
            if (_taskCancellationTokenSource != null)
                _taskCancellationTokenSource.Dispose();

            ActorTaskScheduler.RunTask(async () =>
            {
                await Stop();
            });
        }

        private async Task AbandonMessageAsync(Message message,
            QueueClient queue,
            string method = "",
            string type = "",
            object request = null,
            ReturnMessage response = null)
        {

            var logId = Guid.NewGuid();


            try
            {
                if (message.SystemProperties.DeliveryCount >= _maximumRetryCount)
                {
                    try
                    {
                        var log = LoggerDescription.From(_omieData.Id.ToString(), type, method, request, response, logId);

                        var scope = _serviceProvider.CreateScope();
                        var logsAzureIdentityRepository = scope.ServiceProvider.GetService<LogsAbandonMessageRepository>();
                        var logs = new LogsAbandonMessage(logId, "OmieWebJob", _omieData.Id, method, type, Newtonsoft.Json.JsonConvert.SerializeObject(request));
                        logs.AddErrorInfo(response);

                        var exist = await logsAzureIdentityRepository.ExistAsync(logs);
                        if (!exist)
                        {
                            await logsAzureIdentityRepository.AddAsync(logs);
                            LogError("AbandonMessageAsync - log: {0}", log);
                        }
                        var error = response?.Error;

                        await SendOrderErrorTagToShopify(request, error);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "(Omie )Error in AbandonMessageAsync/logsAzureIdentityRepository");
                        await SendOrderErrorTagToShopify(request, ex);
                    }

                    await queue.DeadLetterAsync(message.SystemProperties.LockToken, new Dictionary<string, object> { { "LogId", logId } }).ConfigureAwait(false);
                }
                else
                {
                    await queue.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Exception when abandon a event message from {0} of Azure Service Bus.", message.SystemProperties.LockToken);
            }
        }

        private async Task SendOrderErrorTagToShopify(object request, Exception exception)
        {
            if (request?.GetType() != typeof(ShopifySendOrderToERPMessage))
            {
                return;
            }

            var messageError = RecorveryMessage(exception);

            var tag = GenerateTagError(exception, messageError);

            await UpdateOrderTagOnShopify((ShopifySendOrderToERPMessage)request, tag);

        }
        private string RecorveryMessage(Exception exception)
        {
            string messageError;

            if (exception?.GetType() == typeof(OmieException))
            {
                messageError = ((OmieException)exception)?.Error?.faultstring;
            }
            else
            {
                var indexStart = exception?.Message?.IndexOf('{');

                if (!indexStart.HasValue || indexStart == -1)
                {
                    messageError = exception?.Message;
                }
                else
                {
                    try
                    {
                        var omieError = Newtonsoft.Json.JsonConvert.DeserializeObject<OmieError>(exception?.Message.Substring(indexStart.Value));

                        messageError = omieError.faultstring;
                    }
                    catch
                    {
                        messageError = exception?.Message;
                    }
                }
            }

            return messageError;
        }

        private string GenerateTagError(Exception exception, string message)
        {
            using var scope = _serviceProvider.CreateScope();

            var _integrationErrorService = scope.ServiceProvider.GetService<IntegrationErrorService>();

            return _integrationErrorService.GenerateTagError(message, IntegrationErrorSource.Omie, () => IsOmieException(exception)).Result;

            static bool IsOmieException(Exception exception)
            {
                return exception?.GetType() == typeof(OmieException) || (exception?.Message?.Contains(OmieConsts.OmieApiPostCall, StringComparison.InvariantCultureIgnoreCase) ?? false);
            }

        }

        private async Task UpdateOrderTagOnShopify(ShopifySendOrderToERPMessage request, string tag)
        {
            if (tag == "Erro-omie-1")
            {
                return;
            }

            var messageValue = request;
            var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
            {
                ShopifyId = messageValue.ID,
                CustomTags = new List<string>() { tag }
            });

            await _shopifyUpdateOrderNumberTagQueueClient.SendAsync(serviceBusMessage.GetMessage(messageValue.ID));

        }


        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new OmieTenantActor(serviceProvider, cancellationToken));
        }
    }
}
