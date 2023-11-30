using Akka.Actor;
using Akka.Event;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using Samurai.Integration.APIClient.SellerCenter.Models.Requests;
using Samurai.Integration.APIClient.SellerCenter.Models.Response;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Enums.SellerCenter;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Repositories;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public partial class SellerCenterService
    {
        public class OrderService
        {
            private ILoggingAdapter _logger;
            private IActorRef _apiActorGroup;
            private readonly IConfiguration _configuration;
            private readonly TenantRepository _tenantRepository;

            public OrderService(IConfiguration configuration, TenantRepository repository)
            {
                _configuration = configuration;
                _tenantRepository = repository;
            }

            public void Init(IActorRef apiActorGroup, ILoggingAdapter logger) { _apiActorGroup = apiActorGroup; _logger = logger; }

            public async Task<ReturnMessage> ListNewOrders(SellerCenterDataMessage sellerCenterData, SellerCenterQueue.Queues queues, CancellationToken cancellationToken = default)
            {
                async Task<ReturnMessage<GetOrderByFilterResponse>> apiGetOrders(GetOrderByFilterRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderByFilterResponse>>(request, cancellationToken);


                var tenant = await _tenantRepository.GetById(sellerCenterData.Id, cancellationToken);
                var transId = tenant.SellerCenterData.GetTransId(TransIdType.NovosPedidos);

                var response = await apiGetOrders(new GetOrderByFilterRequest { StartUpdateDate = transId.LastProcessedDate.DateTime, OrderBy = "UpdateDate", PageSize = 25 });

                if (response.Result == Result.Error) throw response.Error;

                if (response.Data?.Value?.Any() == false)
                {
                    _logger.Warning("OrderService - Error in ListNewOrders", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new GetOrderByFilterRequest { StartUpdateDate = transId.LastProcessedDate.DateTime, OrderBy = "UpdateDate", PageSize = 25 }, sellerCenterData));
                    return new ReturnMessage { Result = Result.Error };
                }

                var orders = response.Data.Value.Where(x => x.SystemStatusType != Domain.Models.SellerCenter.API.Enums.SystemStatus.Error && !string.IsNullOrWhiteSpace(x.Number)).OrderBy(x => x.UpdateDate).ToList();

                if (orders.Any())
                {
                    foreach (var order in orders)
                    {
                        var serviceBusMessage = new ServiceBusMessage(new ProcessOrderMessage(order, sellerCenterData));
                        await queues.ProcessOrderQueue.SendAsync(serviceBusMessage.GetMessage(order.Id));
                    }

                    var lastUpdateDateOrder = orders?.OrderByDescending(x => x.UpdateDate)?.FirstOrDefault()?.UpdateDate;

                    if (lastUpdateDateOrder.HasValue)
                    {
                        transId.LastProcessedDate = lastUpdateDateOrder.Value.ToUniversalTime().AddSeconds(1);
                        tenant.SellerCenterData.SetTransId(transId);
                        await _tenantRepository.CommitAsync(cancellationToken);
                    }
                }

                return new ReturnMessage { Result = Result.OK };
            }

            public async Task<ReturnMessage> UpdateOrderStatus(UpdateOrderStatusMessage message, CancellationToken cancellationToken)
            {

                var status = await _apiActorGroup.Ask<ReturnMessage<GetOrderStatusResponse>>(new GetOrderStatusRequest(), cancellationToken);

                async Task<ReturnMessage<GetOrderByFilterResponse>> apiGetOrders(GetOrderByFilterRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderByFilterResponse>>(request, cancellationToken);

                async Task<ReturnMessage<UpdatePartialOrderResponse>> apiUpdateStatusOrder(UpdatePartialOrderRequest request)
                   => await _apiActorGroup.Ask<ReturnMessage<UpdatePartialOrderResponse>>(request, cancellationToken);

                var response = await apiGetOrders(new GetOrderByFilterRequest { OrderNumber = message.OrderExternalId });

                if (response.Result == Result.Error)
                {
                    _logger.Warning("OrderService - Error in UpdateOrderStatus", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    return new ReturnMessage { Result = Result.Error, Error = new Exception($"not found order {message.OrderExternalId}", response.Error) };
                }

                var order = response.Data.Value.FirstOrDefault();

                if (response.Data.Value.Count <= 0)
                {
                    _logger.Info($"order {message.SellerCenterOrderId?.ToString() ?? message.OrderExternalId} not found");
                }
                else
                {
                    if (message.Cancellation.IsCancelled)
                    {

                        if (order.SystemStatusType != Domain.Models.SellerCenter.API.Enums.SystemStatus.Canceled)
                        {
                            var cancelStatusId = status.Data.GetStatusByLevelAndSystemStatus(GetOrderStatusResponse.StatusItem.StatusLevel.Order, Domain.Models.SellerCenter.API.Enums.SystemStatus.Canceled);

                            var orderCancelResult = await apiUpdateStatusOrder(new UpdatePartialOrderRequest { OrderId = order.Id, CancelId = cancelStatusId.Id, StatusId = cancelStatusId.Id });

                            if (orderCancelResult.Result == Result.Error)
                            {
                                _logger.Warning("OrderService - Error in UpdateOrderStatus", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                                return new ReturnMessage { Result = Result.Error, Error = orderCancelResult.Error };
                            }
                        }

                    }
                    else
                    {
                        if (order.SystemStatusType == Domain.Models.SellerCenter.API.Enums.SystemStatus.Canceled)
                        {
                            _logger.Info($"can't update because order {order.Id} is cancelled");
                        }
                        else
                        {
                            if (message.Payment.IsPaid)
                            {

                                if (order.SystemStatusType != Domain.Models.SellerCenter.API.Enums.SystemStatus.WaitingPayment)
                                {
                                    var cancelStatusId = status.Data.GetStatusByLevelAndSystemStatus(GetOrderStatusResponse.StatusItem.StatusLevel.Order, Domain.Models.SellerCenter.API.Enums.SystemStatus.Paid);

                                    var orderCancelResult = await apiUpdateStatusOrder(new UpdatePartialOrderRequest { OrderId = order.Id, CancelId = cancelStatusId.Id, StatusId = cancelStatusId.Id });

                                    if (orderCancelResult.Result == Result.Error)
                                    {
                                        _logger.Warning("OrderService - Error in UpdateOrderStatus", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                                        return new ReturnMessage { Result = Result.Error, Error = orderCancelResult.Error };
                                    }
                                }

                            }

                            if (message.Shipping.IsShipped)
                            {
                                if (order.SystemStatusType != Domain.Models.SellerCenter.API.Enums.SystemStatus.Sent)
                                {
                                    var cancelStatusId = status.Data.GetStatusByLevelAndSystemStatus(GetOrderStatusResponse.StatusItem.StatusLevel.Order, Domain.Models.SellerCenter.API.Enums.SystemStatus.Sent);

                                    var orderCancelResult = await apiUpdateStatusOrder(new UpdatePartialOrderRequest { OrderId = order.Id, CancelId = cancelStatusId.Id, StatusId = cancelStatusId.Id });

                                    if (orderCancelResult.Result == Result.Error)
                                    {
                                        _logger.Warning("OrderService - Error in UpdateOrderStatus", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                                        return new ReturnMessage { Result = Result.Error, Error = orderCancelResult.Error };
                                    }
                                }
                                else
                                {
                                    if (!message.Shipping.IsDelivered)
                                    {
                                        //nothing
                                        //if (response.Data.Value.FirstOrDefault().SystemStatusType != Domain.Models.SellerCenter.API.Enums.SystemStatus.Sent)
                                        //{
                                        //    var cancelStatusId = status.Data.GetStatusByLevelAndSystemStatus(GetOrderStatusResponse.StatusItem.StatusLevel.Order, Domain.Models.SellerCenter.API.Enums.SystemStatus.Sent);

                                        //    var orderCancelResult = await apiUpdateStatusOrder(new UpdateStatusOrderRequest { OrderId = response.Data.Value.FirstOrDefault().Id, CancelId = cancelStatusId.Id, StatusId = cancelStatusId.Id });

                                        //    if (orderCancelResult.Result == Result.Error)
                                        //        return new ReturnMessage { Result = Result.Error, Error = orderCancelResult.Error };

                                        //}
                                    }
                                }
                            }

                            if (message.Shipping.IsDelivered)
                            {
                                if (order.SystemStatusType != Domain.Models.SellerCenter.API.Enums.SystemStatus.Delivered)
                                {
                                    var cancelStatusId = status.Data.GetStatusByLevelAndSystemStatus(GetOrderStatusResponse.StatusItem.StatusLevel.Order, Domain.Models.SellerCenter.API.Enums.SystemStatus.Delivered);

                                    var orderCancelResult = await apiUpdateStatusOrder(new UpdatePartialOrderRequest { OrderId = order.Id, CancelId = cancelStatusId.Id, StatusId = cancelStatusId.Id });

                                    if (orderCancelResult.Result == Result.Error)
                                    {
                                        _logger.Warning("OrderService - Error in UpdateOrderStatus", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                                        return new ReturnMessage { Result = Result.Error, Error = orderCancelResult.Error };
                                    }
                                }
                            }
                        }
                    }
                }
                return new ReturnMessage { Result = Result.OK };
            }

            public async Task<ReturnMessage> UpdatePartialOrderSeller(SellerCenterDataMessage sellerCenterData, UpdatePartialOrderSellerMessage message, CancellationToken cancellationToken)
            {
                async Task<ReturnMessage<UpdatePartialOrderSellerResponse>> apiUpdatePartialOrderSeller(UpdatePartialOrderSellerRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdatePartialOrderSellerResponse>>(request, cancellationToken);

                async Task<ReturnMessage<GetOrderByIdResponse>> apiGetOrderById(GetOrderByIdRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderByIdResponse>>(request, cancellationToken);

                async Task<ReturnMessage<GetOrderByIdResponse>> apiGetOrderBySellerAndClientId(GetOrderBySellerAndClientIdRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderByIdResponse>>(request, cancellationToken);

                ReturnMessage<GetOrderByIdResponse> resultOrder;

                if (message.OrderId != Guid.Empty)
                    resultOrder = await apiGetOrderById(new GetOrderByIdRequest { OrderId = message.OrderId });
                else
                    resultOrder = await apiGetOrderBySellerAndClientId(new GetOrderBySellerAndClientIdRequest { SellerId = new Guid(sellerCenterData.SellerId), ClientId = message.OrderClientId });

                var order = resultOrder.Data?.Value;

                if (order == null)
                {
                    _logger.Warning("OrderService - Not found order", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, sellerCenterData));
                    return new ReturnMessage { Result = Result.Error, Error = new Exception($"not found order {JsonConvert.SerializeObject(message)}", resultOrder.Error) };
                }
                OrderSellerDto sellerOrder;

                if (message.OrderSellerId != Guid.Empty)
                    sellerOrder = order.OrderSellers?.FirstOrDefault(x => x.Id == message.OrderSellerId);
                else
                    sellerOrder = order?.OrderSellers?.FirstOrDefault(x => x.SellerId == new Guid(sellerCenterData.SellerId));

                if (sellerOrder == null)
                {
                    _logger.Warning($"OrderService - not found order seller {message.OrderSellerId}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, sellerCenterData));
                    return new ReturnMessage { Result = Result.Error, Error = new Exception($"not found order seller {message.OrderSellerId}") };
                }

                if (sellerOrder.ClientId == message.OrderClientId &&
                    sellerOrder.StatusId == message.StatusId &&
                    sellerOrder.Notes == message.Notes)
                    return new ReturnMessage { Result = Result.OK };

                var ret = await apiUpdatePartialOrderSeller(new UpdatePartialOrderSellerRequest
                {
                    OrderId = order.Id,
                    OrderSellerId = sellerOrder.Id,
                    OrderClientId = message.OrderClientId,
                    StatusId = message.StatusId,                    
                    Notes = string.Concat(sellerOrder.Notes, Environment.NewLine, message.Notes)
                });

                return new ReturnMessage { Result = ret.Result };
            }

            public async Task<ReturnMessage> UpdatePartialOrderSellerDeliveryPackage(SellerCenterDataMessage sellerCenterData, UpdateOrderSellerDeliveryPackageMessage message, CancellationToken cancellationToken)
            {
                async Task<ReturnMessage<UpdatePackageOrderSellerDeliveryResponse>> apiUpdateOrderSellerDeliveryPackage(UpdatePackageOrderSellerDeliveryRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdatePackageOrderSellerDeliveryResponse>>(request, cancellationToken);

                async Task<ReturnMessage<AddPackageOrderSellerDeliveryResponse>> apiAddOrderSellerDeliveryPackage(AddPackageOrderSellerDeliveryRequest request)
                   => await _apiActorGroup.Ask<ReturnMessage<AddPackageOrderSellerDeliveryResponse>>(request, cancellationToken);

                async Task<ReturnMessage<GetOrderByIdResponse>> apiGetOrderById(GetOrderByIdRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderByIdResponse>>(request, cancellationToken);

                async Task<ReturnMessage<GetOrderByIdResponse>> apiGetOrderBySellerAndClientId(GetOrderBySellerAndClientIdRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderByIdResponse>>(request, cancellationToken);
                ReturnMessage<GetOrderByIdResponse> resultOrder;

                if (message.OrderId != Guid.Empty)
                    resultOrder = await apiGetOrderById(new GetOrderByIdRequest { OrderId = message.OrderId });
                else
                    resultOrder = await apiGetOrderBySellerAndClientId(new GetOrderBySellerAndClientIdRequest { SellerId = new Guid(sellerCenterData.SellerId), ClientId = message.OrderClientId });

                var order = resultOrder.Data?.Value;

                if (order == null)
                {
                    _logger.Warning($"OrderService - not found order ", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, sellerCenterData));
                    return new ReturnMessage { Result = Result.Error, Error = new Exception($"not found order {JsonConvert.SerializeObject(message)}", resultOrder.Error) };
                }                    

                OrderSellerDto sellerOrder;

                if (message.OrderSellerId != Guid.Empty)
                    sellerOrder = order.OrderSellers?.FirstOrDefault(x => x.Id == message.OrderSellerId);
                else
                    sellerOrder = order?.OrderSellers?.FirstOrDefault(x => x.SellerId == new Guid(sellerCenterData.SellerId));

                if (sellerOrder == null)
                {
                    _logger.Warning($"OrderService - not found order seller {message.OrderSellerId}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, sellerCenterData));
                    return new ReturnMessage { Result = Result.Error, Error = new Exception($"not found order seller {message.OrderSellerId}") };
                }

                if (!string.IsNullOrEmpty(message.DeliveryClientId))
                {
                    var items = sellerOrder.Deliveries.SelectMany(x => x.Items).ToList();
                    var delivery = sellerOrder.Deliveries.FirstOrDefault();

                    OrderSellerDeliveryPackageViewModel package;

                    if (message.OrderSellerDeliveryPackageId != Guid.Empty)
                        package = delivery.Packages.Where(x => x.Id == message.OrderSellerDeliveryPackageId).FirstOrDefault();
                    else
                        package = delivery.Packages.Where(x => x.DeliveryClientId == message.DeliveryClientId).FirstOrDefault();

                    if (package == null)
                    {
                        var ret = await apiAddOrderSellerDeliveryPackage(new AddPackageOrderSellerDeliveryRequest
                        {
                            OrderId = order.Id,
                            SubOrderId = sellerOrder.Id,
                            DeliveryId = delivery.Id,
                            DeliveryClientId = message.DeliveryClientId,
                            DeliveryItens = items.Select(y => y.Id).ToList(),
                            TrackingCode = message.TrackingCode,
                            TrackingUrl = message.TrackingUrl,
                            TrackingPackagePostageStatus = message.TrackingPostageStatus,
                            DeliveryTime = message.DeliveryTime,
                            InvoiceDate = message.InvoiceDate,
                            InvoiceNumber = message.InvoiceNumber,
                            InvoiceKey = message.InvoiceKey,
                            InvoiceLink = message.InvoiceLink
                        });

                        return new ReturnMessage { Result = ret.Result };
                    }
                    else
                    {
                        var ret = await apiUpdateOrderSellerDeliveryPackage(new UpdatePackageOrderSellerDeliveryRequest
                        {
                            OrderId = order.Id,
                            SubOrderId = sellerOrder.Id,
                            DeliveryId = delivery.Id,
                            PackageId = package.Id,
                            DeliveryClientId = message.DeliveryClientId,
                            DeliveryItens = items.Select(y => y.Id).ToList(),
                            TrackingCode = message.TrackingCode,
                            TrackingUrl = message.TrackingUrl,
                            TrackingPackagePostageStatus = message.TrackingPostageStatus,
                            DeliveryTime = message.DeliveryTime,
                            InvoiceDate = message.InvoiceDate,
                            InvoiceNumber = message.InvoiceNumber,
                            InvoiceKey = message.InvoiceKey,
                            InvoiceLink = message.InvoiceLink
                        });

                        return new ReturnMessage { Result = ret.Result };
                    }
                }

                return new ReturnMessage { Result = Result.OK };

            }

            public async Task<ReturnMessage> ListOrder(ListOrderMessage message, SellerCenterDataMessage sellerCenterData, SellerCenterQueue.Queues queues, CancellationToken cancellationToken)
            {
                async Task<ReturnMessage<GetOrderByFilterResponse>> apiGetOrders(GetOrderByFilterRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<GetOrderByFilterResponse>>(request, cancellationToken);

                var response = await apiGetOrders(new GetOrderByFilterRequest { OrderNumber = message.OrderNumber, PageSize = 1 });

                if (response.Result == Result.Error)
                {
                    _logger.Warning($"OrderService - not found order {message.OrderNumber}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, sellerCenterData));
                    return new ReturnMessage { Result = Result.Error, Error = new Exception($"not found order {message.OrderNumber}", response.Error) };
                }

                var order = response.Data.Value.FirstOrDefault();

                if (response.Data.Value.Count <= 0)
                {
                    _logger.Info($"order {message.OrderNumber} not found");
                }
                else
                {
                    var serviceBusMessage = new ServiceBusMessage(new ProcessOrderMessage(order, sellerCenterData));
                    await queues.ProcessOrderQueue.SendAsync(serviceBusMessage.GetMessage(order.Id));
                }

                return new ReturnMessage { Result = Result.OK };
            }
        }
    }
}
