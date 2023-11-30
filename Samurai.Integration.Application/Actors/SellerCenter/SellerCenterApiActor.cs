using Akka.Actor;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.SellerCenter;
using Samurai.Integration.APIClient.SellerCenter.Models;
using Samurai.Integration.APIClient.SellerCenter.Models.Requests;
using Samurai.Integration.APIClient.SellerCenter.Models.Response;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.SellerCenter
{
    public class SellerCenterApiActor : BaseSellerCenterTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly SellerApiAdresses _apiAdresses;
        private readonly SellerCenterApiClient _client;

        public SellerCenterApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken,
                SellerCenterDataMessage sellerCenterData,
                SellerApiAdresses apiAdresses,
                Credentials credentials)
            : base("SellerCenterApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _sellerCenterData = sellerCenterData;
            _apiAdresses = apiAdresses;
            using (var scope = _serviceProvider.CreateScope())
            {
                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
                _client = new SellerCenterApiClient(httpClientFactory, _apiAdresses, credentials, _log);
            }

            ReceiveAsync((Func<GetVariationByFilterRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting SellerCenterApiGetVariationByFilterRequest");
                    string url = "VariationOptions";
                    var param = new Dictionary<string, string>() { };
                    param.Add("Name", message.Name.ToString());
                    param.Add("CultureName", message.CultureName.ToString());
                    param.Add("PageSize", message.PageSize.ToString());
                    param.Add("PageIndex", message.PageIndex);

                    var response = await _client.Products.Get<GetVariationByFilterResponse>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    LogDebug("Ending SellerCenterApiGetVariationByFilterRequest");
                    if (response.Value.Count > 0)
                    {
                        Self.Forward(new GetVariationsByIdRequest { Id = response.Value.FirstOrDefault().Id.Value });
                    }
                    else
                    {
                        Sender.Tell(new ReturnMessage<GetVariationsByIdResponse> { Result = Result.Error, Error = new ArgumentException($"Not Found variation {message.Name}") });
                    }

                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in SellerCenterApiGetVariationByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(),message, null, $"Error in SellerCenterApiGetVariationByFilterRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetVariationByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetVariationsByIdRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting SellerCenterApiApiGetVariationsByIdRequest");
                    string url = $"VariationOptions/{message.Id}";
                    var response = await _client.Products.Get<GetVariationsByIdResponse>(url, _cancellationToken);
                    LogDebug("Ending SellerCenterApiApiGetVariationsByIdRequest");
                    Sender.Tell(new ReturnMessage<GetVariationsByIdResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in SellerCenterApiApiGetVariationsByIdRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in SellerCenterApiApiGetVariationsByIdRequest | {ex.Message}"));                  
                    Sender.Tell(new ReturnMessage<GetVariationsByIdResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CreateVariationOptionRequest, Task>)(async message =>
            {
                try
                {                    
                    LogDebug("Starting SellerCenterApiCreateVariationOptionRequest");
                    string url = $"VariationOptions";
                    var response = await _client.Products.Post<CreateVariationOptionResponse>(url, message, _cancellationToken);
                    LogDebug("Ending SellerCenterApiCreateVariationOptionRequest");
                    Sender.Tell(new ReturnMessage<CreateVariationOptionResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in SellerCenterApiCreateVariationOptionRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in SellerCenterApiCreateVariationOptionRequest | {ex.Message}"));                    
                    Sender.Tell(new ReturnMessage<CreateVariationOptionResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateVariationOptionRequest, Task>)(async message =>
            {
                try
                {                  
                    LogDebug("Starting SellerCenterApiCreateVariationOptionRequest");
                    string url = $"VariationOptions/{message.Id}";
                    var response = await _client.Products.Put<UpdateVariationOptionResponse>(url, message, _cancellationToken);
                    LogDebug("Ending SellerCenterApiCreateVariationOptionRequest");
                    Sender.Tell(new ReturnMessage<UpdateVariationOptionResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in SellerCenterApiCreateVariationOptionRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in SellerCenterApiCreateVariationOptionRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<UpdateVariationOptionResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetCategoriesByFilterRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetCategoriesByFilterRequest");
                    string url = $"Categories";
                    var param = new Dictionary<string, string>() { };
                    param.Add("Name", message.Name.ToString());
                    param.Add("CultureName", message.CultureName.ToString());
                    param.Add("PageSize", message.PageSize.ToString());
                    param.Add("PageIndex", message.PageIndex);
                    var response = await _client.Products.Get<GetCategoriesByFilterResponse>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    LogDebug("Ending GetCategoriesByFilterRequest");
                    Sender.Tell(new ReturnMessage<GetCategoriesByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetCategoriesByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetCategoriesByFilterRequest | {ex.Message}"));                    
                    Sender.Tell(new ReturnMessage<GetCategoriesByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CreateCategoryRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting CreateCategoryRequest");
                    string url = $"Categories";
                    var response = await _client.Products.Post<CreateCategoryResponse>(url, message, _cancellationToken);
                    LogDebug("Ending CreateCategoryRequest");
                    Sender.Tell(new ReturnMessage<CreateCategoryResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in CreateCategoryRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in CreateCategoryRequest | {ex.Message}"));                    
                    Sender.Tell(new ReturnMessage<CreateCategoryResponse> { Result = Result.Error, Error = ex });
                }
            }));

            #region Manufacturers
            ReceiveAsync((Func<GetManufacturersByFilterRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetManufacturersByFilterRequest");
                    string url = $"Manufacturers";
                    var param = new Dictionary<string, string>() { };
                    param.Add("Name", message.Name.ToString());
                    param.Add("CultureName", message.CultureName.ToString());
                    param.Add("PageSize", message.PageSize.ToString());
                    param.Add("PageIndex", message.PageIndex);
                    var response = await _client.Products.Get<GetManufacturersByFilterResponse>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    LogDebug("Ending GetManufacturersByFilterRequest");
                    Sender.Tell(new ReturnMessage<GetManufacturersByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetManufacturersByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetManufacturersByFilterRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetManufacturersByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CreateManufacturersRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting CreateManufacturersRequest");
                    string url = $"Manufacturers";
                    var response = await _client.Products.Post<CreateManufacturersResponse>(url, message, _cancellationToken);
                    LogDebug("Ending CreateManufacturersRequest");
                    Sender.Tell(new ReturnMessage<CreateManufacturersResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in CreateManufacturersRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in CreateManufacturersRequest | {ex.Message}"));                
                    Sender.Tell(new ReturnMessage<CreateManufacturersResponse> { Result = Result.Error, Error = ex });
                }
            }));
            #endregion

            ReceiveAsync((Func<GetSellerRequest, Task>)(async message =>
            {
                try
                {                    
                    string url = $"Sellers/{message.SellerId}";                    

                    var response = await _client.Seller.Get<GetSellerResponse>(url, _cancellationToken);
                    
                    Sender.Tell(new ReturnMessage<GetSellerResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetSellerRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetSellerRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetSellerResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CreateProductRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting CreateProductRequest");

                    LogWarning("Mensagem eviada para Seller Senter: {0}",  JsonConvert.SerializeObject(message));

                    string url = $"Products";

                    var response = await _client.Products.Post<CreateProductResponse>(url, message, _cancellationToken);

                    LogWarning("Resposta Seller Senter: {0}", JsonConvert.SerializeObject(response));

                    LogDebug("Ending CreateProductRequest");

                    Sender.Tell(new ReturnMessage<CreateProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in CreateProductRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in CreateProductRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<CreateProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateProductRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting UpdateProductRequest");

                    string url = $"Products/{message.Id}";

                    var response = await _client.Products.Put<UpdateProductResponse>(url, message, _cancellationToken);

                    LogDebug("Ending UpdateProductRequest");
                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in UpdateProductRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in UpdateProductRequest | {ex.Message}"));                   
                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<List<UpdatePriceProductRequest>, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting UpdatePriceProductRequest");

                    string url = $"Sellers/{message.FirstOrDefault()?.SellerId}/products/{message.FirstOrDefault()?.ProductClientCode}";

                    var response = await _client.Seller.Post<UpdatePriceProductResponse>(url, message, _cancellationToken);

                    LogDebug("Ending UpdatePriceProductRequest");

                    Sender.Tell(new ReturnMessage<UpdatePriceProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in UpdatePriceProductRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in UpdatePriceProductRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<UpdatePriceProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetProductByFilterRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetProductByFilterRequest");

                    string url = $"/Products/GetFullProductByExactlyClientCode";

                    var param = new Dictionary<string, string>() { };
                    param.Add("ClientCode", message.ProductCode.ToString());
                    param.Add("SellerId", message.SellerId.ToString());

                    var response = await _client.Products.Get<Product>(QueryHelpers.AddQueryString(url, param), _cancellationToken);

                    LogDebug("Ending GetProductByFilterRequest");

                    Sender.Tell(new ReturnMessage<GetProductByFilterResponse> { Result = Result.OK, Data = new GetProductByFilterResponse { Value = response }});
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetProductByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetProductByFilterRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetProductByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetProductVariationByClientCodeRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetProductVariationByClientCodeRequest");

                    string url = $"Sellers/{message.SellerId}/products/{message.ClientCode}";

                    var response = await _client.Seller.Get<GetProductVariationByClientCodeResponse>(url, _cancellationToken);

                    LogDebug("Ending GetProductVariationByClientCodeRequest");

                    Sender.Tell(new ReturnMessage<GetProductVariationByClientCodeResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetProductVariationByClientCodeRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetProductVariationByClientCodeRequest | {ex.Message}"));                    
                    Sender.Tell(new ReturnMessage<GetProductVariationByClientCodeResponse> { Result = Result.Error, Error = ex });
                }
            }));

            #region Orders
            ReceiveAsync((Func<GetOrderByFilterRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetOrderByFilterRequest");
                    string url = $"Orders";
                    var param = new Dictionary<string, string>() { };

                    if (message.StartUpdateDate.HasValue)
                        param.Add("StartUpdateDate", message.StartUpdateDate.Value.ToString("o"));

                    param.Add("OrderBy", message.OrderBy?.ToString() ?? "");
                    param.Add("OrderNumber", message.OrderNumber ?? "");
                    param.Add("PageSize", message.PageSize.ToString());
                    param.Add("PageIndex", message.PageIndex.ToString());
                    var response = await _client.Orders.Get<GetOrderByFilterResponse>(QueryHelpers.AddQueryString(url, param), _cancellationToken);

                    if (response.TotalItems > response.PageSize)
                    {
                        var totalPages = Math.Ceiling((decimal)response.TotalItems / response.PageSize);

                        for (int i = 1; i < totalPages; i++)
                        {
                            param["PageIndex"] = i.ToString();
                            var responsePage = await _client.Orders.Get<GetOrderByFilterResponse>(QueryHelpers.AddQueryString(url, param), _cancellationToken);

                            response.Value.AddRange(responsePage.Value);
                        };
                    }

                    LogDebug("Ending GetOrderByFilterRequest");
                    Sender.Tell(new ReturnMessage<GetOrderByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetOrderByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetOrderByFilterRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetOrderByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdatePartialOrderRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting UpdateStatusOrderRequest");
                    string url = $"OrderStatus/{message.OrderId}";
                    var response = await _client.Orders.Post<UpdatePartialOrderResponse>(url, message, _cancellationToken);
                    LogDebug("Ending UpdateStatusOrderRequest");
                    Sender.Tell(new ReturnMessage<UpdatePartialOrderResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in UpdateStatusOrderRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in UpdateStatusOrderRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<UpdatePartialOrderResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetOrderByIdRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetOrderByIdRequest");
                    string url = $"Orders/{message.OrderId}";
                    var response = await _client.Orders.Get<GetOrderByIdResponse>(url, _cancellationToken);
                    LogDebug("Ending GetOrderByIdRequest");
                    Sender.Tell(new ReturnMessage<GetOrderByIdResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetOrderByIdRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetOrderByIdRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetOrderByIdResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdatePartialOrderSellerRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting UpdatePartialOrderSellerRequest");
                    string url = $"Orders/PartialOrderSeller";
                    var response = await _client.Orders.Put<UpdatePartialOrderSellerResponse>(url, message, _cancellationToken);
                    LogDebug("Ending UpdatePartialOrderSellerRequest");
                    Sender.Tell(new ReturnMessage<UpdatePartialOrderSellerResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in UpdatePartialOrderSellerRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in UpdatePartialOrderSellerRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<UpdatePartialOrderSellerResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<AddPackageOrderSellerDeliveryRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting AddPackageOrderSellerDeliveryRequest");

                    string url = $"Orders/AddPackage";
                    var response = await _client.Orders.Post<AddPackageOrderSellerDeliveryResponse>(url, message, _cancellationToken);

                    LogDebug("Ending AddPackageOrderSellerDeliveryRequest");

                    Sender.Tell(new ReturnMessage<AddPackageOrderSellerDeliveryResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in AddPackageOrderSellerDeliveryRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in AddPackageOrderSellerDeliveryRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<AddPackageOrderSellerDeliveryResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdatePackageOrderSellerDeliveryRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting UpdatePackageOrderSellerDeliveryRequest");

                    string url = $"Orders/UpdatePackage";
                    var response = await _client.Orders.Put<UpdatePackageOrderSellerDeliveryResponse>(url, message, _cancellationToken);

                    LogDebug("Ending UpdatePackageOrderSellerDeliveryRequest");

                    Sender.Tell(new ReturnMessage<UpdatePackageOrderSellerDeliveryResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in UpdatePackageOrderSellerDeliveryRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in UpdatePackageOrderSellerDeliveryRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<UpdatePackageOrderSellerDeliveryResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetOrderStatusRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetOrderStatusRequest");
                    string url = $"OrderStatus";
                    var param = new Dictionary<string, string>() { };
                    //param.Add("StartDate", message.StartDate.ToString("yyyy-MM-dd"));
                    var response = await _client.Orders.Get<GetOrderStatusResponse>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    LogDebug("Ending GetOrderStatusRequest");
                    Sender.Tell(new ReturnMessage<GetOrderStatusResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetOrderStatusRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetOrderStatusRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetOrderStatusResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetOrderBySellerAndClientIdRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting GetOrderByIdRequest");
                    string url = $"Orders/Seller/{message.SellerId}/ClientId/{message.ClientId}";
                    var response = await _client.Orders.Get<GetOrderByIdResponse>(url, _cancellationToken);
                    LogDebug("Ending GetOrderByIdRequest");
                    Sender.Tell(new ReturnMessage<GetOrderByIdResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterApiActor - Error in GetOrderByIdRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in GetOrderByIdRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage<GetOrderByIdResponse> { Result = Result.Error, Error = ex });
                }
            }));
            #endregion

        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, SellerCenterDataMessage sellerCenterData, SellerApiAdresses apiAdresses, Credentials credentials)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterApiActor(serviceProvider, cancellationToken, sellerCenterData, apiAdresses, credentials));
        }
    }
}
