using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Samurai.Integration.APIClient.SellerCenter.Models;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Messages.SellerCenter.ProductActor;
using Samurai.Integration.Domain.Queues;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.SellerCenter
{
    public class SellerCenterTenantActor : BaseSellerCenterTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;

        #region Actors
        private List<IActorRef> _apiActors;
        private IActorRef _apiActorGroup;
        private IActorRef _productActor;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private SellerCenterQueue.Queues _queues;
        private IConfiguration _configuration;
        private Task _listNewOrdersTask;

        #endregion

        public SellerCenterTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            : base("SellerCenterTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializeSellerCenterTenantMessage, Task>)(async message =>
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
                    LogError(ex, "SellerCenterTenantActor - Error in InitializeSellerCenterTenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in InitializeSellerCenterTenantMessage | {ex.Message}"));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync<UpdateSellerCenterTenantMessage>(async message =>
            {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_sellerCenterData.EqualsTo(message.Data) == false)
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
                    LogError(ex, "SellerCenterTenantActor -Error in UpdateShopifyTenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in UpdateShopifyTenantMessage | {ex.Message}"));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopSellerCenterTenantMessage, Task>)(async message =>
            {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterTenantActor -Error in StopSellerCenterTenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in StopSellerCenterTenantMessage | {ex.Message}"));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task Stop()
        {
            if (_sellerCenterData != null && _taskCancellationTokenSource != null)
            {
                #region Tasks
                _taskCancellationTokenSource.Cancel();

                if (_sellerCenterData.OrderIntegrationStatus == true && _sellerCenterData.ErpType != TenantType.Bling)
                {                    
                   await _listNewOrdersTask.CloseAsyncSafe();
                }

                #endregion

                #region QueueClients
                if (_sellerCenterData.ProductIntegrationStatus == true)
                {
                    await _queues.CreateProductQueue.CloseAsyncSafe();
                    await _queues.ProcessVariationOptionsProductQueue.CloseAsyncSafe();
                    await _queues.ProcessCategoriesProductQueue.CloseAsyncSafe();
                    await _queues.ProcessManufacturersProductQueue.CloseAsyncSafe();
                    await _queues.UpdatePriceQueue.CloseAsyncSafe();
                    await _queues.UpdateStockProductQueue.CloseAsyncSafe();

                }

                if (_sellerCenterData.OrderIntegrationStatus == true)
                {
                    await _queues.ProcessOrderQueue.CloseAsyncSafe();
                    await _queues.ListOrderQueue.CloseAsyncSafe();
                    await _queues.ListNewOrdersQueue.CloseAsyncSafe();
                    await _queues.UpdatePartialOrderSeller.CloseAsyncSafe();
                    await _queues.UpdateOrderSellerDeliveryPackage.CloseAsyncSafe();
                }

                #endregion

                #region Actors
                if (_sellerCenterData.ProductIntegrationStatus == true)
                {
                    if (_productActor != null)
                        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _productActor = null;
                }

                if (_sellerCenterData.OrderIntegrationStatus == true)
                {
                    if (_orderActor != null)
                        await _orderActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _orderActor = null;
                }

                if (_apiActors != null)
                {
                    foreach (var apiActor in _apiActors)
                    {
                        await apiActor.GracefulStop(TimeSpan.FromSeconds(30));
                    }
                    if (_apiActorGroup != null)
                        await _apiActorGroup.GracefulStop(TimeSpan.FromSeconds(30));
                    _apiActors = null;
                    _apiActorGroup = null;
                }

                #endregion

                _taskCancellationTokenSource.Dispose();
                _taskCancellationTokenSource = null;
            }
        }

        private void Initialize(SellerCenterDataMessage data, IServiceScope scope)
        {
            _sellerCenterData = data;

            var maxConcurrency = 1;
            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            _configuration = scope.ServiceProvider.GetService<IConfiguration>();
            var resources = _configuration.GetSection("SellerCenter").Get<SellerApiAdresses>();
            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);


            #region Actors

            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(SellerCenterApiActor.Props(_serviceProvider, _webJobCancellationToken, _sellerCenterData,
                                    resources, new Credentials { Username = data.Username, Password = data.Password, TenantId = data.TenantId })));


            if (_sellerCenterData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                    .Props(SellerCenterProductActor.Props(_serviceProvider, _webJobCancellationToken, _sellerCenterData, _apiActorGroup)));
            }

            if (_sellerCenterData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(SellerCenterOrderActor.Props(_serviceProvider, _webJobCancellationToken, _sellerCenterData, _apiActorGroup)));
            }

            #endregion


            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };

            if (_sellerCenterData.ProductIntegrationStatus == true)
            {
                //CreateProduct
                _queues.CreateProductQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.CreateProductQueue);
                _queues.CreateProductQueue.RegisterMessageHandler(CreateProductMessageAsync, messageHandlerOptions);

                _queues.ProcessVariationOptionsProductQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ProcessVariationOptionsProductQueue);
                _queues.ProcessVariationOptionsProductQueue.RegisterMessageHandler(ProcessVariationOptionsProducMessageAsync, messageHandlerOptions);

                _queues.ProcessCategoriesProductQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ProcessCategoriesProductQueue);
                _queues.ProcessCategoriesProductQueue.RegisterMessageHandler(ProcessCategoriesProductMessageAsync, messageHandlerOptions);

                _queues.ProcessManufacturersProductQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ProcessManufacturersProductQueue);
                _queues.ProcessManufacturersProductQueue.RegisterMessageHandler(ProcessManufacturersProductMessageAsync, messageHandlerOptions);

                _queues.UpdatePriceQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdatePriceQueue);
                _queues.UpdatePriceQueue.RegisterMessageHandler(UpdatePriceProductMessageAsync, messageHandlerOptions);

                _queues.UpdateStockProductQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdateStockProductQueue);
                _queues.UpdateStockProductQueue.RegisterMessageHandler(UpdateStockProductMessageAsync, messageHandlerOptions);
            }

            if (_sellerCenterData.OrderIntegrationStatus == true)
            {
                _queues.ProcessOrderQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ProcessOrderQueue);
                _queues.ProcessOrderQueue.RegisterMessageHandler(ProcessOrderMessageAsync, messageHandlerOptions);

                _queues.UpdateStatusOrderQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdateStatusOrderQueue);
                _queues.UpdateStatusOrderQueue.RegisterMessageHandler(UpdateStatusOrderMessageAsync, messageHandlerOptions);

                _queues.ListOrderQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ListOrderQueue);
                _queues.ListOrderQueue.RegisterMessageHandler(ListOrderMessageAsync, messageHandlerOptions);

                _queues.ListNewOrdersQueue = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ListNewOrdersQueue);
                _queues.ListNewOrdersQueue.RegisterMessageHandler(ListNewOrderMessageAsync, messageHandlerOptions);


                _queues.UpdatePartialOrderSeller = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdatePartialOrderSeller);
                _queues.UpdatePartialOrderSeller.RegisterMessageHandler(UpdatePartialOrderSellerMessageAsync, messageHandlerOptions);

                _queues.UpdateOrderSellerDeliveryPackage = _tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdateOrderSellerDeliveryPackage);
                _queues.UpdateOrderSellerDeliveryPackage.RegisterMessageHandler(UpdateOrderSellerDeliveryPackageMessageAsync, messageHandlerOptions);

                if (_sellerCenterData.ErpType != TenantType.Bling)
                    _listNewOrdersTask = ListNewOrdersTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["MaxOrderSleep"]));
            }

            #endregion
        }

        #region Task
        private async Task ListNewOrdersTaskAsync(int maxSleep)
        {
            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await _orderActor.Ask<ReturnMessage>(new ListNewOrdersMessage(), _webJobCancellationToken);
                await Task.Delay(maxSleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { }); //ignore exception
            }
        }
        #endregion

        #region Product
        private async Task CreateProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.CreateProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor - Error in CreateProductMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"CreateProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.CreateProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        #endregion

        #region VariationOptions
        private async Task ProcessVariationOptionsProducMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {

                var messageValue = new ServiceBusMessage(message.Body).GetValue<ProcessVariationOptionsMessage>();
                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.ProcessVariationOptionsProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in ProcessVariationOptionsProducMessageAsync", "SellerCenterTenantActor - ", $"ProcessVariationOptionsProducMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}",
                            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message,
                            null, $"ProcessVariationOptionsProducMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));

                    await _queues.ProcessVariationOptionsProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }


        #endregion

        #region Categories
        private async Task ProcessCategoriesProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {

                var messageValue = new ServiceBusMessage(message.Body).GetValue<ProcessCategoriesProductMessage>();
                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.ProcessCategoriesProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in ProcessCategoriesProductMessageAsync", $"ProcessCategoriesProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}",
                            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message,
                            null, $"ProcessCategoriesProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));

                    await _queues.ProcessCategoriesProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }
        #endregion

        #region Manufacturers
        private async Task ProcessManufacturersProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {

                var messageValue = new ServiceBusMessage(message.Body).GetValue<ProcessManufacturersMessage>();
                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.ProcessManufacturersProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in ProcessManufacturersProductMessageAsync", $"ProcessManufacturersProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}",
                            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message,
                            null, $"ProcessManufacturersProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));

                    await _queues.ProcessManufacturersProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        #endregion
        private async Task UpdatePriceProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {

                var messageValue = new ServiceBusMessage(message.Body).GetValue<SellerCenterUpdatePriceAndStockMessage>();
                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.UpdatePriceQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in UpdatePriceProductMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"UpdatePriceProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.UpdatePriceQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task UpdateStockProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {

                var messageValue = new ServiceBusMessage(message.Body).GetValue<SellerCenterUpdateStockProductMessage>();
                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.UpdateStockProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in UpdateStockProductMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"UpdateStockProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.UpdateStockProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        #region Orders
        private async Task ProcessOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ProcessOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.ProcessOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in ProcessOrderMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"ProcessOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.ProcessOrderQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task UpdateStatusOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<UpdateOrderStatusMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.UpdateStatusOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in UpdateStatusOrderMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"UpdateStatusOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.UpdateStatusOrderQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task UpdatePartialOrderSellerMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<UpdatePartialOrderSellerMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.UpdatePartialOrderSeller.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in UpdatePartialOrderSellerMessageAsync", $"UpdateStatusOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}",
                         LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    await _queues.UpdatePartialOrderSeller.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task UpdateOrderSellerDeliveryPackageMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<UpdateOrderSellerDeliveryPackageMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.UpdateOrderSellerDeliveryPackage.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in UpdateOrderSellerDeliveryPackageMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"UpdateOrderSellerDeliveryPackageMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.UpdateOrderSellerDeliveryPackage.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ListOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ListOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.ListOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in ListOrderMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"ListOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.ListOrderQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ListNewOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ListOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.ListOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "SellerCenterTenantActor -  Error in ListOrderMessageAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"ListOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await _queues.ListOrderQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        #endregion
        private async Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError("Error in SellerCenter Queue", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), arg, null, "Error in SellerCenter Queue"));
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

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            => Akka.Actor.Props.Create(() => new SellerCenterTenantActor(serviceProvider, cancellationToken));
    }
}
