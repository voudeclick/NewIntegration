using Akka.Actor;
using Akka.Event;
using AutoMapper;
using Samurai.Integration.APIClient.AliExpress.Models.Request;
using Samurai.Integration.APIClient.AliExpress.Models.Response;
using Samurai.Integration.APIClient.Tray.Models.Requests.Order;
using Samurai.Integration.APIClient.Tray.Models.Response.Order;
using Samurai.Integration.Domain.Enums.AliExpress;
using Samurai.Integration.Domain.Enums.Tray;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress;
using Samurai.Integration.Domain.Messages.AliExpress.Order;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Messages.Tray.OrderActor;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public class AliExpressService
    {
        private ILoggingAdapter _logger;
        private readonly IMapper _mapper;

        private IActorRef _apiActorGroup;

        public AliExpressService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter logger)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;

        }

        public async Task<ReturnMessage<List<TrayAppReturnMessage>>> ProcessOrder(AliExpressGetOrderMessage message, TrayQueue.Queues queues, CancellationToken cancellationToken)
        {
            try
            {
                var updateStatusOrderMessage = new TrayUpdateOrderStatusMessage()
                {
                    StoreId = message.StoreId,
                    TrayOrderId = message.TrayId,
                    HasProductVirtual = message.HasProductVirtual,
                    CancelStatusId = message.CancelStatusId,
                    SendStatusId = message.SendStatusId,
                    DeliveryStatusId = message.DeliveryStatusId,
                    AliExpressOrders = new List<TrayUpdateOrderStatusMessage.AliExpressOrder>()
                };

                foreach (var order in message.AliExpressOrdersIds)
                {
                    try
                    {
                        var orderAliExpress = await _apiActorGroup.Ask<ReturnMessage<AliExpressOrderResponse>>(new AliExpressOrderRequest
                        {
                            AliExpressOrderId = order.AliExpressOrderId
                        }, cancellationToken);

                        if (orderAliExpress.Result == Result.OK && orderAliExpress?.Data?.Order?.Result != null)
                        {
                            var orderAliExpressResult = orderAliExpress?.Data?.Order?.Result;

                            if (orderAliExpressResult.OrderStatus == AliExpressOrderStatusEnum.PLACE_ORDER_SUCCESS.ToString())
                            {
                                var trayAppReturnMessage = new TrayAppReturnMessage()
                                {
                                    Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                                    Success = true,
                                    StoreId = message.StoreId,
                                    Order = new TrayAppReturnMessage.OrderIntegration()
                                    {
                                        AliExpressOrderId = order.AliExpressOrderId.ToString(),
                                        CurrencyCode = orderAliExpressResult.Amount.CurrencyCode,
                                        AmountPaid = orderAliExpressResult.Amount.Amount,
                                        Items = orderAliExpressResult.Items.Products.Select(x => new TrayAppReturnMessage.OrderIntegration.OrderAliExpressItems()
                                        {
                                            ProductId = x.ProductId,
                                            Price = x.Price.Amount
                                        }).ToList(),
                                        Status = TrayOrderStatusEnum.AguardandoPagamento.ToString()
                                    }
                                };

                                await queues.SendEncryptedMessage((queues.TrayAppReturnMessage, trayAppReturnMessage, true));

                                updateStatusOrderMessage.AliExpressOrders.Add(new TrayUpdateOrderStatusMessage.AliExpressOrder()
                                {
                                    Id = order.AliExpressOrderId,
                                    TrayStatus = trayAppReturnMessage.Order.Status
                                });
                            }
                            else if (orderAliExpressResult.OrderStatus == AliExpressOrderStatusEnum.WAIT_SELLER_SEND_GOODS.ToString())
                            {
                                var trayAppReturnMessage = new TrayAppReturnMessage()
                                {
                                    Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                                    Success = true,
                                    StoreId = message.StoreId,
                                    Order = new TrayAppReturnMessage.OrderIntegration()
                                    {
                                        AliExpressOrderId = order.AliExpressOrderId.ToString(),
                                        CurrencyCode = orderAliExpressResult.Amount.CurrencyCode,
                                        AmountPaid = orderAliExpressResult.Amount.Amount,
                                        Items = orderAliExpressResult.Items.Products.Select(x => new TrayAppReturnMessage.OrderIntegration.OrderAliExpressItems()
                                        {
                                            ProductId = x.ProductId,
                                            Price = x.Price.Amount
                                        }).ToList(),
                                        Status = TrayOrderStatusEnum.PedidoPagoAguardandoEnvio.ToString()
                                    }
                                };

                                await queues.SendEncryptedMessage((queues.TrayAppReturnMessage, trayAppReturnMessage, true));

                                updateStatusOrderMessage.AliExpressOrders.Add(new TrayUpdateOrderStatusMessage.AliExpressOrder()
                                {
                                    Id = order.AliExpressOrderId,
                                    TrayStatus = trayAppReturnMessage.Order.Status
                                });
                            }
                            else if ((orderAliExpressResult.LogisticsStatus == AliExpressOrderLogisticStatusEnum.NO_LOGISTICS.ToString() &&
                                      orderAliExpressResult.OrderStatus == AliExpressOrderStatusEnum.FINISH.ToString()) ||
                                      orderAliExpressResult.OrderStatus == AliExpressOrderStatusEnum.IN_CANCEL.ToString())
                            {

                                var trayAppReturnMessage = new TrayAppReturnMessage()
                                {
                                    Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                                    Success = true,
                                    StoreId = message.StoreId,
                                    Order = new TrayAppReturnMessage.OrderIntegration()
                                    {
                                        AliExpressOrderId = order.AliExpressOrderId.ToString(),
                                        Status = TrayOrderStatusEnum.ParcialmenteCancelado.ToString(),
                                        CurrencyCode = orderAliExpressResult.Amount.CurrencyCode,
                                        AmountPaid = orderAliExpressResult.Amount.Amount
                                    }
                                };

                                await queues.SendEncryptedMessage((queues.TrayAppReturnMessage, trayAppReturnMessage, true));

                                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);

                                var products = orderAliExpressResult.Items.Products.Select(x => x.ProductId).Distinct().ToList();

                                var messageCancel = $"Atenção: Recebemos o status de cancelamento da AliExpress no dia {now:dd/MM/yyyy} do pedido {order.AliExpressOrderId} | Produto(s) {string.Join(", ", products)}.";

                                updateStatusOrderMessage.AliExpressOrders.Add(new TrayUpdateOrderStatusMessage.AliExpressOrder()
                                {
                                    Id = order.AliExpressOrderId,
                                    TrayStatus = trayAppReturnMessage.Order.Status,
                                    OrderCancellation = new TrayUpdateOrderStatusMessage.TrayOrderCancellation()
                                    {
                                        StoreNote = $"<br/><span style='color:red'>{messageCancel}</span>",
                                        Products = string.Join(", ", products)
                                    }
                                });
                            }
                            else if (orderAliExpressResult.LogisticsStatus == AliExpressOrderLogisticStatusEnum.BUYER_ACCEPT_GOODS.ToString() &&
                                     orderAliExpressResult.OrderStatus == AliExpressOrderStatusEnum.FINISH.ToString())
                            {
                                var trayAppReturnMessage = new TrayAppReturnMessage()
                                {
                                    Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                                    Success = true,
                                    StoreId = message.StoreId,
                                    Order = new TrayAppReturnMessage.OrderIntegration()
                                    {
                                        AliExpressOrderId = order.AliExpressOrderId.ToString(),
                                        Status = TrayOrderStatusEnum.PedidoEntregue.ToString(),
                                        CurrencyCode = orderAliExpressResult.Amount.CurrencyCode,
                                        AmountPaid = orderAliExpressResult.Amount.Amount
                                    }
                                };

                                await queues.SendEncryptedMessage((queues.TrayAppReturnMessage, trayAppReturnMessage, true));

                                updateStatusOrderMessage.AliExpressOrders.Add(new TrayUpdateOrderStatusMessage.AliExpressOrder()
                                {
                                    Id = order.AliExpressOrderId,
                                    TrayStatus = trayAppReturnMessage.Order.Status
                                });
                            }
                            else if (orderAliExpressResult.OrderStatus == AliExpressOrderStatusEnum.WAIT_BUYER_ACCEPT_GOODS.ToString() &&
                                     orderAliExpressResult.LogisticsStatus == AliExpressOrderLogisticStatusEnum.SELLER_SEND_GOODS.ToString())
                            {
                                var logisticServiceName = orderAliExpressResult.Logistic?.OrderLogistic.Select(x => x.ServiceName).ToList();
                                var trackingCode = orderAliExpressResult.Logistic?.OrderLogistic.Select(x => x.TrackingCode).ToList();

                                var trayAppReturnMessage = new TrayAppReturnMessage()
                                {
                                    Type = TrayAppReturnMessageTypeEnum.Order.ToString(),
                                    Success = true,
                                    StoreId = message.StoreId,
                                    Order = new TrayAppReturnMessage.OrderIntegration()
                                    {
                                        AliExpressOrderId = order.AliExpressOrderId.ToString(),
                                        TrackingCode = string.Join(",", trackingCode),
                                        LogisticServiceName = logisticServiceName != null ? string.Join(",", logisticServiceName) : string.Empty,
                                        CurrencyCode = orderAliExpressResult.Amount.CurrencyCode,
                                        AmountPaid = orderAliExpressResult.Amount.Amount,
                                        Items = orderAliExpressResult.Items.Products.Select(x => new TrayAppReturnMessage.OrderIntegration.OrderAliExpressItems()
                                        {
                                            ProductId = x.ProductId,
                                            Price = x.Price.Amount
                                        }).ToList(),
                                        Status = TrayOrderStatusEnum.PedidoEnviado.ToString()
                                    }
                                };

                                await queues.SendEncryptedMessage((queues.TrayAppReturnMessage, trayAppReturnMessage, true));

                                updateStatusOrderMessage.AliExpressOrders.Add(new TrayUpdateOrderStatusMessage.AliExpressOrder()
                                {
                                    Id = order.AliExpressOrderId,
                                    TrayStatus = trayAppReturnMessage.Order.Status,
                                    OrderTracking = new TrayUpdateOrderStatusMessage.TrayOrderTracking()
                                    {
                                        LogisticServiceName = trayAppReturnMessage.Order.LogisticServiceName,
                                        TrackingCode = trayAppReturnMessage.Order.TrackingCode
                                    }
                                });
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (updateStatusOrderMessage.AliExpressOrders != null && updateStatusOrderMessage.AliExpressOrders.Count() > 0)
                    await queues.SendMessages((queues.UpdateStatusOrderQueue, updateStatusOrderMessage, true));

                return new ReturnMessage<List<TrayAppReturnMessage>>() { Result = Result.OK, Data = new List<TrayAppReturnMessage>() };
            }
            catch (Exception ex)
            {
                return new ReturnMessage<List<TrayAppReturnMessage>>() { Result = Result.Error, Error = ex };
            }
        }
    }
}
