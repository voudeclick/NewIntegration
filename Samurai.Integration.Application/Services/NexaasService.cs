using Akka.Actor;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Samurai.Integration.APIClient.Nexaas.Models.Requests;
using Samurai.Integration.APIClient.Nexaas.Models.Results;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Nexaas;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models;
using Samurai.Integration.Domain.Models.Nexaas;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public class NexaasService
    {
        private ILoggingAdapter _logger;
        private readonly TenantRepository _tenantRepository;
        private IActorRef _apiActorGroup;

        public NexaasService(TenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter logger)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;
        }

        public async Task<ReturnMessage> SendFullProductShopifyMessage(NexaasSendFullProductShopifyMessage productNexaasMessage,
                                    NexaasData nexaasData,
                                    QueueClient shopifyFullProductQueueClient,
                                    CancellationToken cancellationToken = default)
        {
            var fullProductMessage = GetFullProductMessage(productNexaasMessage, nexaasData);
            var serviceBusMessage = new ServiceBusMessage(fullProductMessage);
            await shopifyFullProductQueueClient.SendAsync(serviceBusMessage.GetMessage(fullProductMessage.ProductInfo.ExternalId));

            return new ReturnMessage { Result = Result.OK };
        }

        private ShopifyUpdateFullProductMessage GetFullProductMessage(NexaasSendFullProductShopifyMessage productNexaasMessage, NexaasData nexaasData)
        {
            return new ShopifyUpdateFullProductMessage
            {
                ProductInfo = GetProductInfo(productNexaasMessage, nexaasData)
            };
        }

        private Product.Info GetProductInfo(NexaasSendFullProductShopifyMessage productNexaas, NexaasData nexaasData)
        {
            var variants = GetVariants(productNexaas, nexaasData);
            var activeSkus = variants.Where(v => v.Status).Join(productNexaas.Skus, v => v.Sku, s => GetShopifySkuCode(s), (v, s) => s).ToList();
            var productInfo = new Product.Info
            {
                ExternalId = productNexaas.Product.id.ToString(),
                ExternalCode = productNexaas.Product.code,
                Title = productNexaas.Product.name,
                Status = productNexaas.Product.IsActive() && activeSkus.Any(),
                BodyHtml = productNexaas.Product.description,
                VendorId = productNexaas.Vendor.id.ToString(),
                Vendor = productNexaas.Vendor.name,
                Images = new ProductImages
                {
                    ImageUrls = productNexaas.Product.product_images?.Select(i => i.dataURL).ToList() ?? new List<string>(),
                    SkuImages = productNexaas.Skus.Select(s => new ProductImages.SkuImage
                    {
                        Sku = GetShopifySkuCode(s),
                        SkuImageUrl = s.product_images?.Select(i => i.dataURL).FirstOrDefault()
                    }).ToList()
                },
                OptionsName = activeSkus.FirstOrDefault()?.product_features?.GetAllFeatures().Select(s => s.name).ToList() ?? new List<string>(),
                Categories = GetCategories(productNexaas.Categories),
                Variants = variants
            };

            return productInfo;
        }

        private List<Product.Category> GetCategories(List<NexaasCategory> categories)
        {
            if (categories.Any() == false)
                return new List<Product.Category>();

            Product.Category response = null;

            foreach (var category in categories)
            {
                response = new Product.Category
                {
                    Id = category.id.ToString(),
                    Name = category.name,
                    ChildCategories = response == null ? new List<Product.Category>() : new List<Product.Category> { response }
                };
            }

            return new List<Product.Category> { response };
        }

        private List<Product.SkuInfo> GetVariants(NexaasSendFullProductShopifyMessage productNexaas, NexaasData nexaasData)
        {
            var response = new List<Product.SkuInfo>();
            foreach (var sku in productNexaas.Skus)
            {
                var options = sku.product_features?.GetAllFeatures().Select(s => s.feature_variant.name).ToList();
                var price = sku.SkuPrice;
                var stocks = productNexaas.StocksSkus.Where(s => s.product_sku_id == sku.id &&
                                                                s.stock_id == nexaasData.StockId &&
                                                                s.stock.organization_id == nexaasData.OrganizationId &&
                                                                s.stock.sale_channels.Any(c => c.id == nexaasData.SaleChannelId));

                if (sku.active)
                {
                    if (stocks.Any() == false)
                    {
                        _logger.Info($"stock missing for product {productNexaas.Product.id}, sku {sku.id}");
                        sku.active = false;
                    }

                    if (price == null)
                    {
                        _logger.Info($"price missing for product {productNexaas.Product.id}, sku {sku.id}");
                        sku.active = false;
                    }
                }

                var responseSkus = new Product.SkuInfo();
                responseSkus.Sku = GetShopifySkuCode(sku);
                responseSkus.Status = sku.active;
                responseSkus.WeightInKG = sku.weight ?? 0;
                responseSkus.Barcode = sku.ean;
                responseSkus.Options = options ?? new List<string>();
                responseSkus.Price = new Product.SkuPrice
                {
                    Price = price?.sale_price ?? 0,
                    CompareAtPrice = price?.price ?? price?.sale_price
                };
                responseSkus.Stock = new Product.SkuStock
                {
                    Quantity = stocks.Sum(s => s.available_quantity)
                };
                response.Add(responseSkus);
            }
            return response;
        }

        public async Task<ReturnMessage> ListPartialProduct(NexaasListPartialProductMessage message, NexaasData nexaasData, QueueClient nexaasFullProductQueueClient, QueueClient shopifyPartialProductQueueClient, QueueClient shopifyPartialSkuQueueClient, CancellationToken cancellationToken)
        {
            var resultSku = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListSkuResult>>(new NexaasApiListSkuRequest { Id = message.ProductSkuId }, cancellationToken);
            if (resultSku.Result == Result.Error)
            {
                _logger.Warning($"NexaasService - Error in ListPartialProduct | {resultSku.Error.Message}",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, nexaasData));
                return new ReturnMessage { Result = Result.Error, Error = resultSku.Error };
            }

            if (resultSku.Data?.product_sku == null)
                throw new Exception($"sku {message.ProductSkuId} not found");


            var skuNexaas = resultSku.Data.product_sku;
            var productNexaas = resultSku.Data.product_sku.product;
            List<NexaasStockSku> stocks = null;
            NexaasSkuPrice price = null;

            if (skuNexaas.active && !message.NewSku)
            {
                var resultSkuPrice = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListProductPricesResult>>(new NexaasApiListProductPricesRequest { ProductSkuId = message.ProductSkuId }, cancellationToken);
                if (resultSkuPrice.Result == Result.Error)
                {
                    _logger.Warning($"NexaasService - Error in ListPartialProduct | {resultSkuPrice.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, nexaasData));
                    return new ReturnMessage { Result = Result.Error, Error = resultSku.Error };
                }

                price = resultSkuPrice.Data?.sku_price;

                var stocksSkus = new List<NexaasStockSku>();
                ReturnMessage<NexaasApiListProductStocksResult> resultStock = null;
                var productStockRequest = new NexaasApiListProductStocksRequest
                {
                    search = new NexaasApiListProductStocksRequest.ProductStocksRequest
                    {
                        stock_skus = new List<NexaasApiListProductStocksRequest.SkuStockRequest>
                    {
                        new NexaasApiListProductStocksRequest.SkuStockRequest
                        {
                            product_sku_id = message.ProductSkuId
                        }
                    }
                    }
                };
                productStockRequest.Page = 1;
                do
                {
                    resultStock = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListProductStocksResult>>(productStockRequest, cancellationToken);
                    if (resultStock.Result == Result.Error)
                    {
                        _logger.Warning($"NexaasService - Error in ListPartialProduct | {resultStock.Error.Message}",
                                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, nexaasData));
                        return new ReturnMessage { Result = Result.Error, Error = resultStock.Error };
                    }

                    stocksSkus.AddRange(resultStock.Data.stock_skus);
                    productStockRequest.Page++;
                } while (resultStock.Data.stock_skus.Any());

                stocks = stocksSkus.Where(s => s.stock_id == nexaasData.StockId &&
                                                                s.stock.organization_id == nexaasData.OrganizationId &&
                                                                s.stock.sale_channels.Any(c => c.id == nexaasData.SaleChannelId)).ToList();

                if (stocks?.Any() != true)
                {
                    _logger.Info($"stock missing for product {productNexaas.id}, sku {skuNexaas.id}");
                    skuNexaas.active = false;
                }

                if (price == null)
                {
                    _logger.Info($"price missing for product {productNexaas.id}, sku {skuNexaas.id}");
                    skuNexaas.active = false;
                }
            }

            if (!skuNexaas.active || message.NewSku)
            {
                //se a sku está inativa precisa pegar as outras skus para atualizar
                await nexaasFullProductQueueClient.SendAsync(
                                new ServiceBusMessage(
                                    new ShopifyListERPFullProductMessage
                                    {
                                        ExternalId = productNexaas.id.ToString()
                                    })
                                .GetMessage(productNexaas.id));
            }
            else
            {
                var partialProductMessage = new ServiceBusMessage(new ShopifyUpdatePartialProductMessage
                {
                    ProductInfo = new Product.Info
                    {
                        ExternalId = productNexaas.id.ToString(),
                        ExternalCode = productNexaas.code,
                        Title = productNexaas.name,
                        Status = productNexaas.IsActive(),
                        BodyHtml = productNexaas.description,
                        VendorId = productNexaas.product_brand_id.ToString(),
                        Images = new ProductImages
                        {
                            ImageUrls = productNexaas.product_images?.Select(i => i.dataURL).ToList() ?? new List<string>(),
                            SkuImages = productNexaas.product_skus.Select(s => new ProductImages.SkuImage
                            {
                                Sku = GetShopifySkuCode(s),
                                SkuImageUrl = s.product_images?.Select(i => i.dataURL).FirstOrDefault()
                            }).ToList()
                        },
                        Categories = productNexaas.product_category_id > 0 ? new List<Product.Category>
                        {
                            new Product.Category
                            {
                                Id = productNexaas.product_category_id.ToString()
                            }
                        } : new List<Product.Category>()
                    }
                });
                await shopifyPartialProductQueueClient.SendAsync(partialProductMessage.GetMessage(productNexaas.id));


                var optionsName = skuNexaas.product_features?.GetAllFeatures().Select(s => s.name).ToList();
                var options = skuNexaas.product_features?.GetAllFeatures().Select(s => s.feature_variant.name).ToList();

                var partialSkuMessage = new ServiceBusMessage(new ShopifyUpdatePartialSkuMessage
                {
                    ExternalProductId = productNexaas.id.ToString(),
                    SkuInfo = new Product.SkuInfo
                    {
                        Sku = GetShopifySkuCode(skuNexaas),
                        Status = skuNexaas.active,
                        WeightInKG = skuNexaas.weight ?? 0,
                        Barcode = skuNexaas.ean,
                        OptionsName = optionsName ?? new List<string>(),
                        Options = options ?? new List<string>(),
                        Price = new Product.SkuPrice
                        {
                            Price = price?.sale_price ?? 0,
                            CompareAtPrice = price?.price
                        },
                        Stock = new Product.SkuStock
                        {
                            Quantity = stocks.Sum(s => s.available_quantity)
                        }
                    }
                });
                await shopifyPartialSkuQueueClient.SendAsync(partialSkuMessage.GetMessage(skuNexaas.id));
            }

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> ListStockSku(long stockSkuId, NexaasData nexaasData, QueueClient shopifyStockQueueClient, CancellationToken cancellationToken)
        {
            var resultStockSku = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListStockSkuResult>>(new NexaasApiListStockSkuRequest { StockSkuId = stockSkuId }, cancellationToken);
            if (resultStockSku.Result == Result.Error)
            {
                _logger.Warning($"NexaasService - Error in ListStockSku | {resultStockSku.Error.Message}",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), resultStockSku, nexaasData));
                return new ReturnMessage { Result = Result.Error, Error = resultStockSku.Error };
            }


            if (resultStockSku.Data?.stock_sku == null)
                throw new Exception($"stocksku {stockSkuId} not found");

            var stockSku = resultStockSku.Data?.stock_sku;


            var stocksSkus = new List<NexaasStockSku>();
            ReturnMessage<NexaasApiListProductStocksResult> resultStock = null;
            var productStockRequest = new NexaasApiListProductStocksRequest
            {
                search = new NexaasApiListProductStocksRequest.ProductStocksRequest
                {
                    stock_skus = new List<NexaasApiListProductStocksRequest.SkuStockRequest>
                    {
                        new NexaasApiListProductStocksRequest.SkuStockRequest
                        {
                            product_sku_id = stockSku.product_sku_id
                        }
                    }
                }
            };
            productStockRequest.Page = 1;

            do
            {
                resultStock = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListProductStocksResult>>(productStockRequest, cancellationToken);
                if (resultStock.Result == Result.Error)
                {
                    _logger.Warning($"NexaasService - Error in ListStockSku | {resultStock.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), resultStock, nexaasData));
                    return new ReturnMessage { Result = Result.Error, Error = resultStock.Error };
                }

                stocksSkus.AddRange(resultStock.Data.stock_skus);
                productStockRequest.Page++;
            } while (resultStock.Data.stock_skus.Any());

            var stocks = stocksSkus.Where(s => s.stock_id == nexaasData.StockId &&
                                                            s.stock.organization_id == nexaasData.OrganizationId &&
                                                            s.stock.sale_channels.Any(c => c.id == nexaasData.SaleChannelId));
            if (stocks.Any() == false)
                throw new Exception($"stocks for sku {stockSku.product_sku_id} not found");

            var stockMessage = new ShopifyUpdateStockMessage
            {
                ExternalProductId = stockSku.product_sku.product_id.ToString(),
                Value = new Product.SkuStock
                {
                    Sku = GetShopifySkuCode(stocks.First().product_sku),
                    Quantity = stocks.Sum(s => s.available_quantity)
                }
            };
            var serviceBusMessage = new ServiceBusMessage(stockMessage);
            await shopifyStockQueueClient.SendAsync(serviceBusMessage.GetMessage(stockMessage.Value.Sku));

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> ListVendor(NexaasListVendorMessage message,
                            NexaasData nexaasData,
                            QueueClient shopifyUpdateVendorQueueClient,
                            CancellationToken cancellationToken = default)
        {

            var resultVendor = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListVendorResult>>(new NexaasApiListVendorRequest { Id = message.Id }, cancellationToken);
            if (resultVendor.Result == Result.Error)
            {
                _logger.Warning($"NexaasService - Error in ListVendor | {resultVendor.Error.Message}",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, nexaasData));
                return new ReturnMessage { Result = Result.Error, Error = resultVendor.Error };
            }

            var vendorMessage = new ShopifyUpdateVendorMessage
            {
                Id = resultVendor.Data.product_brand.id.ToString(),
                Name = resultVendor.Data.product_brand.name
            };
            var serviceBusMessage = new ServiceBusMessage(vendorMessage);
            await shopifyUpdateVendorQueueClient.SendAsync(serviceBusMessage.GetMessage(vendorMessage.Id));

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> ListProductCategories(string id,
                    NexaasData nexaasData,
                    QueueClient shopifyPartialProductQueueClient,
                    CancellationToken cancellationToken = default)
        {

            var resultProduct = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListProductResult>>(new NexaasApiListProductRequest { Id = long.Parse(id) }, cancellationToken);
            if (resultProduct.Result == Result.Error)
            {
                _logger.Warning($"NexaasService - Error in ListProductCategories | {resultProduct.Error.Message}",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), resultProduct, nexaasData));
                return new ReturnMessage { Result = Result.Error, Error = resultProduct.Error };
            }

            long? categoryId = resultProduct.Data.product.product_category_id;
            var categories = new List<NexaasCategory>();
            while (categoryId > 0)
            {
                var resultCategory = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListCategoryResult>>(new NexaasApiListCategoryRequest { Id = (long)categoryId }, cancellationToken);
                if (resultCategory.Result == Result.Error)
                {
                    _logger.Warning($"NexaasService - Error in ListProductCategories | {resultCategory.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), resultCategory, nexaasData));
                    return new ReturnMessage { Result = Result.Error, Error = resultCategory.Error };
                }

                categories.Add(resultCategory.Data.product_category);
                categoryId = resultCategory.Data.product_category.parent_id;
            }

            var productPartialMessage = new ShopifyUpdatePartialProductMessage
            {
                ProductInfo = new Product.Info
                {
                    ExternalId = id,
                    Categories = GetCategories(categories)
                }
            };

            var serviceBusMessage = new ServiceBusMessage(productPartialMessage);
            await shopifyPartialProductQueueClient.SendAsync(serviceBusMessage.GetMessage(productPartialMessage.ProductInfo.ExternalId));

            return new ReturnMessage { Result = Result.OK };
        }


        public async Task<ReturnMessage> ListOrder(NexaasListOrderMessage message, NexaasData nexaasData, QueueClient shopifyUpdateOrderStatusQueueClient, CancellationToken cancellationToken)
        {
            NexaasOrder currentData = null;
            if (message.NexaasOrderId.HasValue)
            {
                var queryByIdResult = await GetOrderById(message.NexaasOrderId.Value, cancellationToken);

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"NexaasService - Error in ListOrder | {queryByIdResult.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, nexaasData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data.order != null && queryByIdResult.Data.order.organization_id == nexaasData.OrganizationId)
                    currentData = queryByIdResult.Data.order;
            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.ExternalOrderId))
            {
                var queryByExternalIdResult = await GetOrderByExternalId(message.ExternalOrderId, cancellationToken);

                if (queryByExternalIdResult.Result == Result.Error)
                {
                    _logger.Warning($"NexaasService - Error in ListOrder | {queryByExternalIdResult.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, nexaasData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByExternalIdResult.Error };
                }

                if (queryByExternalIdResult.Data.orders.Any() == true)
                    currentData = queryByExternalIdResult.Data.orders[0];
            }
            if (currentData != null)
            {
                var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderStatusMessage
                {
                    OrderExternalId = currentData.data.code,
                    Cancellation = new ShopifyUpdateOrderStatusMessage.CancellationStatus
                    {
                        IsCancelled = currentData.payment_status == "rejected" || currentData.status == "cancelled"
                    },
                    Payment = new ShopifyUpdateOrderStatusMessage.PaymentStatus
                    {
                        IsPaid = currentData.payment_status == "approved"
                    },
                    Shipping = new ShopifyUpdateOrderStatusMessage.ShippingStatus
                    {
                        IsShipped = new List<string> { "transporting", "delivered" }.Contains(currentData.status), //Despachado, Entregue
                        IsDelivered = new List<string> { "delivered" }.Contains(currentData.status), //Entregue
                        TrackingObject = currentData.shipping_service?.tracking_code,
                        TrackingUrl = currentData.shipping_service?.tracking_url
                    }
                });
                await shopifyUpdateOrderStatusQueueClient.SendAsync(serviceBusMessage.GetMessage(currentData.data.code));
                return new ReturnMessage { Result = Result.OK };
            }
            else
            {
                throw new Exception($"order {message.NexaasOrderId?.ToString() ?? message.ExternalOrderId} not found found");
            }
        }

        public async Task<ReturnMessage> UpdateOrder(NexaasData nexaasData, ShopifySendOrderToERPMessage message, CancellationToken cancellationToken)
        {
            var orderList = await GetOrderByExternalId(message.ExternalID, cancellationToken);

            if (orderList.Result == Result.Error)
            {
                _logger.Warning($"NexaasService - Error in UpdateOrder | {orderList.Error.Message}",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, nexaasData));
                return new ReturnMessage { Result = Result.Error, Error = orderList.Error };
            }

            string payment_status = "pending";
            if (message.Cancelled)
                payment_status = "rejected";
            else if (message.Approved)
                payment_status = "approved";

            if (orderList.Data.orders.Any() == false)
            {
                var stockID = nexaasData.StockId;
                var daysToDeliver = message.DaysToDelivery;
                if (message.IsPickup == true)
                {
                    stockID = long.Parse(message.PickupAdditionalData?.ElementAtOrDefault(0) ?? stockID.ToString());
                    daysToDeliver = int.Parse(message.PickupAdditionalData?.ElementAtOrDefault(1) ?? daysToDeliver.ToString());
                }

                var response = await _apiActorGroup.Ask<ReturnMessage>(
                   new NexaasApiCreateOrderRequest
                   {
                       order = new NexaasApiCreateOrderRequest.Order
                       {
                           organization_id = nexaasData.OrganizationId,
                           sale_channel_id = nexaasData.SaleChannelId,
                           stock_id = stockID,
                           code = message.ExternalID,
                           placed_at = message.CreatedAt,
                           discount = message.DiscountsValues,
                           pre_order = false,
                           payment_status = payment_status,
                           shipping = message.ShippingValue,
                           total_value = message.Total,
                           pickup_on_store = message.IsPickup == true,
                           items = message.Items.Select(i => new NexaasApiCreateOrderRequest.Item
                           {
                               item_value = i.Price * i.Quantity,
                               product_sku_id = SplitShopifySkuCode(i.Sku).id,
                               quantity = i.Quantity,
                               unit_price = i.Price
                           }).ToList(),
                           customer = new NexaasApiCreateOrderRequest.Customer
                           {
                               document = !nexaasData.DisableCustomerDocument ? message.Customer.Company.CleanDocument() : string.Empty,
                               email = message.Customer.Email,
                               name = $"{message.Customer.FirstName} {message.Customer.LastName}",
                               phones = (new List<string> { message.Customer.Phone, message.Customer.BillingAddress.Phone, message.Customer.DeliveryAddress.Phone }).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList()
                           },
                           billing_address = new NexaasApiCreateOrderRequest.Address
                           {
                               city = message.Customer.BillingAddress.City,
                               country = message.Customer.BillingAddress.CountryCode,
                               detail = message.Customer.BillingAddress.Complement,
                               neighborhood = message.Customer.BillingAddress.District,
                               number = message.Customer.BillingAddress.Number,
                               state = message.Customer.BillingAddress.State,
                               street = message.Customer.BillingAddress.Address,
                               zipcode = message.Customer.BillingAddress.ZipCode
                           },
                           shipping_address = new NexaasApiCreateOrderRequest.Address
                           {
                               city = message.Customer.DeliveryAddress.City,
                               country = message.Customer.DeliveryAddress.CountryCode,
                               detail = message.Customer.DeliveryAddress.Complement,
                               neighborhood = message.Customer.DeliveryAddress.District,
                               number = message.Customer.DeliveryAddress.Number,
                               state = message.Customer.DeliveryAddress.State,
                               street = message.Customer.DeliveryAddress.Address,
                               zipcode = message.Customer.DeliveryAddress.ZipCode
                           },
                           payments = new List<NexaasApiCreateOrderRequest.Payment>
                            {
                                new NexaasApiCreateOrderRequest.Payment
                                {
                                    installments = message.PaymentData.InstallmentQuantity,
                                    method = GetPaymentMethod(message),
                                    card_brand = GetIssuer(message),
                                    value = message.Total - message.TaxValue,
                                    taxes = message.TaxValue,
                                    total = message.Total
                                }
                            },
                           shipping_service = new NexaasApiCreateOrderRequest.ShippingService
                           {
                               name = message.CarrierName,
                               estimated_due_date = DateTime.Today.AddDays(daysToDeliver).ToString("yyyy-MM-dd")
                           }
                       }
                   }, cancellationToken
               );

                return response;
            }
            else
            {
                var order = orderList.Data.orders[0];
                if (message.Cancelled)
                {
                    if (order.payment_status != "rejected" && order.status != "cancelled")
                    {
                        if (order.payment_status == "pending")
                        {
                            var response = await _apiActorGroup.Ask<ReturnMessage>(
                               new NexaasApiUpdateOrderRequest
                               {
                                   order = new NexaasApiUpdateOrderRequest.Order
                                   {
                                       id = order.id,
                                       payment_status = "rejected"
                                   }
                               }, cancellationToken
                            );
                            return response;
                        }
                        else
                        {
                            var response = await _apiActorGroup.Ask<ReturnMessage>(
                               new NexaasApiCancelOrderRequest
                               {
                                   Id = order.id
                               }, cancellationToken
                            );
                            return response;
                        }
                    }
                }
                else if (message.Approved)
                {
                    if (order.payment_status != "approved")
                    {
                        var response = await _apiActorGroup.Ask<ReturnMessage>(
                           new NexaasApiUpdateOrderRequest
                           {
                               order = new NexaasApiUpdateOrderRequest.Order
                               {
                                   id = order.id,
                                   payment_status = "approved"
                               }
                           }, cancellationToken
                        );
                        return response;
                    }
                }
            }

            return new ReturnMessage { Result = Result.OK };
        }

        public string GetShopifySkuCode(NexaasSku sku)
        {
            return $"{sku.id}-{sku.code}";
        }

        public (long id, string code) SplitShopifySkuCode(string skuCode)
        {
            var id = long.Parse(skuCode.Substring(0, skuCode.IndexOf('-')));
            var code = skuCode.Substring(skuCode.IndexOf('-') + 1);
            return (id, code);
        }

        private static string GetPaymentMethod(ShopifySendOrderToERPMessage message)
        {
            if (message.PaymentData.PaymentType.ToUpper().Equals("BOLETO"))
                return NexaasApiCreateOrderRequest.PaymentMethods.BoletoBancario;
            else if (new List<string> { "DEPÓSITO BANCÁRIO", "TRANSFERÊNCIA BANCÁRIA", "MANUAL" }.Contains(message.PaymentData.PaymentType.ToUpper()))
                return NexaasApiCreateOrderRequest.PaymentMethods.Outros;
            else
                return NexaasApiCreateOrderRequest.PaymentMethods.CartaaoDeCredito;
        }

        private static string GetIssuer(ShopifySendOrderToERPMessage message)
        {
            return message.PaymentData.Issuer.ToUpper() switch
            {
                "VISA" => "1",
                "MASTERCARD" => "2",
                "AMEX" => "3",
                "DINERS" => "4",
                "ELO" => "6",
                "HIPERCARD" => "7",
                "AURA" => "9",
                _ => null
            };
        }

        private async Task<ReturnMessage<NexaasApiListOrderResult>> GetOrderById(long nexaasOrderId, CancellationToken cancellationToken)
        {
            var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListOrderResult>>(
                new NexaasApiListOrderRequest { Id = nexaasOrderId }, cancellationToken
            );
            return queryByIdResult;
        }

        private async Task<ReturnMessage<NexaasApiSearchOrdersResult>> GetOrderByExternalId(string externalOrderId, CancellationToken cancellationToken)
        {
            var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<NexaasApiSearchOrdersResult>>(
                new NexaasApiSearchOrdersRequest { ExternalId = externalOrderId }, cancellationToken
            );
            return queryByIdResult;
        }
    }
}
