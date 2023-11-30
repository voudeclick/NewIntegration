using Akka.Actor;
using Akka.Dispatch;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

using Samurai.Integration.APIClient.Bling.Models.Requests;
using Samurai.Integration.APIClient.Bling.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Enums.Bling;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Bling.Shopify
{
    public class ShopifyBlingOrderActor : BaseBlingTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _updateOrderStatusQueue;
        public ShopifyBlingOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
            : base("BlingOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _blingData = blingData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();


                _updateOrderStatusQueue = tenantService.GetQueueClient(_blingData, ShopifyQueue.UpdateOrderStatusQueue);
            }

            ReceiveAsync((Func<BlingListOrderMessage, Task>)(async message =>
            {
                try
                {
                    var situacaoResult = await _apiActorGroup.Ask<ReturnMessage<BlingGetAllSituationResult>>(new BlingGetAllSituationRequest
                    {
                        Modulo = "Vendas"
                    }, cancellationToken);

                    if (situacaoResult.Result == Result.Error)
                        throw situacaoResult.Error;

                    var orderResult = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(new BlingApiListOrderResquest
                    {
                        NumeroPedido = message.OrderNumber
                    }, cancellationToken);

                    var order = orderResult.Data?.retorno?.pedidos?.FirstOrDefault()?.pedido;


                    if (order != null && !string.IsNullOrWhiteSpace(order.numeroPedidoLoja))
                    {
                        var orderStatus = situacaoResult.Data.GetSituationByName(order.situacao);

                        var messageStatus = new ShopifyUpdateOrderStatusMessage
                        {
                            OrderExternalId = order.numeroPedidoLoja,
                            Cancellation = new ShopifyUpdateOrderStatusMessage.CancellationStatus
                            {
                                IsCancelled = new List<int> { 12 }.Contains(orderStatus.id) //Cancelado
                            },
                            Payment = new ShopifyUpdateOrderStatusMessage.PaymentStatus
                            {
                                IsPaid = new List<int> { 18, 9 }.Contains(orderStatus.id) //Pagamento Confirmado, Em Separação, Despachado, Entregue
                            },
                            //TODO ->  Status Bling pendente
                            Shipping = new ShopifyUpdateOrderStatusMessage.ShippingStatus
                            {
                                IsShipped = new List<int> { 3, 4 }.Contains(orderStatus.id), //Despachado, Entregue
                                IsDelivered = new List<int> { 4 }.Contains(orderStatus.id) //Entregue
                            }
                        };

                        var serviceBusMessage = new ServiceBusMessage(messageStatus);
                        await _updateOrderStatusQueue.SendAsync(serviceBusMessage.GetMessage(messageStatus.OrderExternalId));
                    }

                    Sender.Tell(orderResult);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = ex });
                }

            }));

            ReceiveAsync((Func<BlingUpdateOrderMessage, Task>)(async message =>
            {
                try
                {
                    var orderBling = await CreateOrderBling(message);

                    if (orderBling.Result == Result.Error)
                        throw orderBling.Error;

                    if (orderBling.Data.retorno.erros != null)
                    {
                        if (orderBling.Data.retorno.erros.Any(x => x.erro.cod == 30))
                        {
                            var msg = orderBling.Data.retorno.erros.Where(x => x.erro.cod == 30).FirstOrDefault().erro.msg;
                            var numero = Regex.Match(msg, @"\((?<numero>.*)\)").Groups["numero"].ToString();

                            orderBling = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(new BlingApiListOrderResquest
                            {
                                NumeroPedido = numero
                            }, cancellationToken);

                            if (!string.IsNullOrWhiteSpace(numero))
                            {
                                int? idStatus = message.GetOrderStatus() switch
                                {
                                    Domain.Enums.OrderStatus.Pending => null,
                                    Domain.Enums.OrderStatus.Paid => 9,
                                    Domain.Enums.OrderStatus.Cancelled => 12,
                                    Domain.Enums.OrderStatus.Shipped => null,
                                    Domain.Enums.OrderStatus.Delivered => null,
                                    _ => null
                                };

                                if (idStatus.HasValue)
                                {

                                    var orderUpdateResult = await _apiActorGroup.Ask<ReturnMessage<BlingUpdateStatusOrderResult>>(new BlingUpdateStatusOrderRequest
                                    {
                                        IdSituacao = idStatus.Value,
                                        IdPedido = numero
                                    }, _cancellationToken);

                                }
                            }

                        }
                    }

                    Sender.Tell(orderBling);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));


            ReceiveAsync((Func<ShopifySendOrderToERPMessage, Task>)(async message =>
            {
                try
                {
                    var result = await CreateOrderBling(message);

                    if (result.Result == Result.Error)
                        throw result.Error;

                    if (result.Data?.retorno?.erros != null && !result.Data.retorno.erros.Any(x => x.erro.cod == 30))
                        throw new Exception($"Erro ao criar pedido Bling {string.Join("\n", result.Data.retorno.erros.Select(x => x.erro).ToList())}");
                    else
                    {
                        var order = result.Data?.retorno?.pedidos?.Select(x => x.pedido).FirstOrDefault();
                        if (order != null)
                        {
                            int? idStatus = message.GetOrderStatus() switch
                            {
                                Domain.Enums.OrderStatus.Pending => null,
                                Domain.Enums.OrderStatus.Paid => 9,
                                Domain.Enums.OrderStatus.Cancelled => 12,
                                Domain.Enums.OrderStatus.Shipped => null,
                                Domain.Enums.OrderStatus.Delivered => null,
                                _ => null
                            };

                            if (idStatus.HasValue)
                            {
                                var orderUpdateResult = await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(new BlingApiUpdateOrderRequest()
                                {
                                    NumeroPedido = order.numero,
                                    IdSituacao = idStatus.Value
                                }, _cancellationToken);
                            }
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


        private async Task<ReturnMessage<BlingApiOrderResult>> CreateOrderBling(ShopifySendOrderToERPMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.ExternalID))
            {
                return await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(new BlingApiListOrderResquest
                {
                    NumeroPedido = message.ExternalID
                });
            }
            else if (message.Approved)
            {
                var address = message.Customer?.DeliveryAddress;
                var phone = message.Customer;

                var etiqueta = new Dados_etiqueta
                {
                    Nome = message.Customer.FullName,
                    Endereco = address?.Address,
                    Numero = address?.Number,
                    Complemento = address?.Complement,
                    Municipio = address?.City,
                    Uf = address?.State,
                    Cep = address?.ZipCode,
                    Bairro = address?.District
                };

                return await _apiActorGroup.Ask<ReturnMessage<BlingApiOrderResult>>(
                    new BlingApiCreateOrderRequest
                    {
                        NumeroLoja = $"{_blingData.OrderPrefix}{GetOrder(message)}",
                        Cliente = new Cliente
                        {
                            Nome = message.Customer.FullName,
                            TipoPessoa = message.Customer?.Company.CleanDocument().Length <= 11 ? "F" : "J",
                            Endereco = address?.Address,
                            Cpf_cnpj = message.Customer?.Company.CleanDocument(),
                            Numero = address?.Number,
                            Complemento = address?.Complement,
                            Bairro = address?.District,
                            Cep = address?.ZipCode,
                            Cidade = address?.City,
                            Uf = address?.State,
                            Fone = $"{phone?.DDD}{phone?.Phone?.Replace("-", string.Empty)}",
                            Email = message.Customer?.Email
                        },
                        Transporte = new Transporte
                        {
                            Transportadora = message?.CarrierName,
                            Tipo_frete = "R",
                            Dados_etiqueta = etiqueta
                        },
                        Itens = new Itens
                        {
                            Item = message.Items?.Select(x => new Item
                            {
                                Codigo = x?.Sku,
                                Descricao = x?.Name,
                                Un = "un",
                                Qtde = Convert.ToDecimal(x.Quantity),
                                Vlr_unit = x.Price
                            }).ToList()
                        },
                        Parcelas = new Parcelas
                        {
                            Parcela = new List<Parcela> {
                                    new Parcela {
                                        Vlr = message.Total,
                                        Obs = $"Forma de pagamento: {message.PaymentData.PaymentType} | Parcelas: {message.PaymentData.InstallmentQuantity}"
                                    }
                            }
                        },
                        Vlr_frete = message.ShippingValue,
                        Vlr_desconto = message.DiscountsValues.ToString(),
                        Obs = message.Note,
                        IdSituacao = _blingData.OrderStatusId ?? 6
                    },
                _cancellationToken);
            }
            else
                return new ReturnMessage<BlingApiOrderResult> { Result = Result.OK };
        }
        private string GetOrder(ShopifySendOrderToERPMessage message)
        {
            return _blingData.OrderField switch
            {
                OrderFieldBlingType.number => message.Number.ToString(),
                OrderFieldBlingType.id_shopify => message.ID.ToString(),
                _ => message.Number.ToString()
            };

        }
        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _updateOrderStatusQueue.CloseAsyncSafe();
            });
        }
        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new ShopifyBlingOrderActor(serviceProvider, cancellationToken, blingData, apiActorGroup));
        }
    }
}
