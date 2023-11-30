using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.Tray;
using Samurai.Integration.APIClient.Tray.Models.Requests.Attribute;
using Samurai.Integration.APIClient.Tray.Models.Requests.Category;
using Samurai.Integration.APIClient.Tray.Models.Requests.Customer;
using Samurai.Integration.APIClient.Tray.Models.Requests.Manufacture;
using Samurai.Integration.APIClient.Tray.Models.Requests.Order;
using Samurai.Integration.APIClient.Tray.Models.Requests.Product;
using Samurai.Integration.APIClient.Tray.Models.Requests.Variant;
using Samurai.Integration.APIClient.Tray.Models.Requests.Variation;
using Samurai.Integration.APIClient.Tray.Models.Response.Attribute;
using Samurai.Integration.APIClient.Tray.Models.Response.Category;
using Samurai.Integration.APIClient.Tray.Models.Response.Customer;
using Samurai.Integration.APIClient.Tray.Models.Response.Manufacture;
using Samurai.Integration.APIClient.Tray.Models.Response.Order;
using Samurai.Integration.APIClient.Tray.Models.Response.Product;
using Samurai.Integration.APIClient.Tray.Models.Response.Variant;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Results.Logger;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Tray
{
    public class TrayApiActor : BaseTrayTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly TrayApiClient _client;

        public TrayApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData) : base("TrayApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantData = tenantData;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

                _client = new TrayApiClient(_configuration.GetSection("UrlAuthentication").Value,
                                            _configuration.GetSection("MethodAuthentication").Value,
                                            _configuration.GetSection("TrayAppKey").Value,
                                            _tenantData.Id.ToString(),
                                            _log);

            }

            #region Product

            #region Category

            ReceiveAsync((Func<CreateCategoryRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"categories";

                    var param = new Dictionary<string, string>() { };
                    param.Add("name", message.Name);
                    param.Add("description", message.Description);
                    param.Add("slug", message.Slug);
                    param.Add("order", message.Order);
                    param.Add("title", message.Title);
                    param.Add("metatag.keywords", message.Metatag.Keywords);
                    param.Add("metatag.description", message.Metatag.Description);

                    var response = await _client.Post<CreateCategoryResponse>(url, param);

                    Sender.Tell(new ReturnMessage<CreateCategoryResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<CreateCategoryResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetCategoriesByFilterRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"categories";

                    var param = new Dictionary<string, string>() { };

                    if (message.Id > 0)
                        param.Add("id", message.Id.ToString());

                    if (!string.IsNullOrEmpty(message.Name))
                        param.Add("name", message.Name);

                    param.Add("limit", message.Limit.ToString());
                    param.Add("page", message.Page.ToString());

                    var response = await _client.Get<GetCategoriesByFilterResponse>(url, param);

                    Sender.Tell(new ReturnMessage<GetCategoriesByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetCategoriesByFilterResponse>
                    {
                        Result = Result.Error,
                        Error = ex
                    });
                }
            }));

            #endregion

            #region Manufacture

            ReceiveAsync((Func<CreateManufactureRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/brands";

                    var param = new Dictionary<string, string>() { };
                    param.Add("brand", message.Brand);
                    param.Add("slug", message.Slug);

                    var response = await _client.Post<CreateManufactureResponse>(url, param: param);

                    Sender.Tell(new ReturnMessage<CreateManufactureResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<CreateManufactureResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetManufactureByFilterRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/brands";

                    var param = new Dictionary<string, string>() { };

                    if (message.Id > 0)
                        param.Add("id", message.Id.ToString());

                    if (!string.IsNullOrEmpty(message.Brand))
                        param.Add("brand", message.Brand);

                    param.Add("limit", message.Limit.ToString());
                    param.Add("page", message.Page.ToString());

                    var response = await _client.Get<GetManufactureByFilterResponse>(url, param);

                    Sender.Tell(new ReturnMessage<GetManufactureByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetManufactureByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            #endregion

            #region Atributos

            ReceiveAsync((Func<CreateAttributeRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"properties";

                    var response = await _client.Post<CreateAttributeResponse>(url, message);

                    Sender.Tell(new ReturnMessage<CreateAttributeResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<CreateAttributeResponse> { Result = Result.Error, Error = ex });
                }
            }));

            #endregion

            ReceiveAsync((Func<CreateProductRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products";

                    var response = await _client.Post<CreateProductResponse>(url, message);

                    Sender.Tell(new ReturnMessage<CreateProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<CreateProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CreateAttributeByProductRequest, Task>)(async message =>
            {
                try
                {

                    string url = $"products/{message.ProductId}/properties";

                    var response = await _client.Post<CreateAttributeByProductResponse>(url, message);

                    Sender.Tell(new ReturnMessage<CreateAttributeByProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<CreateAttributeByProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateProductRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/{message.Id}";

                    var response = await _client.Put<UpdateProductResponse>(url, message);

                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateProductPriceRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/{message.Id}";

                    var response = await _client.Put<UpdateProductResponse>(url, message);

                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.Error, Error = ex });
                }
            }));


            ReceiveAsync((Func<UpdateProductStockRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/{message.Id}";

                    var response = await _client.Put<UpdateProductResponse>(url, message);

                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateProductAvailableRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/{message.Id}";

                    var response = await _client.Put<UpdateProductResponse>(url, message);

                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateProductResponse> { Result = Result.Error, Error = ex });
                }
            }));


            ReceiveAsync((Func<DeleteProductRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/{message.Id}";

                    var response = await _client.Delete<DeleteProductResponse>(url);

                    Sender.Tell(new ReturnMessage<DeleteProductResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<DeleteProductResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetProductsByFilterRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products";

                    var param = new Dictionary<string, string>() { };

                    if (message.Id > 0)
                        param.Add("id", message.Id.ToString());

                    if (!string.IsNullOrEmpty(message.Name))
                        param.Add("name", message.Name);

                    if (!string.IsNullOrEmpty(message.CategoryId))
                        param.Add("category_id", message.CategoryId);

                    if (!string.IsNullOrEmpty(message.Ean))
                        param.Add("ean", message.Ean);

                    if (!string.IsNullOrEmpty(message.Brand))
                        param.Add("brand", message.Brand);

                    if (!string.IsNullOrEmpty(message.Reference))
                        param.Add("reference", message.Reference);

                    if (message.Created != null && message.Created != DateTime.MinValue)
                        param.Add("created", message.Created.ToString("yyyy-MM-dd"));

                    if (message.Modified != null && message.Modified != DateTime.MinValue)
                        param.Add("modified", message.Modified.ToString("yyyy-MM-dd"));

                    param.Add("limit", message.Limit.ToString());
                    param.Add("page", message.Page.ToString());

                    var response = await _client.Get<GetProductsByFilterResponse>(url, param);

                    Sender.Tell(new ReturnMessage<GetProductsByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetProductsByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetProductByIdRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/{message.Id}";

                    var response = await _client.Get<GetProductByIdResponse>(url);

                    Sender.Tell(new ReturnMessage<GetProductByIdResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetProductByIdResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateProductProcessingRequest, Task>)(async message =>
            {
                try
                {
                    var response = await _client.UpdateProductProcessing(message);
                    if (!response)
                        Sender.Tell(new ReturnMessage { Result = Result.Error });

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetProductProcessRequest, Task>)(async message =>
            {
                try
                {
                    var response = await _client.GetProductProcess(message);

                    Sender.Tell(new ReturnMessage<GetProductProcessResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            #region ProductVariation

            ReceiveAsync((Func<CreateVariantRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/variants";

                    var response = await _client.Post<CreateVariantResponse>(url, message.ProductVariation);

                    Sender.Tell(new ReturnMessage<CreateVariantResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<CreateVariantResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateVariantRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/variants/{message.Id}";

                    var response = await _client.Put<UpdateVariantResponse>(url, message.ProductVariation);

                    Sender.Tell(new ReturnMessage<UpdateVariantResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateVariantResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateVariantPriceRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/variants/{message.Id}";

                    var response = await _client.Put<UpdateVariantResponse>(url, message);

                    Sender.Tell(new ReturnMessage<UpdateVariantResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateVariantResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateVariantStockRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/variants/{message.Id}";

                    var response = await _client.Put<UpdateVariantResponse>(url, message);

                    Sender.Tell(new ReturnMessage<UpdateVariantResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateVariantResponse> { Result = Result.Error, Error = ex });
                }
            }));



            ReceiveAsync((Func<GetVariantsByFilterRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/variants";

                    var param = new Dictionary<string, string>() { };

                    if (message.Id > 0)
                        param.Add("id", message.Id.ToString());

                    if (!string.IsNullOrEmpty(message.ProductId.ToString()))
                        param.Add("product_id", message.ProductId.ToString());

                    if (!string.IsNullOrEmpty(message.Ean))
                        param.Add("ean", message.Ean);

                    if (!string.IsNullOrEmpty(message.Reference))
                        param.Add("reference", message.Reference);

                    param.Add("limit", message.Limit.ToString());


                    var variants = new List<Samurai.Integration.APIClient.Tray.Models.Response.Inputs.Variant>();

                    param.Add("page", message.Page.ToString());

                    var page = message.Page;

                    while (true)
                    {
                        try
                        {
                            param["page"] = page.ToString();

                            var response = await _client.Get<GetVariantsByFilterResponse>(url, param);

                            if (response?.Variants?.Any() == false)
                                break;

                            variants.AddRange(response.Variants);

                            if (response?.Variants.Count < message.Limit)
                                break;

                            page++;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    Sender.Tell(new ReturnMessage<GetVariantsByFilterResponse> { Result = Result.OK, Data = new GetVariantsByFilterResponse() { Variants = variants } });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetVariantsByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<DeleteVariantRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"products/variants/{message.Id}";

                    var response = await _client.Delete<DeleteVariantResponse>(url);

                    Sender.Tell(new ReturnMessage<DeleteVariantResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<DeleteVariantResponse> { Result = Result.Error, Error = ex });
                }
            }));

            #endregion

            #endregion

            #region Order
            ReceiveAsync((Func<GetOrderByFilterRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"orders";

                    var param = new Dictionary<string, string>() { };

                    if (message.OrderId.HasValue)
                        param.Add("id", message.OrderId.ToString());

                    if (message.CustomerId.HasValue)
                        param.Add("customer_id", message.CustomerId.ToString());

                    if (!string.IsNullOrEmpty(message.Status))
                        param.Add("status", message.Status);


                    param.Add("limit", message.Limit.ToString());
                    param.Add("page", message.Page.ToString());

                    var response = await _client.Get<GetOrderByFilterResponse>(url, param);

                    Sender.Tell(new ReturnMessage<GetOrderByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetOrderByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetOrderCompleteByIdRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"orders/{message.OrderId}/complete";

                    var response = await _client.Get<GetOrderCompleteByIdResponse>(url);

                    Sender.Tell(new ReturnMessage<GetOrderCompleteByIdResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetOrderCompleteByIdResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateOrderRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"orders/{message.OrderId}";

                    object order = null;

                    if (message.Order != null) order = message.Order;
                    else if (message.OrderStatus != null) order = message.OrderStatus;
                    else if (message.OrderCancel != null) order = message.OrderCancel;

                    var response = new UpdateOrderResponse();

                    if (order != null)
                        response = await _client.Put<UpdateOrderResponse>(url, order);

                    Sender.Tell(new ReturnMessage<UpdateOrderResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateOrderResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<TrayOrderCancelRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"orders/cancel/{message.OrderId}";

                    var response = await _client.Put<UpdateOrderResponse>(url, _cancellationToken);

                    Sender.Tell(new ReturnMessage<UpdateOrderResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<UpdateOrderResponse> { Result = Result.Error, Error = ex });
                }
            }));

            #endregion


            #region Customer
            ReceiveAsync((Func<GetCustomerByFilterRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"customers";

                    var param = new Dictionary<string, string>() { };

                    param.Add("id", message.CustomerId.ToString());
                    param.Add("limit", message.Limit.ToString());
                    param.Add("page", message.Page.ToString());

                    var response = await _client.Get<GetCustomerByFilterResponse>(url, param);

                    if (message.IncludeAddress)
                    {
                        if (response.Customers.Any())
                        {

                            var result = await GetCustomerAddress(new GetCustomerByFilterRequest
                            {
                                CustomerId = message.CustomerId
                            });

                            var addressId = response.Customers.FirstOrDefault().Customer.CustomerAddress.FirstOrDefault().Id;

                            var address = result.CustomerAddresses.Where(x => x.CustomerAddress.Id == addressId).FirstOrDefault().CustomerAddress;

                            response.Customers.Select(x => { x.Customer.Address = address; return x; }).ToList();
                        }
                    }

                    Sender.Tell(new ReturnMessage<GetCustomerByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetCustomerByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetCustomerAddressByFilterRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"customers/addresses";

                    var param = new Dictionary<string, string>() { };

                    param.Add("customer_id", message.CustomerId.ToString());
                    param.Add("limit", message.Limit.ToString());
                    param.Add("page", message.Page.ToString());

                    var response = await _client.Get<GetCustomerAddressByFilterResponse>(url, param);

                    Sender.Tell(new ReturnMessage<GetCustomerAddressByFilterResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<GetCustomerAddressByFilterResponse> { Result = Result.Error, Error = ex });
                }
            }));
            #endregion

        }

        public async Task<GetCustomerAddressByFilterResponse> GetCustomerAddress(GetCustomerByFilterRequest parameters)
        {
            try
            {
                string url = $"customers/addresses";

                var param = new Dictionary<string, string>() { };

                param.Add("customer_id", parameters.CustomerId.ToString());
                param.Add("limit", parameters.Limit.ToString());
                param.Add("page", parameters.Page.ToString());

                return await _client.Get<GetCustomerAddressByFilterResponse>(url, param);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData)
        {
            return Akka.Actor.Props.Create(() => new TrayApiActor(serviceProvider, cancellationToken, tenantData));
        }


    }
}
