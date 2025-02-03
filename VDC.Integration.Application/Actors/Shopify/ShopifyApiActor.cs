using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Shopify;
using VDC.Integration.APIClient.Shopify.Models.Request;
using VDC.Integration.APIClient.Shopify.Models.Request.REST;
using VDC.Integration.Domain.Entities.Database.TenantData;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Shopify.Models.Request;
using VDC.Integration.Domain.Shopify.Models.Results;
using OrderResult = VDC.Integration.Domain.Shopify.Models.Results.REST.OrderResult;

namespace VDC.Integration.Application.Actors.Shopify
{
    public class ShopifyApiActor : BaseShopifyTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly ShopifyApp _app;
        private readonly ShopifyApiClient _client;
        private readonly ShopifyRESTClient _restClient;

        public ShopifyApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, ShopifyDataMessage shopifyData, ShopifyApp app)
            : base("ShopifyApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _shopifyData = shopifyData;
            _app = app;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _configuration = scope.ServiceProvider.GetService<IConfiguration>();
                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

                var loggerShopifyApiClient = scope.ServiceProvider.GetService<ILogger<ShopifyApiClient>>();
                var loggerShopifyRESTClient = scope.ServiceProvider.GetService<ILogger<ShopifyRESTClient>>();

                string versionShopify = _configuration.GetSection("Shopify")["Version"];

                _client = new ShopifyApiClient(httpClientFactory, _shopifyData.Id.ToString(), _shopifyData.ShopifyStoreDomain, versionShopify, _app.ShopifyPassword);
                _restClient = new ShopifyRESTClient(loggerShopifyRESTClient, httpClientFactory, _shopifyData.Id.ToString(), _shopifyData.ShopifyStoreDomain, versionShopify, _app.ShopifyPassword);
            }

            ReceiveAsync<ProductByIdQuery>(Receive);
            ReceiveAsync<ProductByTagQuery>(Receive);
            ReceiveAsync<ProductIdsByTagQuery>(Receive);
            ReceiveAsync<ProductMetafieldsByTagQuery>(Receive);
            ReceiveAsync<VariantByIdQuery>(Receive);
            ReceiveAsync<VariantBySkuQuery>(Receive);
            ReceiveAsync<VariantParentProductsBySkuQuery>(Receive);
            ReceiveAsync<LocationQuery>(Receive);
            ReceiveAsync<AllProductsTagsQuery>(Receive);
            ReceiveAsync<AllCollectionsQuery>(Receive);
            ReceiveAsync<ProductAndInventoryUpdateMutation>(Receive);
            ReceiveAsync<ProductCreateMutation>(Receive);
            ReceiveAsync<ProductUpdateMutation>(Receive);
            ReceiveAsync<ProductAppendImagesMutation>(Receive);
            ReceiveAsync<VariantUpdateMutation>(Receive);
            ReceiveAsync<InventorySetQuantitiesMutation>(Receive);
            ReceiveAsync<InventoryActivateMutation>(Receive);
            ReceiveAsync<InventoryUpdateMutation>(Receive);
            ReceiveAsync<CollectionCreateMutation>(Receive);
            ReceiveAsync<OrderByIdQuery>(Receive);
            ReceiveAsync<OrderByTagQuery>(Receive);
            ReceiveAsync<OrderByDateQuery>(Receive);
            ReceiveAsync<OrderUpdateMutation>(Receive);
            ReceiveAsync<OrderMarkAsPaidMutation>(Receive);
            ReceiveAsync<FulfillmentCreateMutation>(Receive);
            ReceiveAsync<FulfillmentTrackingInfoUpdateMutation>(Receive);
            ReceiveAsync<OrderByDateAndStatusQuery>(Receive);


            #region REST
            ReceiveAsync((Func<GetOrderRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"orders/{message.OrderId}.json";
                    var response = await _restClient.Get<OrderResult>(url, cancellationToken: _cancellationToken);

                    Sender.Tell(new ReturnMessage<OrderResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyApiActor - Error in GetOrderRequest", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage<OrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetOrderFulfillmentRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"orders/{message.OrderId}/fulfillment_orders.json";
                    var response = await _restClient.Get<OrderResult>(url, cancellationToken: _cancellationToken);

                    Sender.Tell(new ReturnMessage<OrderResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyApiActor - Error in GetOrderFulfillmentRequest", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage<OrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CreateFulfillmentEventRequest, Task>)(async message =>
            {
                try
                {

                    string url = $"orders/{message.OrderId}/fulfillments/{message.FulfillmentId}/events.json";
                    await _restClient.Post(url, new { @event = new { status = message.Status } }, cancellationToken: _cancellationToken);

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyApiActor - Error in CreateFulfillmentEventRequest", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CancelOrderRequest, Task>)(async message =>
            {
                try
                {

                    string url = $"orders/{message.OrderId}/cancel.json";
                    await _restClient.Post(url, new { email = message.SendEmail }, cancellationToken: _cancellationToken);

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyApiActor - Error in CancelOrderRequest", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetOrderTransactionRequest, Task>)(async message =>
            {
                try
                {
                    var url = $"orders/{message.OrderId}/transactions.json";
                    var response = await _restClient.Get<GetOrderTransactionResult>(url, cancellationToken: _cancellationToken);

                    Sender.Tell(new ReturnMessage<GetOrderTransactionResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
            #endregion
        }

        public async Task Receive<I, O>(BaseMutation<I, O> message)
            where I : BaseMutationInput
            where O : BaseMutationOutput, new()
        {
            try
            {


                var response = await _client.Post(message, _cancellationToken);

                Sender.Tell(new ReturnMessage<O> { Result = Result.OK, Data = response });
            }
            catch (Exception ex)
            {
                LogError(ex, Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, null));
                Sender.Tell(new ReturnMessage<O> { Result = Result.Error, Error = ex });
            }
        }

        public async Task Receive<O>(BaseQuery<O> message) where O : BaseQueryOutput, new()
        {
            try
            {

                var response = await _client.Post(message, _cancellationToken);

                Sender.Tell(new ReturnMessage<O> { Result = Result.OK, Data = response });
            }
            catch (Exception ex)
            {
                LogError(ex, Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, null));
                Sender.Tell(new ReturnMessage<O> { Result = Result.Error, Error = ex });
            }
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, ShopifyDataMessage shopifyData, ShopifyApp app)
        {
            return Akka.Actor.Props.Create(() => new ShopifyApiActor(serviceProvider, cancellationToken, shopifyData, app));
        }
    }
}
