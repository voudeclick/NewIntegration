using Akka.Actor;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

using Samurai.Integration.APIClient.Bling.Models.Requests;
using Samurai.Integration.APIClient.Bling.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Queues;

using System;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Samurai.Integration.Application.Actors.Bling.SellerCenter
{
    public class SellerCenterBlingOrderActor : BaseBlingTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _erpUpdatePartialOrderSellerQueueClient;

        public SellerCenterBlingOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
            : base("SellerCenterBlingOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _blingData = blingData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _erpUpdatePartialOrderSellerQueueClient = tenantService.GetQueueClient(_blingData, SellerCenterQueue.UpdatePartialOrderSeller);
            }

            ReceiveAsync((Func<BlingListOrderMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage<BlingApiOrderResult> result;

                    result = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(new BlingApiListOrderResquest { NumeroPedido = message.OrderNumber }, cancellationToken);

                    var blingOrder = result.Data?.retorno?.pedidos?.FirstOrDefault()?.pedido;

                    if (blingOrder != null && !string.IsNullOrWhiteSpace(blingOrder.numeroPedidoLoja))
                    {
                        var statusMapping = _blingData.OrderStatusMapping?.FirstOrDefault(x => x.BlingSituacaoNome.Equals(blingOrder.situacao, StringComparison.OrdinalIgnoreCase));

                        if (statusMapping != null)
                        {
                            var serviceBusMessage = new ServiceBusMessage(new UpdatePartialOrderSellerMessage
                            {
                                OrderId = Guid.Empty,
                                OrderSellerId = Guid.Empty,
                                OrderClientId = blingOrder.numero,
                                StatusId = statusMapping.ERPStatusId
                            });

                            await _erpUpdatePartialOrderSellerQueueClient.SendAsync(serviceBusMessage.GetMessage(message.OrderNumber));
                        }
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ProcessOrderMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage<BlingApiOrderResult> result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderSeller = message.OrderSellers.FirstOrDefault(x => x.SellerId.ToString() == message.SITenantSellerId);

                        if (orderSeller != null)
                        {
                            var statusMapping = _blingData.OrderStatusMapping?.FirstOrDefault(x => x.ERPStatusId == orderSeller.StatusId);

                            if (orderSeller.ClientId == null)
                            {
                                result = await CreateBlingOrder(message, orderSeller, statusMapping);
                            }
                            else
                            {
                                result = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(new BlingApiListOrderResquest { NumeroPedido = orderSeller.ClientId }, cancellationToken);

                                var blingOrder = result.Data?.retorno?.pedidos?.FirstOrDefault();

                                if (blingOrder != null)
                                {
                                    if (statusMapping == null || statusMapping?.BlingSituacaoNome?.Equals(blingOrder.pedido?.situacao, StringComparison.OrdinalIgnoreCase) == true)
                                    {
                                        result = new ReturnMessage<BlingApiOrderResult> { Result = Result.OK };
                                    }
                                    else
                                    {
                                        var orderUpdateRequest = BlingApiUpdateOrderRequest.From(orderSeller, statusMapping);
                                        result = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(orderUpdateRequest, cancellationToken);
                                    }
                                }
                                else
                                {
                                    result = await CreateBlingOrder(message, orderSeller, statusMapping);
                                }
                            }
                        }
                        else
                        {
                            result = new ReturnMessage<BlingApiOrderResult> { Result = Result.OK };
                        }
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task<ReturnMessage<BlingApiOrderResult>> CreateBlingOrder(ProcessOrderMessage message, OrderSellerDto orderSeller, BlingDataOrderStatusMapping statusMapping)
        {
            var orderCreateRequest = BlingApiCreateOrderRequest.From(message, orderSeller, _blingData);
            var ret = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(orderCreateRequest, _cancellationToken);

            var blingOrder = ret.Data?.retorno?.pedidos?.FirstOrDefault()?.pedido;

            if (ret.Data?.retorno?.erros?.Any() == true && blingOrder == null)
            {
                var regex = new Regex("Um pedido com o mesmo hash ja encontra-se cadastrado \\((?<numero>[^-]*)\\)");

                var match = ret.Data?.retorno?.erros.Select(x => regex.Match(x.erro.msg)).FirstOrDefault(x => x.Success);

                if (match == null)
                {
                    await SendInfoToSellerCenter(message, orderSeller, blingOrder, ret);

                    return new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = new Exception(string.Join(";", ret.Data?.retorno?.erros.Select(x => x.erro?.msg))) };
                }

                blingOrder = new Pedido
                {
                    numero = match.Groups["numero"].ToString()
                };
            }

            if (blingOrder != null)
            {
                if (statusMapping != null)
                {
                    var orderUpdateRequest = BlingApiUpdateOrderRequest.From(orderSeller, statusMapping);
                    orderUpdateRequest.NumeroPedido = blingOrder.numero;

                    var resultUpdateOrder = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(orderUpdateRequest, _cancellationToken);

                    if (resultUpdateOrder.Result == Result.Error)
                        return new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = resultUpdateOrder.Error };
                }

                await SendInfoToSellerCenter(message, orderSeller, blingOrder, ret);               
            }

            return ret;
        }

        private async Task SendInfoToSellerCenter(ProcessOrderMessage message, OrderSellerDto orderSeller,Pedido blingOrder, ReturnMessage<BlingApiOrderResult> ret)
        {
            var serviceBusMessage = new ServiceBusMessage(new UpdatePartialOrderSellerMessage
            {
                OrderId = message.Id,
                OrderSellerId = orderSeller.Id,
                OrderClientId= (blingOrder is null && string.IsNullOrWhiteSpace(blingOrder?.numero)) ? String.Empty : blingOrder?.numero,
                Notes = ret.Data?.retorno?.erros.Any() == true ? JsonConvert.SerializeObject(ret.Data?.retorno?.erros) : String.Empty
            });

            await _erpUpdatePartialOrderSellerQueueClient.SendAsync(serviceBusMessage.GetMessage(message.Id));
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterBlingOrderActor(serviceProvider, cancellationToken, blingData, apiActorGroup));
        }
    }
}
