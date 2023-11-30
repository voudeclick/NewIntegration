using Akka.Actor;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto;
using Samurai.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;
using Samurai.Integration.APIClient.Pier8.Models.Requests.Pedido;
using Samurai.Integration.APIClient.Pier8.Models.Response;
using Samurai.Integration.APIClient.Shopify.Models;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.Domain.Shopify.Models.Results;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Pier8;
using Samurai.Integration.Domain.Messages.Pier8.OrderActor;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Messages.Shopify.OrderActor;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace Samurai.Integration.Application.Services
{
    public partial class Pier8Service
    {
        private ILoggingAdapter _logger;
        private IDictionary<EnumActorType, IActorRef> _apiActorGroup;
        private readonly IConfiguration _configuration;
        private readonly TenantRepository _tenantRepository;

        public Pier8Service(IConfiguration configuration, TenantRepository tenantRepository)
        {
            _configuration = configuration;
            _tenantRepository = tenantRepository;
        }

        public void Init(IDictionary<EnumActorType, IActorRef> apiActorGroup, ILoggingAdapter logger)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;
        }
        public async Task<ReturnMessage> WebhookOrderProcess(WebhookOrderProcessMessage message, Pier8DataMessage pier8DataMessage,
            Pier8Queue.Queues queues, QueueClient _updateTrackingOrderQueue, QueueClient _updateOrderTagNumber,
            CancellationToken cancellationToken = default)
        {

            if (string.IsNullOrEmpty(message.transportadora.rastreador.codigo))
            {
                var omieOrder = await _apiActorGroup.Route(EnumActorType.Omie).Ask<ReturnMessage<ConsultarPedidoOmieRequestOutput>>(
                        new ConsultarPedidoOmieRequest(new APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs.ConsultarPedidoOmieRequestInput { numero_pedido = message.pedido.nroPedido }));

                if (omieOrder.Result == Result.Error)
                {
                    _logger.Warning("Pier8Service - Error in WebhookOrderProcess | {0} | Error: {1}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, pier8DataMessage), omieOrder?.Error?.Message);
                    return new ReturnMessage { Result = Result.Error, Error = omieOrder.Error };
                }

                var queryByTagResult = await _apiActorGroup.Route(EnumActorType.Shopify).Ask<ReturnMessage<OrderByTagQueryOutput>>(
                   new OrderByTagQuery($"ExtId-{omieOrder.Data.pedido_venda_produto.cabecalho.codigo_pedido_integracao}-Intg", OrderResult.CompleteOrder()), cancellationToken);

                if (queryByTagResult.Result == Result.Error)
                {
                    _logger.Warning("Pier8Service - Error in WebhookOrderProcess | {0} | Error: {1}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, pier8DataMessage), queryByTagResult.Error);
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };
                }

                var currentData = queryByTagResult.Data.orders.edges[0].node;

                var tags = new List<string> { $"P8St-{Regex.Replace(message.status, @"\s+", "")}", $"P8Id-{message.pedido.nroPedido}" };

                if (currentData.tags.Any(x => !tags.Contains(x)))
                {
                    var updateOrderTagBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
                    {
                        ShopifyId = Convert.ToInt64(currentData.legacyResourceId),
                        CustomTags = tags
                    });
                    await _updateOrderTagNumber.SendAsync(updateOrderTagBusMessage.GetMessage(Guid.NewGuid()));
                }
                _logger.Info("not found trackingCode for nroPedido: {0} - Status: {1}", message.pedido.nroPedido, message.status);
                return new ReturnMessage { Result = Result.OK };
            }


            var response = await _apiActorGroup.Route(EnumActorType.Omie).Ask<ReturnMessage<ConsultarPedidoOmieRequestOutput>>(
                        new ConsultarPedidoOmieRequest(new APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs.ConsultarPedidoOmieRequestInput { numero_pedido = message.pedido.nroPedido }));
                       
            if (response.Result == Result.Error)
            {
                _logger.Warning("Pier8Service - Error in WebhookOrderProcess | {0} | Error: {1}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, pier8DataMessage), response.Error);
                return new ReturnMessage { Result = Result.Error, Error = response.Error };
            }


            var updateTrackingOrder = new ShopifyUpdateTrackingOrder
            {
                OrderExternalId = response.Data.pedido_venda_produto.cabecalho.codigo_pedido_integracao,
                Shipping = new ShopifyUpdateTrackingOrder.ShippingStatus
                {
                    IsShipped = true,
                    IsDelivered = string.Equals(message.status, "ENTREGUE", StringComparison.OrdinalIgnoreCase),
                    TrackingCode = message.transportadora.rastreador.codigo,
                    TrackingUrl = message.transportadora.rastreador.link
                }
            };
            var serviceBusMessage = new ServiceBusMessage(updateTrackingOrder);
            await _updateTrackingOrderQueue.SendAsync(serviceBusMessage.GetMessage(updateTrackingOrder.OrderExternalId));


            return new ReturnMessage { Result = Result.OK };

        }

        public async Task<ReturnMessage> ProcessUpdateTracking(Pier8UpdateTrackingMessage message, Pier8DataMessage pier8DataMessage,
            Pier8Queue.Queues queues, QueueClient _updateTrackingOrderQueue, QueueClient _updateOrderTagNumber,
            CancellationToken cancellationToken = default)
        {

            OrderResult currentData = null;
            if (!string.IsNullOrEmpty(message.ExternalOrderId))
            {
                var queryByIdResult = await _apiActorGroup.Route(EnumActorType.Shopify).Ask<ReturnMessage<OrderByIdQueryOutput>>(
                        new OrderByIdQuery(Convert.ToInt64(message.ExternalOrderId), OrderResult.CompleteOrder()), cancellationToken
                    );

                var current = queryByIdResult;

                if (current.Result == Result.Error)
                {
                    _logger.Warning("Pier8Service - Error in ProcessUpdateTracking", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, pier8DataMessage));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }


                if (current.Data?.order != null)
                    currentData = current.Data.order;
            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.ExternalRefOrderId))
            {
                var queryByTagResult = await _apiActorGroup.Route(EnumActorType.Shopify).Ask<ReturnMessage<OrderByTagQueryOutput>>(
                   new OrderByTagQuery($"ExtId-{message.ExternalRefOrderId}-Intg", OrderResult.CompleteOrder()), cancellationToken
               );

                var current = queryByTagResult;

                if (current.Result == Result.Error)
                {
                    _logger.Warning("Pier8Service - Error in ProcessUpdateTracking", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, pier8DataMessage));
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };
                }

                if (current.Data?.orders.edges.Any() == true)
                    currentData = current.Data.orders.edges[0].node;
            }


            var externalOrderTag = SearchTagValue(currentData.tags, Tags.OrderExternalId);

            string externalID = null;
            if (externalOrderTag?.Any() == true)
            {
                externalID = externalOrderTag[0];
            }

            if (currentData == null)
            {
                _logger.Info($"order {message.ExternalOrderId ?? message.ExternalRefOrderId} not found");
            }
            else
            {

                var response = await _apiActorGroup.Route(EnumActorType.Omie).Ask<ReturnMessage<ConsultarPedidoOmieRequestOutput>>(
                        new ConsultarPedidoOmieRequest(new APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs.ConsultarPedidoOmieRequestInput { codigo_pedido_integracao = externalID }));

                if (response.Result == Result.Error)
                {
                    _logger.Warning("Pier8Service - Error in ProcessUpdateTracking", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, pier8DataMessage));
                    return new ReturnMessage { Result = Result.Error, Error = response.Error };
                }


                var pedido = await _apiActorGroup.Route(EnumActorType.Pier8).Ask<ReturnMessage<ConsultaPedidoResponse>>(
                       new ConsultaPedidoRequest { pedido = response.Data.pedido_venda_produto.cabecalho.numero_pedido.PadLeft(15, '0') }, cancellationToken
                   );

                if (pedido.Result == Result.Error || pedido.Data?.erros?.Any() == true)
                {
                    _logger.Warning("Pier8Service - Error in ProcessUpdateTracking", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, pier8DataMessage));
                    return new ReturnMessage { Result = Result.Error, Error = pedido.Error };
                }

                var updateOrderTagBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
                {
                    ShopifyId = Convert.ToInt64(currentData.legacyResourceId),
                    CustomTags = new List<string> {
                        $"P8St-{Regex.Replace(pedido.Data.status, @"\s+", "")}",
                        $"P8Id-{pedido.Data.pedido.nroPedido}"
                    }
                });

                await _updateOrderTagNumber.SendAsync(updateOrderTagBusMessage.GetMessage(Guid.NewGuid()));

                _logger.Info($"Pier8 - ProcessUpdateTracking - id Order not found trackingCode for {pedido.Data.pedido} - {pedido.Data.status}");

                var updateTrackingOrder = new ShopifyUpdateTrackingOrder
                {
                    ShopifyId = Convert.ToInt64(currentData.legacyResourceId),
                    Shipping = new ShopifyUpdateTrackingOrder.ShippingStatus
                    {
                        IsShipped = true,
                        IsDelivered = string.Equals(pedido.Data.status, "ENTREGUE", StringComparison.OrdinalIgnoreCase),
                        TrackingCode = pedido.Data.CodigoRastreamento,
                        TrackingUrl = WebUtility.UrlDecode(pedido.Data.transportadora.rastreador.link)
                    }
                };

                var serviceBusMessage = new ServiceBusMessage(updateTrackingOrder);
                await _updateTrackingOrderQueue.SendAsync(serviceBusMessage.GetMessage(updateTrackingOrder.OrderExternalId));

            }


            return new ReturnMessage { Result = Result.OK };

        }

        public string SetTagValue(string key, string value)
        {
            return $"{key}-{value}-Intg";
        }

        public List<string> SearchTagValue(List<string> allTags, string key)
        {
            return allTags?
                    .Select(t => t.Trim())
                    .Where(t => IsTag(t, key))
                    .Select(t =>
                    {
                        return GetTagValue(t, key);
                    })
                    .ToList();
        }

        public string GetTagValue(string tag, string key)
        {
            tag = tag.Remove(0, $"{key}-".Length);
            tag = tag.Remove(tag.Length - "-Intg".Length, "-Intg".Length);
            return tag;
        }

        public bool IsTag(string tag, string key)
        {
            return tag.StartsWith($"{key}-") && tag.EndsWith("-Intg");
        }

    }
}
