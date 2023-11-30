using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.Configuration;
using Samurai.Integration.APIClient.AliExpress.Models.Response;
using Samurai.Integration.APIClient.Tray.Models.Requests.Customer;
using Samurai.Integration.APIClient.Tray.Models.Requests.Order;
using Samurai.Integration.APIClient.Tray.Models.Requests.Product;
using Samurai.Integration.APIClient.Tray.Models.Response.Customer;
using Samurai.Integration.APIClient.Tray.Models.Response.Order;
using Samurai.Integration.APIClient.Tray.Models.Response.Product;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Enums.AliExpress;
using Samurai.Integration.Domain.Enums.Tray;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress.Order;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Messages.Tray.OrderActor;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public partial class TrayService
    {
        public class TrayOrderService
        {
            private ILoggingAdapter _logger;
            private IActorRef _apiActorGroup;
            private readonly IConfiguration _configuration;
            private readonly TenantRepository _tenantRepository;

            public TrayOrderService(IConfiguration configuration, TenantRepository repository)
            {
                _configuration = configuration;
                _tenantRepository = repository;
            }

            public void Init(IActorRef apiActorGroup, ILoggingAdapter logger) { _apiActorGroup = apiActorGroup; _logger = logger; }

            public async Task<ReturnMessage<TrayAppReturnMessage>> CancelOrder(TrayUpdateOrderStatusMessage message, string aliExpressOrderId, CancellationToken cancellationToken)
            {
                async Task<ReturnMessage<UpdateOrderResponse>> apiUpdateOrder(UpdateOrderRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdateOrderResponse>>(request, cancellationToken);

                async Task<ReturnMessage<GetOrderCompleteByIdResponse>> apiGetOrderById(long orderId)
                => await _apiActorGroup.Ask<ReturnMessage<GetOrderCompleteByIdResponse>>(new GetOrderCompleteByIdRequest { OrderId = orderId }, cancellationToken);

                var orderCancellation = message.AliExpressOrders.Where(x => x.Id.ToString() == aliExpressOrderId).FirstOrDefault();

                var trayAppReturnMessage = new TrayAppReturnMessage()
                {
                    Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                    Success = true,
                    StoreId = message.StoreId,
                    Order = new TrayAppReturnMessage.OrderIntegration()
                    {
                        AliExpressOrderId = orderCancellation.Id.ToString(),
                        Status = orderCancellation.TrayStatus
                    }
                };

                try
                {
                    var response = await apiGetOrderById(message.TrayOrderId);

                    var trayOrder = response?.Data?.Order;
                    if (trayOrder != null && !trayOrder.StoreNote.Contains(orderCancellation.OrderCancellation.Products))
                    {
                        var request = new UpdateOrderRequest()
                        {
                            OrderId = message.TrayOrderId,
                            OrderCancel = new TrayOrderCancelUpdate()
                            {
                                StoreNote = !string.IsNullOrEmpty(orderCancellation.OrderCancellation.StoreNote) && !trayOrder.StoreNote.Contains(orderCancellation.OrderCancellation.StoreNote) ?
                                trayOrder.StoreNote += orderCancellation.OrderCancellation.StoreNote : trayOrder.StoreNote
                            }
                        };

                        var updateOrder = await apiUpdateOrder(request);

                        if (updateOrder.Result == Result.Error || (updateOrder.Data != null && updateOrder.Data.Code != 200))
                        {
                            trayAppReturnMessage.Success = false;
                            trayAppReturnMessage.Message = updateOrder.Error.Message;
                        }
                    }
                }
                catch (Exception ex)
                {
                    trayAppReturnMessage.Success = false;
                    trayAppReturnMessage.Message = $"Falha ao cancelar parcialmente o pedido {message.TrayOrderId} na Tray. {ex.Message}";

                    _logger.Warning($"TrayOrderService - Error in CancelOrder {message.TrayOrderId} | {JsonSerializer.Serialize(ex)}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));

                }

                return new ReturnMessage<TrayAppReturnMessage>() { Result = Result.OK, Data = trayAppReturnMessage };

            }

            public async Task<ReturnMessage<List<TrayAppReturnMessage>>> UpdateOrderStatus(TrayUpdateOrderStatusMessage message, CancellationToken cancellationToken)
            {
                var result = new List<TrayAppReturnMessage>();

                async Task<ReturnMessage<UpdateOrderResponse>> apiUpdateOrder(UpdateOrderRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdateOrderResponse>>(request, cancellationToken);

                async Task<ReturnMessage<UpdateOrderResponse>> apiOrderCancel(TrayOrderCancelRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdateOrderResponse>>(request, cancellationToken);

                async Task<ReturnMessage<GetOrderCompleteByIdResponse>> apiGetOrderById(long orderId)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderCompleteByIdResponse>>(new GetOrderCompleteByIdRequest { OrderId = orderId }, cancellationToken);

                var response = await apiGetOrderById(message.TrayOrderId);

                var trayOrder = response?.Data?.Order;

                //se todos os subpedidos foram parcialmente cancelados
                if (message.AliExpressOrders.Count(x => x.TrayStatus == TrayOrderStatusEnum.ParcialmenteCancelado.ToString()) == message.AliExpressOrders.Count())
                {
                    var returnMessage = new TrayAppReturnMessage()
                    {
                        Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                        Success = true,
                        StoreId = message.StoreId,
                        Order = new TrayAppReturnMessage.OrderIntegration()
                        {
                            TrayOrderId = message.TrayOrderId,
                            Status = TrayOrderStatusEnum.PedidoCancelado.ToString()
                        }
                    };

                    try
                    {
                        var updateStatus = await apiOrderCancel(new TrayOrderCancelRequest()
                        {
                            OrderId = message.TrayOrderId
                        });

                        if (updateStatus.Data == null || updateStatus.Result == Result.Error)
                            returnMessage.Message = updateStatus.Error.Message;
                        else
                            returnMessage.Message = updateStatus.Data.Message;

                        result.Add(returnMessage);
                    }
                    catch (Exception ex)
                    {
                        returnMessage.Message = $"Falha ao cancelar pedido {message.TrayOrderId} na Tray. {ex.Message}";
                        _logger.Error($"TrayOrderService - Error in ProcessUpdateOrderTracking {message.TrayOrderId} | {JsonSerializer.Serialize(ex)}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    }
                }
                else
                {
                    foreach (var order in message.AliExpressOrders)
                    {
                        var returnMessage = new TrayAppReturnMessage()
                        {
                            Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                            Success = true,
                            StoreId = message.StoreId,
                            Order = new TrayAppReturnMessage.OrderIntegration()
                            {
                                AliExpressOrderId = order.Id.ToString()
                            }
                        };

                        if (order.TrayStatus == TrayOrderStatusEnum.PedidoEnviado.ToString())
                        {
                            if (trayOrder != null)
                            {
                                try
                                {
                                    var orderTracking = order.OrderTracking;
                                    var sendingCode = string.Empty;

                                    if (!string.IsNullOrEmpty(orderTracking.TrackingCode))
                                        sendingCode = orderTracking.TrackingCode;

                                    if (!string.IsNullOrEmpty(sendingCode))
                                    {
                                        if (!trayOrder.SendingCode.Contains(orderTracking.TrackingCode))
                                        {
                                            sendingCode = trayOrder.SendingCode + ", " + orderTracking.TrackingCode;

                                            returnMessage.Order.TrackingCode = sendingCode;
                                            returnMessage.Order.LogisticServiceName = orderTracking.LogisticServiceName;
                                            returnMessage.Order.Status = order.TrayStatus;

                                            var storeNote = $" - Código(s) de Rastreio: {sendingCode}";

                                            if (trayOrder.StoreNote == null || (trayOrder.StoreNote != null && !trayOrder.StoreNote.Contains(storeNote)))
                                                trayOrder.StoreNote += storeNote;

                                            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                                            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);

                                            if (trayOrder.SendingCode != sendingCode || trayOrder.SendingDate != now.Date.ToString())
                                            {
                                                var updateOrderStatus = await apiUpdateOrder(new UpdateOrderRequest()
                                                {
                                                    OrderId = long.Parse(trayOrder.Id),
                                                    Order = new TrayOrderUpdate()
                                                    {
                                                        SendingCode = sendingCode,
                                                        StoreNote = trayOrder.StoreNote,
                                                        SendingDate = now.Date.ToString("yyyy-MM-dd")
                                                    }
                                                });

                                                if (updateOrderStatus != null && updateOrderStatus.Result == Result.Error)
                                                {
                                                    returnMessage.Success = false;
                                                    returnMessage.Message = updateOrderStatus.Error.Message;

                                                    result.Add(returnMessage);

                                                    continue;
                                                }
                                            }
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    returnMessage.Success = false;
                                    returnMessage.Message = $"Falha ao atualizar status do pedido {order.Id} para Enviado. {ex.Message}";

                                    result.Add(returnMessage);
                                    _logger.Error($"TrayOrderService - Error in ProcessUpdateOrderTracking (PedidoEnviado) {message.TrayOrderId} | {JsonSerializer.Serialize(ex)}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));

                                    continue;
                                }


                                result.Add(returnMessage);
                            }
                        }
                        else if (order.TrayStatus == TrayOrderStatusEnum.ParcialmenteCancelado.ToString())
                        {
                            await CancelOrder(message, order.Id.ToString(), cancellationToken);

                            returnMessage.Order.Status = order.TrayStatus;
                            result.Add(returnMessage);
                        }
                    }

                    if (!message.HasProductVirtual)
                    {
                        try
                        {
                            var updateStatus = await UpdateStatusOrderTracking(message, trayOrder?.OrderStatus?.Id.Trim(), cancellationToken);
                            if (updateStatus != null && (updateStatus.Data == null || updateStatus.Result == Result.Error))
                            {

                                result.Add(new TrayAppReturnMessage()
                                {
                                    Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                                    Success = false,
                                    Message = updateStatus.Error.Message,
                                    StoreId = message.StoreId,
                                    Order = new TrayAppReturnMessage.OrderIntegration()
                                    {
                                        TrayOrderId = message.TrayOrderId
                                    }
                                });
                            }

                        }
                        catch (Exception ex)
                        {
                            result.Add(new TrayAppReturnMessage()
                            {
                                Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                                Success = false,
                                Message = $"Falha ao atualizar status do pedido {message.TrayOrderId} na Tray. {ex.Message}",
                                StoreId = message.StoreId,
                                Order = new TrayAppReturnMessage.OrderIntegration()
                                {
                                    TrayOrderId = message.TrayOrderId
                                }
                            });

                            _logger.Error($"TrayOrderService - Error in ProcessUpdateOrderTracking (HasProductVirtual) {message.TrayOrderId} | {JsonSerializer.Serialize(ex)}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));

                        }

                    }
                }

                return new ReturnMessage<List<TrayAppReturnMessage>>
                {
                    Result = Result.OK,
                    Data = result.Distinct().ToList()
                };


            }

            public async Task<ReturnMessage<UpdateOrderResponse>> UpdateStatusOrderTracking(TrayUpdateOrderStatusMessage message, string trayOrderStatusId, CancellationToken cancellationToken)
            {
                async Task<ReturnMessage<UpdateOrderResponse>> apiUpdateOrder(UpdateOrderRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdateOrderResponse>>(request, cancellationToken);

                //Atualizar o status na tray:

                //se todos os subpedidos foram enviados
                if (message.AliExpressOrders.Count(x => x.TrayStatus == TrayOrderStatusEnum.PedidoEnviado.ToString()) == message.AliExpressOrders.Count())
                {
                    if (!string.IsNullOrEmpty(trayOrderStatusId) && trayOrderStatusId != message.SendStatusId.ToString())
                    {
                        return await apiUpdateOrder(new UpdateOrderRequest()
                        {
                            OrderId = message.TrayOrderId,
                            OrderStatus = new TrayOrderStatusUpdate() { StatusId = message.SendStatusId }
                        });
                    }
                }
                //se todos os subpedidos foram entregues
                else if (message.AliExpressOrders.Count(x => x.TrayStatus == TrayOrderStatusEnum.PedidoEntregue.ToString()) == message.AliExpressOrders.Count())
                {
                    if (!string.IsNullOrEmpty(trayOrderStatusId) && trayOrderStatusId != message.DeliveryStatusId.ToString())
                    {
                        return await apiUpdateOrder(new UpdateOrderRequest()
                        {
                            OrderId = message.TrayOrderId,
                            OrderStatus = new TrayOrderStatusUpdate() { StatusId = message.DeliveryStatusId }
                        });
                    }
                }

                return null;
            }
        }
    }
}
