using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.PluggTo.Enum;
using Samurai.Integration.APIClient.PluggTo.Models.Requests;
using Samurai.Integration.APIClient.PluggTo.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Samurai.Integration.Domain.Messages.PluggTo.PluggToData;

namespace Samurai.Integration.Application.Actors.PluggTo.SellerCenter
{
    public class SellerCenterPluggToOrderActor : BasePluggToTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActorRef _apiActorGroup;
        private readonly CancellationToken _cancellationToken;

        private readonly QueueClient _erpUpdatePartialOrderSellerQueueClient;
        private readonly QueueClient _updateOrderSellerDeliveryPackageQueueClient;

        public SellerCenterPluggToOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData, IActorRef apiActorGroup)
          : base("SellerCenterPluggToOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _pluggToData = pluggToData;
            _apiActorGroup = apiActorGroup;


            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _erpUpdatePartialOrderSellerQueueClient = tenantService.GetQueueClient(_pluggToData, SellerCenterQueue.UpdatePartialOrderSeller);
                _updateOrderSellerDeliveryPackageQueueClient = tenantService.GetQueueClient(_pluggToData, SellerCenterQueue.UpdateOrderSellerDeliveryPackage);
                             
            }

            ReceiveAsync((Func<PluggToListOrderMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting PluggToListOrderMessage id: {message.ExternalOrderId}");

                    ReturnMessage<PluggToApiOrderResult> result;

                    if (!string.IsNullOrEmpty(message.Order))
                    {
                        var order = JsonConvert.DeserializeObject<PluggToApiOrderResult>(message.Order);
                        result = new ReturnMessage<PluggToApiOrderResult>() { Data = order, Result = Result.OK };
                    }
                    else
                        result = await _apiActorGroup.Ask<ReturnMessage<PluggToApiOrderResult>>(new PluggToApiListOrderRequest { ExternalOrderId = message.ExternalOrderId }, cancellationToken);

                    var pluggToOrder = result.Data?.Order;

                    if (pluggToOrder != null)
                    {
                        var statusMapping = _pluggToData.OrderStatusMapping?.FirstOrDefault(x => x.PluggToSituacaoNome.Equals(pluggToOrder.status, StringComparison.OrdinalIgnoreCase));

                        if (statusMapping != null)
                        {
                            var serviceBusMessage = new ServiceBusMessage(new UpdatePartialOrderSellerMessage
                            {
                                OrderId = Guid.Empty,
                                OrderSellerId = Guid.Empty,
                                OrderClientId = pluggToOrder.id,
                                StatusId = statusMapping.ERPStatusId
                            });

                            await _erpUpdatePartialOrderSellerQueueClient.SendAsync(serviceBusMessage.GetMessage(message.ExternalOrderId));

                            await UpdateOrderSellerDeliveryPackage(pluggToOrder);
                        }
                    }

                    LogDebug("Ending ListOrderMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in ListOrderMessage");
                    Sender.Tell(new ReturnMessage<PluggToApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ProcessOrderMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ProcessOrderMessage id: {message.Id}");

                    ReturnMessage<PluggToApiOrderResult> result;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderSeller = message.OrderSellers.FirstOrDefault(x => x.SellerId.ToString().ToUpper() == message.SITenantSellerId.ToUpper());

                        if (orderSeller != null)
                        {
                            var statusMapping = _pluggToData.OrderStatusMapping?.FirstOrDefault(x => x.ERPStatusId.ToString().ToUpper() == orderSeller.StatusId.ToString().ToUpper());
                            if (statusMapping == null)
                                result = new ReturnMessage<PluggToApiOrderResult> { Result = Result.OK };
                            else
                            {
                                if (orderSeller.ClientId == null)
                                {
                                    result = await CreatePluggToOrder(message, orderSeller, statusMapping);
                                }
                                else
                                {
                                    result = await _apiActorGroup.Ask<ReturnMessage<PluggToApiOrderResult>>(new PluggToApiListOrderRequest { ExternalOrderId = orderSeller.ClientId }, cancellationToken);

                                    var pluggToOrder = result.Data?.Order;

                                    if (pluggToOrder != null)
                                    {
                                        if (statusMapping == null || statusMapping?.PluggToSituacaoNome?.Equals(pluggToOrder.status, StringComparison.OrdinalIgnoreCase) == true)
                                        {
                                            result = new ReturnMessage<PluggToApiOrderResult> { Result = Result.OK };
                                        }
                                        else
                                        {
                                            var orderUpdateRequest = PluggToApiUpdateOrderRequest.From(orderSeller, statusMapping);
                                            result = await _apiActorGroup.Ask<ReturnMessage<PluggToApiOrderResult>>(orderUpdateRequest, cancellationToken);
                                        }
                                    }
                                    else
                                    {
                                        result = await CreatePluggToOrder(message, orderSeller, statusMapping);
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = new ReturnMessage<PluggToApiOrderResult> { Result = Result.OK };
                        }
                    }

                    LogDebug("Ending ProcessOrderMessage");

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in ProcessOrderMessage");
                    Sender.Tell(new ReturnMessage<PluggToApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));
        }


        private async Task<ReturnMessage<PluggToApiOrderResult>> CreatePluggToOrder(ProcessOrderMessage message, OrderSellerDto orderSeller, PluggToDataOrderStatusMapping statusMapping)
        {
            var orderCreateRequest = PluggToApiCreateOrderRequest.From(message, orderSeller, _pluggToData, statusMapping.PluggToSituacaoNome);

            var ret = await _apiActorGroup.Ask<ReturnMessage<PluggToApiOrderResult>>(orderCreateRequest, _cancellationToken);
           
            if (ret.Error != null)
                return new ReturnMessage<PluggToApiOrderResult> { Result = Result.Error, Error = new Exception(ret.Error.Message) };

            var pluggToOrder = ret.Data?.Order;
            if (pluggToOrder != null)
            {
                var serviceBusMessage = new ServiceBusMessage(new UpdatePartialOrderSellerMessage
                {
                    OrderId = message.Id,
                    OrderSellerId = orderSeller.Id,
                    OrderClientId = pluggToOrder.id
                });

                await _erpUpdatePartialOrderSellerQueueClient.SendAsync(serviceBusMessage.GetMessage(message.Id));

                await UpdateOrderSellerDeliveryPackage(pluggToOrder);
            }

            return ret;
        }
        private async Task UpdateOrderSellerDeliveryPackage(OrderResult pluggToOrder)
        {
            if (pluggToOrder.shipments != null && (pluggToOrder.status == PluggToOrderStatus.invoiced.ToString() ||
                                                   pluggToOrder.status == PluggToOrderStatus.shipping_informed.ToString() ||
                                                   pluggToOrder.status == PluggToOrderStatus.shipped.ToString()))
            {
                List<Task> sendMessages = new List<Task>();

                foreach (var shipment in pluggToOrder.shipments)
                {
                    var deliveryTime = 0;

                    if (shipment.date_shipped != null && shipment.estimate_delivery_date != null)
                    {
                        var dateShipped = (DateTime)shipment.date_shipped;
                        var estimateDeliveryDate = (DateTime)shipment.estimate_delivery_date;

                        deliveryTime = estimateDeliveryDate.Subtract(dateShipped).Days;
                    }

                    var messageOrderSellerDelivery = new ServiceBusMessage(new UpdateOrderSellerDeliveryPackageMessage()
                    {
                        OrderId = Guid.Empty,
                        OrderSellerId = Guid.Empty,
                        OrderClientId = pluggToOrder.id,
                        TrackingCode = shipment.track_code,
                        TrackingPostageStatus = TrackingPostageStatus.Posted.ToString(),
                        TrackingUrl = shipment.track_url,
                        DeliveryTime = deliveryTime,
                        InvoiceDate = shipment.nfe_date,
                        InvoiceNumber = shipment.nfe_number,
                        InvoiceKey = shipment.nfe_key,
                        InvoiceLink = shipment.nfe_link,
                        DeliveryClientId = shipment.id
                    });

                    sendMessages.Add(_updateOrderSellerDeliveryPackageQueueClient.SendAsync(messageOrderSellerDelivery.GetMessage(Guid.NewGuid().ToString())));
                }

                await Task.WhenAll(sendMessages);
            }
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterPluggToOrderActor(serviceProvider, cancellationToken, pluggToData, apiActorGroup));
        }
    }
}
