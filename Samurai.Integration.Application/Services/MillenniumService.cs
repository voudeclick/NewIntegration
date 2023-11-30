using Akka.Actor;
using Akka.Event;

using AutoMapper;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Samurai.Integration.APIClient.Millennium.Models.Requests;
using Samurai.Integration.APIClient.Millennium.Models.Results;
using Samurai.Integration.Application.Actors;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Helpers;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Entities;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.ProductActor;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models;
using Samurai.Integration.Domain.Models.Millennium;
using Samurai.Integration.Domain.Models.Millennium.Integration;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Repositories;
using Samurai.Integration.Domain.Services.Interfaces;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.longegration.APIClient.Millennium.Models.Results;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Samurai.Integration.Domain.Models.Product;
using SellerCenterMessages = Samurai.Integration.Domain.Messages.SellerCenter;

namespace Samurai.Integration.Application.Services
{
    public class MillenniumService
    {
        ILoggingAdapter _logger;
        readonly TenantRepository _tenantRepository;
        readonly ITenantService _tenantService;
        IActorRefWrapper _apiActorGroup;
        IServiceProvider _serviceProvider;
        MillenniumProductStockIntegrationRepository _millenniumProductStockIntegrationRepository;
        MillenniumNewStockProcessRepository _millenniumNewStockProcessRepository;
        MillenniumNewPricesProcessRepository _millenniumNewPricesProcessRepository;
        MillenniumProductPriceIntegrationRepository _millenniumProductPriceIntegrationRepository;
        MillenniumNewProductProcessRepository _millenniumNewProductProcessRepository;
        MillenniumProductIntegrationRepository _millenniumProductIntegrationRepository;
        MillenniumUpdateOrderProcessRepository _millenniumUpdateOrderProcessRepository;
        MillenniumProductImageIntegrationRepository _millenniumProductImageIntegrationRepository;
        MillenniumListProductManualProcessRepository _millenniumListProductManualProcessRepository;
        MillenniumImageIntegrationRepository _millenniumImageIntegrationRepository;
        IMethodPaymentRepository _methodPaymentRepository;
        IMillenniumDomainService _millenniumDomainService;
        readonly MillenniumOrderStatusUpdateRepository _millenniumOrderStatusUpdateRepository;
        readonly List<ICreateOrder> _createOrder;

        readonly IMapper _mapper;

        public MillenniumService() { }
        public MillenniumService(TenantRepository tenantRepository,
            ITenantService tenantService,
            IMapper mapper,
            IServiceProvider service,
            IMethodPaymentRepository methodPaymentRepository,
            IMillenniumDomainService millenniumDomainService,
            MillenniumProductStockIntegrationRepository millenniumProductStockIntegrationRepository,
            MillenniumNewStockProcessRepository millenniumNewStockProcessRepository,
            MillenniumNewPricesProcessRepository millenniumNewPricesProcessRepository,
            MillenniumProductPriceIntegrationRepository millenniumProductPriceIntegrationRepository,
            MillenniumNewProductProcessRepository millenniumNewProductProcessRepository,
            MillenniumProductIntegrationRepository millenniumProductIntegrationRepository,
            MillenniumProductImageIntegrationRepository millenniumProductImageIntegrationRepository,
            MillenniumImageIntegrationRepository millenniumImageIntegrationRepository,
            MillenniumListProductManualProcessRepository millenniumListProductManualProcessRepository,
            MillenniumUpdateOrderProcessRepository millenniumUpdateOrderProcessRepository,
            MillenniumOrderStatusUpdateRepository millenniumOrderStatusUpdateRepository,
            Func<string, ICreateOrder> totalDiscount,
            Func<string, ICreateOrder> productDiscount)
        {
            _mapper = mapper;
            _tenantRepository = tenantRepository;
            _serviceProvider = service;
            _tenantService = tenantService;
            _methodPaymentRepository = methodPaymentRepository;
            _millenniumDomainService = millenniumDomainService;
            _millenniumProductStockIntegrationRepository = millenniumProductStockIntegrationRepository;
            _millenniumNewStockProcessRepository = millenniumNewStockProcessRepository;
            _millenniumNewPricesProcessRepository = millenniumNewPricesProcessRepository;
            _millenniumProductPriceIntegrationRepository = millenniumProductPriceIntegrationRepository;
            _millenniumNewProductProcessRepository = millenniumNewProductProcessRepository;
            _millenniumProductIntegrationRepository = millenniumProductIntegrationRepository;
            _millenniumProductImageIntegrationRepository = millenniumProductImageIntegrationRepository;
            _millenniumImageIntegrationRepository = millenniumImageIntegrationRepository;
            _millenniumListProductManualProcessRepository = millenniumListProductManualProcessRepository;
            _millenniumUpdateOrderProcessRepository = millenniumUpdateOrderProcessRepository;
            _millenniumOrderStatusUpdateRepository = millenniumOrderStatusUpdateRepository;
            _createOrder = new List<ICreateOrder>()
            {
                totalDiscount("totalDiscount"),
                productDiscount("productDiscount")
            };
        }

        public void Init(IActorRefWrapper apiActorGroup, ILoggingAdapter logger)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;
        }

        public async Task<ReturnMessage> ListProduct(string productId,
                                                MillenniumData millenniumData,
                                                QueueClient shopifyFullProductQueueClient,
                                                QueueClient millenniumProcessProductImageQueueClient,
                                                CancellationToken cancellationToken = default)
        {
            var process = new MillenniumListProductManualProcess()
            {
                Id = Guid.NewGuid(),
                TenantId = millenniumData.Id,
                ProcessDate = DateTime.Now,
                ProductId = productId,
                //Aproveitei o campo existente que não era usado.
                MillenniumResult = "Retorno interno - Status: Recebido"
            };


            if (millenniumData.EnableSaveProcessIntegrations)
            {
                try
                {
                    _millenniumListProductManualProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo manual de integração de produto");
                }
            }

            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                    throw new Exception($"ProductId must not be empty");

                var products = new List<MillenniumApiListProductsResult.Value>();

                var id = GetMillenniumProductId(millenniumData, productId);

                if (id <= 0)
                    throw new Exception($"ProductId must not be empty");

                var productList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListProductsResult>>(
                    new MillenniumApiListProductsRequest
                    {
                        ProductId = id,
                        ListaPreco = true
                    }, cancellationToken
                );

                if (productList.Result == Result.Error)
                {
                    _logger.Warning("MillenniumService - Error in ListProduct | {0} - {1}", productList.Error.Message, LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListProductsRequest
                    {
                        ProductId = id,
                        ListaPreco = true
                    }, millenniumData));

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            process.Exception = productList.Error.Message;

                            _millenniumListProductManualProcessRepository.Save(process);
                        }
                        catch (Exception exx)
                        {
                            _logger.Error(exx, $"Problemas ao salvar catch do processo manual de integração de produto");
                        }
                    }

                    return new ReturnMessage { Result = Result.Error, Error = productList.Error };
                }

                var productCode = GetMillenniumProductCode(millenniumData, productId);

                if (productList.Data.value.Any())
                {
                    if (productCode != null)
                        products.AddRange(productList.Data.GetValues().Where(p => p.cod_produto == productCode));
                    else
                        products.AddRange(productList.Data.GetValues());
                }
                else
                {
                    if (productList.Data.GetValues().Count() < 1)
                    {
                        var productListByBruteForce = await ReListProducts(cancellationToken, productCode, 0);
                        products.AddRange(productListByBruteForce);
                    }
                }

                if (products.Any())
                {
                    var productsSendToShopify = new List<long>();
                    foreach (var product in products)
                    {
                        if (product.tipo_prod == "AC")
                        {
                            if (millenniumData.EnableProductKit == false && product.kit)
                                continue;

                            var stockList = new ReturnMessage<MillenniumApiListStocksResult>();

                            if (!product.kit)
                                stockList = await GetStock(GetMillenniumProductId(millenniumData, product.produto.ToString()), cancellationToken);

                            if (millenniumData.EnableProductKit && product.kit)
                            {
                                stockList.Data = new MillenniumApiListStocksResult();
                                stockList.Data.value = await GetStockkit(product, millenniumData, cancellationToken);
                            }


                            if (stockList.Result == Result.Error)
                            {
                                _logger.Warning($"MillenniumService - Error in ListProduct | {stockList.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListStocksRequest
                                {
                                    ProductId = id
                                }, millenniumData));

                                if (millenniumData.EnableSaveProcessIntegrations)
                                {
                                    try
                                    {
                                        process.Exception = stockList.Error.Message;

                                        _millenniumListProductManualProcessRepository.Save(process);
                                    }
                                    catch (Exception exx)
                                    {
                                        _logger.Error(exx, $"Problemas ao salvar catch do processo manual de integração de produto");
                                    }
                                }

                                return new ReturnMessage { Result = Result.Error, Error = stockList.Error };
                            }

                            var fullProductMessage = GetFullProductMessage(product, stockList.Data.value, millenniumData, process?.Id);
                            var serviceBusMessage = new ServiceBusMessage(fullProductMessage);
                            await shopifyFullProductQueueClient.ScheduleMessageAsync(serviceBusMessage.GetMessage(fullProductMessage.ProductInfo.ExternalId), DateTime.UtcNow.AddSeconds(80));
                            productsSendToShopify.Add(product.produto);

                            if (millenniumData.ProductImageIntegration)
                            {
                                MillenniumProductImageIntegration imageIntegration = new MillenniumProductImageIntegration();

                                var processImageMessage = new ProcessProductImageMessage { IdProduto = product.produto, ExternalId = product.cod_produto, ProductIntegrationRefenceId = process.Id };

                                if (millenniumData.EnableSaveIntegrationInformations)
                                {
                                    try
                                    {
                                        imageIntegration = new MillenniumProductImageIntegration()
                                        {
                                            Id = Guid.NewGuid(),
                                            TenantId = millenniumData.Id,
                                            IdProduto = product.produto,
                                            ExternalId = product.cod_produto,
                                            Payload = JsonConvert.SerializeObject(processImageMessage),
                                            Status = IntegrationStatus.Received,
                                            IntegrationDate = DateTime.Now,
                                            MillenniumListProductProcessId = process.Id,
                                            Routine = "ListProduct"
                                        };

                                        _millenniumProductImageIntegrationRepository.Save(imageIntegration);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex, $"Problemas ao salvar integração manual de produto");
                                    }
                                }

                                var serviceBusMessageImage = new ServiceBusMessage(processImageMessage);
                                await millenniumProcessProductImageQueueClient.SendAsync(serviceBusMessageImage.GetMessage(processImageMessage.IdProduto));

                                if (millenniumData.EnableSaveIntegrationInformations)
                                {
                                    try
                                    {
                                        imageIntegration.Status = IntegrationStatus.SendendToShopifyQueue;

                                        _millenniumProductImageIntegrationRepository.Save(imageIntegration);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex, $"Problemas ao salvar integração de imagens do produto");
                                    }
                                }
                            }
                        }
                    }

                    process.MillenniumResult = $"Retorno interno - Produtos enviados para WebjobShopify: {JsonConvert.SerializeObject(productsSendToShopify)}";


                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumListProductManualProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo manual de integração de produto");
                        }
                    }


                    return new ReturnMessage { Result = Result.OK };
                }
                else
                {
                    throw new Exception($"product {productId} not found");
                }
            }
            catch (Exception ex)
            {
                if (millenniumData.EnableSaveProcessIntegrations)
                {
                    try
                    {
                        process.Exception = ex.Message;

                        _millenniumListProductManualProcessRepository.Save(process);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error(exx, $"Problemas ao salvar catch do processo manual de integração de produto");
                    }
                }

                throw ex;
            }
        }

        async Task<ReturnMessage<MillenniumApiListStocksResult>> GetStock(long id, CancellationToken cancellationToken)
        {
            return await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListStocksResult>>(
                    new MillenniumApiListStocksRequest
                    {
                        ProductId = id
                    }, cancellationToken
                );
        }

        async Task<List<MillenniumApiListStocksResult.Value>> GetStockkit(MillenniumApiListProductsResult.Value product,
                                                                                    MillenniumData millenniumData,
                                                                                    CancellationToken cancellationToken)
        {
            var stocks = new List<MillenniumApiListStocksResult.Value>();

            var componentesKit = product.sku?.SelectMany(s => s.componentes_sku_kit)?.ToList();

            foreach (var componentesSkuKit in componentesKit)
            {
                var id = GetMillenniumProductId(millenniumData, componentesSkuKit.produto_filho.ToString());

                var getStock = await GetStock(id, cancellationToken);
                if (getStock.Result == Result.Error)
                {
                    _logger.Warning($"MillenniumService - Error in GetStockkit | {getStock.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListStocksRequest
                    {
                        ProductId = id
                    }, millenniumData));
                    throw new Exception(getStock.Error.Message);
                }

                stocks.AddRange(getStock.Data.value.Where(s => s.sku == componentesSkuKit.sku));
            }

            return stocks;
        }


        async Task<List<MillenniumApiListProductsResult.Value>> ReListProducts(CancellationToken cancellationToken, string codProduto, long transId)
        {
            var result = new List<MillenniumApiListProductsResult.Value>();

            var productList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListProductsResult>>(
                new MillenniumApiListProductsRequest
                {
                    Top = 500,
                    ListaPreco = true,
                    TransId = transId
                }, cancellationToken
            );

            if (productList.Data.value.Count() > 0)
            {
                result.AddRange(productList.Data.GetValues().Where(x => x.cod_produto == codProduto));
                var auxTransId = productList.Data.GetValues().OrderByDescending(o => o.trans_id).FirstOrDefault().trans_id;

                return await ReListProducts(cancellationToken, codProduto, auxTransId);
            }

            return result;
        }

        long GetMillenniumProductId(MillenniumData millenniumData, string productId)
        {
            if (millenniumData.SplitEnabled)
            {
                return long.Parse(productId.Split("_".ToCharArray(), 2)[0]);
            }
            else
            {
                return long.Parse(productId);
            }
        }

        string GetMillenniumProductCode(MillenniumData millenniumData, string productId)
        {
            if (millenniumData.SplitEnabled)
            {
                var split = productId.Split("_".ToCharArray(), 2);
                if (split.Length > 1)
                    return split[1];
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public async Task<ReturnMessage> ListNewProducts(MillenniumData millenniumData,
                                                     SellerCenterQueue.Queues queueSellerCenter,
                                                     QueueClient erpPartialProductQueueClient,
                                                     QueueClient millenniumProcessProductImageQueueClient,
                                                     QueueClient millenniumFullProductQueueClient,
                                                     CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantRepository.GetById(millenniumData.Id, cancellationToken);
            var transId = tenant.MillenniumData.GetTransId(TransIdType.ListaVitrine);

            var lastIntegrationProcess = _millenniumNewProductProcessRepository.GetLastByTenantId(millenniumData.Id);

            ReturnMessage<MillenniumApiListProductsResult> productList;

            var process = new MillenniumNewProductProcess()
            {
                Id = Guid.NewGuid(),
                TenantId = millenniumData.Id,
                ProcessDate = DateTime.Now,
                Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25,
            };

            if (millenniumData.ControlProductByUpdateDate)
            {
                process.InitialUpdateDate = transId.MillenniumLastUpdateDate != null ? transId.MillenniumLastUpdateDate : new DateTime(2000, 01, 01);

            }
            else
            {
                process.InitialTransId = transId.Value;
            }

            if (millenniumData.EnableSaveProcessIntegrations)
            {
                try
                {
                    _millenniumNewProductProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo de integração de produto | Error.mensagem: {ex.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                        $"Problemas ao salvar processo de integração de produto | Error.mensagem: {ex.Message}"));
                }
            }

            try
            {
                if (millenniumData.ControlProductByUpdateDate)
                {
                    productList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListProductsResult>>(
                        new MillenniumApiListProductsRequest
                        {
                            DataAtualizacao = transId.MillenniumLastUpdateDate != null ? ((DateTime)transId.MillenniumLastUpdateDate).ToString("yyyy-MM-dd HH:mm:ss") : "2000-01-01",
                            Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25,
                            ListaPreco = true
                        }, cancellationToken);
                }
                else
                {
                    productList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListProductsResult>>(
                        new MillenniumApiListProductsRequest
                        {
                            TransId = transId.Value,
                            Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25,
                            ListaPreco = true
                        }, cancellationToken);
                }

                if (productList.Result == Result.Error)
                {
                    process.MillenniumResult = JsonConvert.SerializeObject(productList.Error.Message);

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewProductProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    return new ReturnMessage { Result = Result.Error, Error = productList.Error }; ;
                }

                long? newTransIdValue = null;
                DateTime? newMillenniumLastUpdateDate = null;

                if (productList.Data.value.Any())
                {
                    var products = productList.Data.GetValues();

                    process.TotalCount = productList.Data.value.Count();

                    var firstItem = products.First();
                    var lastItem = products.Last();

                    if (millenniumData.ControlProductByUpdateDate)
                    {
                        var newFirstAndLast = new LastFirstAndLastIds
                        {
                            FirstId = firstItem.produto + firstItem.trans_id + firstItem.data_alteracao_iso,
                            LastId = lastItem.produto + lastItem.trans_id + lastItem.data_alteracao_iso
                        };

                        process.LastFirstAndLastIds = JsonConvert.SerializeObject(newFirstAndLast);

                        if (lastIntegrationProcess != null && !String.IsNullOrWhiteSpace(lastIntegrationProcess.LastFirstAndLastIds))
                        {
                            var oldFirstAndLast = JsonConvert.DeserializeObject<LastFirstAndLastIds>(lastIntegrationProcess.LastFirstAndLastIds);

                            if (((lastIntegrationProcess.Exception == null && lastIntegrationProcess.FinalUpdateDate != null)
                                || (lastIntegrationProcess.Exception == "Same registers" && lastIntegrationProcess.FinalUpdateDate == null))
                                && lastIntegrationProcess.MillenniumResult == null
                                && lastIntegrationProcess.TotalCount == process.TotalCount
                                && newFirstAndLast.FirstId == oldFirstAndLast.FirstId
                                && newFirstAndLast.LastId == oldFirstAndLast.LastId)
                            {
                                throw new Exception("Same registers");
                            }
                        }
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewProductProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    if (millenniumData.ControlProductByUpdateDate)
                    {
                        IFormatProvider culture = new CultureInfo("en-US", true);
                        newMillenniumLastUpdateDate = DateTime.ParseExact(products.Last().data_alteracao_iso, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff'Z'", culture);

                        if (newMillenniumLastUpdateDate == null || (newMillenniumLastUpdateDate < transId.MillenniumLastUpdateDate))
                            throw new Exception($"new newMillenniumLastUpdateDate {newMillenniumLastUpdateDate} is less than old value {transId.MillenniumLastUpdateDate} or null");
                    }
                    else
                    {
                        newTransIdValue = products.OrderByDescending(o => o.trans_id).FirstOrDefault().trans_id;

                        if (newTransIdValue <= transId.Value)
                            throw new Exception($"new transid value {newTransIdValue} is less than old value {transId.Value}");
                    }

                    var itens = new List<Product.Info>();

                    foreach (var product in products)
                    {
                        try
                        {
                            var integration = new MillenniumProductIntegration();
                            if (millenniumData.EnableSaveIntegrationInformations)
                            {
                                try
                                {
                                    integration = new MillenniumProductIntegration()
                                    {
                                        Id = Guid.NewGuid(),
                                        TenantId = millenniumData.Id,
                                        ProductId = product.produto.ToString(),
                                        Payload = JsonConvert.SerializeObject(product),
                                        Status = IntegrationStatus.Received,
                                        IntegrationDate = DateTime.Now,
                                        MillenniumNewProductProcessId = process.Id
                                    };

                                    _millenniumProductIntegrationRepository.Save(integration);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warning("Erro ao salvar integração | MillenniumService:503 | {0}",
                                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), integration, millenniumData,
                                        $"Problemas ao salvar integração de produto | Error.mensagem: {ex.Message}"));
                                }
                            }

                            if (IsSimpleProduct(product))
                            {
                                var partialProductMessage = GetPartialProductMessage(product, millenniumData, integration.Id);

                                itens.Add(partialProductMessage.ProductInfo);

                                var serviceBusMessage = new ServiceBusMessage(partialProductMessage);

                                if (millenniumData.IntegrationType == Domain.Enums.IntegrationType.SellerCenter)
                                    _ = erpPartialProductQueueClient.ScheduleMessageAsync(serviceBusMessage.GetMessage(partialProductMessage.ProductInfo.ExternalId), DateTime.UtcNow.AddSeconds(60));
                                else
                                    _ = erpPartialProductQueueClient.SendAsync(serviceBusMessage.GetMessage(partialProductMessage.ProductInfo.ExternalId));

                                if (millenniumData.ProductImageIntegration)
                                {
                                    MillenniumProductImageIntegration imageIntegration = new MillenniumProductImageIntegration();

                                    var processImageMessage = new ProcessProductImageMessage { IdProduto = product.produto, ExternalId = product.cod_produto, ProductIntegrationRefenceId = integration.Id };

                                    if (millenniumData.EnableSaveIntegrationInformations)
                                    {
                                        try
                                        {
                                            imageIntegration = new MillenniumProductImageIntegration()
                                            {
                                                Id = Guid.NewGuid(),
                                                TenantId = millenniumData.Id,
                                                IdProduto = product.produto,
                                                ExternalId = product.cod_produto,
                                                Payload = JsonConvert.SerializeObject(processImageMessage),
                                                Status = IntegrationStatus.Received,
                                                IntegrationDate = DateTime.Now,
                                                MillenniumListProductProcessId = process.Id,
                                                Routine = "ListNewProducts"
                                            };

                                            _millenniumProductImageIntegrationRepository.Save(imageIntegration);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(ex, $"Problemas ao salvar integração de produto");
                                        }
                                    }

                                    var serviceBusMessageImage = new ServiceBusMessage(processImageMessage);
                                    await millenniumProcessProductImageQueueClient.SendAsync(serviceBusMessageImage.GetMessage(processImageMessage.IdProduto));

                                    if (millenniumData.EnableSaveIntegrationInformations)
                                    {
                                        try
                                        {
                                            imageIntegration.Status = IntegrationStatus.SendendToShopifyQueue;

                                            _millenniumProductImageIntegrationRepository.Save(imageIntegration);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(ex, $"Problemas ao salvar integração de imagens do produto");
                                        }
                                    }
                                }

                                if (millenniumData.EnableSaveIntegrationInformations)
                                {
                                    try
                                    {
                                        integration.Status = IntegrationStatus.SendendToShopifyQueue;
                                        _millenniumProductIntegrationRepository.Save(integration);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Warning("Erro ao salvar integração | MillenniumService:503 | {0}",
                                            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), integration, millenniumData,
                                            $"Problemas ao salvar integração de produto | Error.mensagem: {ex.Message}"));
                                    }
                                }
                            }
                            else if (millenniumData.EnableProductKit)
                            {
                                var listProductMessage = new ShopifyListERPFullProductMessage
                                {
                                    ExternalId = product.produto.ToString(),
                                    IntegrationId = millenniumData.EnableSaveIntegrationInformations ? integration.Id : (Guid?)null
                                };

                                var serviceBusMessage = new ServiceBusMessage(listProductMessage);
                                await millenniumFullProductQueueClient.SendAsync(serviceBusMessage.GetMessage(listProductMessage.ExternalId));
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Warning("MillenniumService:594 - Error in ListNewProduct | Error: {0}", ex.Message);
                        }
                    }

                    if (millenniumData.IntegrationType == IntegrationType.SellerCenter)
                    {
                        var message = new Product.SummarySellerCenter(itens);

                        await queueSellerCenter.SendMessages(
                            (queueSellerCenter.ProcessVariationOptionsProductQueue, message, message.Variants.Count > 0),
                            (queueSellerCenter.ProcessCategoriesProductQueue, message, message.Categories.Count > 0),
                            (queueSellerCenter.ProcessManufacturersProductQueue, message, message.Manufacturers.Count > 0));
                    }

                    if (millenniumData.ControlProductByUpdateDate)
                    {
                        transId.MillenniumLastUpdateDate = newMillenniumLastUpdateDate ?? transId.MillenniumLastUpdateDate;
                        process.FinalUpdateDate = (DateTime)transId.MillenniumLastUpdateDate;
                    }
                    else
                    {
                        transId.Value = newTransIdValue ?? transId.Value;
                        process.FinalTransId = transId.Value;
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewProductProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de produto | Error.mensagem: {ex.Message}",
                                LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                $"Problemas ao salvar processo de integração de preço | Error.mensagem: {ex.Message}"));
                        }
                    }

                    tenant.MillenniumData.SetTransId(transId);
                    await _tenantRepository.CommitAsync(cancellationToken);

                    return new ReturnMessage { Result = Result.OK };
                }
                else
                {
                    if (millenniumData.ControlProductByUpdateDate)
                    {
                        process.FinalUpdateDate = transId.MillenniumLastUpdateDate;
                    }
                    else
                    {
                        process.FinalTransId = transId.Value;
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewProductProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    return new ReturnMessage { Result = Result.Error };
                }
            }
            catch (Exception ex)
            {
                if (millenniumData.EnableSaveProcessIntegrations)
                {
                    try
                    {
                        if (ex.Message == "Same registers")
                        {
                            process.Exception = ex.Message;
                        }
                        else if (ex.Message.Contains("Product without price"))
                        {
                            process.Exception = ex.Message;
                        }
                        else
                            process.Exception = $"StackTrace: {ex.StackTrace} | ErroMessage: {ex.Message}";


                        _millenniumNewProductProcessRepository.Save(process);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error(ex, $"Problemas ao salvar processo de integração de produto | Error.mensagem: {exx.Message}",
                            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                            $"Problemas ao salvar processo de integração de produto | Error.mensagem: {exx.Message}"));
                    }
                }

                throw ex;
            }
        }

        public async Task<ReturnMessage> ListNewPrices(MillenniumData millenniumData,
                                    QueueClient millenniumFullProductQueueClient,
                                    QueueClient shopifyPriceQueueClient,
                                    SellerCenterQueue.Queues queues,
                                    CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantRepository.GetById(millenniumData.Id, cancellationToken);
            var transId = tenant.MillenniumData.GetTransId(TransIdType.PrecoDeTabela);

            ReturnMessage<MillenniumApiListPricesResult> priceList;

            var lastRetrievedIdRange = _millenniumNewPricesProcessRepository.GetLastIdProcess(millenniumData.Id);

            var process = new MillenniumNewPricesProcess()
            {
                Id = Guid.NewGuid(),
                TenantId = millenniumData.Id,
                ProcessDate = DateTime.Now,
                Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25,
            };

            if (millenniumData.ControlPriceByUpdateDate)
            {
                process.InitialUpdateDate = transId.MillenniumLastUpdateDate != null ? transId.MillenniumLastUpdateDate : new DateTime(2000, 01, 01);
            }
            else
            {
                process.InitialTransId = transId.Value;
            }

            if (millenniumData.EnableSaveProcessIntegrations)
            {
                try
                {
                    _millenniumNewPricesProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo de integração de preço | Error.mensagem: {ex.Message}",
                            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                            $"Problemas ao salvar processo de integração de preço | Error.mensagem: {ex.Message}"));
                }
            }

            try
            {
                if (millenniumData.ControlPriceByUpdateDate)
                {
                    priceList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListPricesResult>>(
                       new MillenniumApiListPricesRequest
                       {
                           DataAtualizacao = transId.MillenniumLastUpdateDate != null ? ((DateTime)transId.MillenniumLastUpdateDate).ToString("yyyy-MM-dd HH:mm:ss") : "2000-01-01",
                           Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                       }, cancellationToken);
                }
                else
                {
                    priceList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListPricesResult>>(
                       new MillenniumApiListPricesRequest
                       {
                           TransId = transId.Value,
                           Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                       }, cancellationToken);
                }

                if (priceList.Result == Result.Error)
                {
                    var returnMessage = new ReturnMessage { Result = Result.Error, Error = priceList.Error };

                    process.MillenniumResult = JsonConvert.SerializeObject(returnMessage);

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewPricesProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                   LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                   $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    return returnMessage;
                }

                long? newTransIdValue = null;
                DateTime? newMillenniumLastUpdateDate = null;

                if (priceList.Data.value.Any())
                {
                    process.TotalCount = priceList.Data.value.Count();
                    var prices = priceList.Data.GetValues();

                    var newFirstAndLast = new LastFirstAndLastIds
                    {
                        FirstId = $"{prices?.FirstOrDefault().produto}{prices?.FirstOrDefault().trans_id}{ConvertEpochToDateTime(prices?.FirstOrDefault().data_atualizacao)}",
                        LastId = $"{prices?.LastOrDefault().produto}{prices?.LastOrDefault().trans_id}{ConvertEpochToDateTime(prices?.LastOrDefault().data_atualizacao)}"
                    };

                    process.FirstPrice = newFirstAndLast.FirstId;
                    process.LastPrice = newFirstAndLast.LastId;

                    if (millenniumData.ControlPriceByUpdateDate)
                    {
                        if (lastRetrievedIdRange != null && !string.IsNullOrEmpty(lastRetrievedIdRange.FirstPrice)
                             && !string.IsNullOrEmpty(lastRetrievedIdRange.LastPrice))
                        {
                            if (((lastRetrievedIdRange.Exception == null && lastRetrievedIdRange.FinalUpdateDate != null)
                                || (lastRetrievedIdRange.Exception == "Same registers" && lastRetrievedIdRange.FinalUpdateDate == null))
                                                     && lastRetrievedIdRange.MillenniumResult == null
                                                     && lastRetrievedIdRange.TotalCount == process.TotalCount
                                                     && lastRetrievedIdRange.FirstPrice == newFirstAndLast.FirstId
                                                     && lastRetrievedIdRange.LastPrice == newFirstAndLast.LastId
                                                     && lastRetrievedIdRange.FinalUpdateDate != null)
                            {
                                throw new Exception("Same registers");
                            }
                        }
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewPricesProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                        $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));

                        }
                    }


                    if (millenniumData.ControlPriceByUpdateDate)
                    {
                        newMillenniumLastUpdateDate = ConvertEpochToDateTime(priceList.Data.value.Last().data_atualizacao);
                        if (newMillenniumLastUpdateDate == null || (newMillenniumLastUpdateDate < transId.MillenniumLastUpdateDate))
                            throw new Exception($"new newMillenniumLastUpdateDate {newMillenniumLastUpdateDate} is less than old value {transId.MillenniumLastUpdateDate} or null");
                    }
                    else
                    {
                        newTransIdValue = priceList.Data.value.OrderByDescending(o => o.trans_id).FirstOrDefault().trans_id;

                        if (newTransIdValue <= transId.Value)
                            throw new Exception($"new transid value {newTransIdValue} is less than old value {transId.Value}");
                    }

                    if (millenniumData.IntegrationType == Domain.Enums.IntegrationType.SellerCenter)
                    {
                        var message = MessageHelpers.TransformToMessage<SellerCenterUpdatePriceAndStockMessage>(priceList.Data.value);
                        await queues.UpdatePriceQueue.ScheduleMessageAsyncSafe(message, DateTime.UtcNow.AddMinutes(2));

                    }
                    else
                    {
                        foreach (var price in priceList.Data.value)
                        {
                            try
                            {
                                var integration = new MillenniumProductPriceIntegration();

                                if (millenniumData.EnableSaveIntegrationInformations)
                                {
                                    try
                                    {
                                        integration = new MillenniumProductPriceIntegration()
                                        {
                                            Id = Guid.NewGuid(),
                                            TenantId = millenniumData.Id,
                                            ProductId = price.produto.ToString(),
                                            ProductSku = price.sku,
                                            Payload = JsonConvert.SerializeObject(price),
                                            Status = IntegrationStatus.Received,
                                            IntegrationDate = DateTime.Now,
                                            MillenniumNewPriceProcessId = process.Id
                                        };

                                        _millenniumProductPriceIntegrationRepository.Save(integration);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex, $"Problemas ao salvar integração de preço | Error.mensagem: {ex.Message}",
                                           LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), integration, millenniumData,
                                           $"Problemas ao salvar integração de preço | Error.mensagem: {ex.Message}"));
                                    }
                                }

                                if (price.preco1 == null)
                                {
                                    //if price is null, get the full product because there could be a change in sku status
                                    var listProductMessage = new ShopifyListERPFullProductMessage
                                    {
                                        ExternalId = price.produto.ToString(),
                                        IntegrationId = millenniumData.EnableSaveIntegrationInformations ? integration.Id : (Guid?)null
                                    };

                                    var serviceBusMessage = new ServiceBusMessage(listProductMessage);
                                    await millenniumFullProductQueueClient.SendAsync(serviceBusMessage.GetMessage(listProductMessage.ExternalId));

                                    if (millenniumData.EnableSaveIntegrationInformations)
                                    {
                                        integration.Status = IntegrationStatus.SendendToShopifyQueue;
                                    }
                                }
                                else if (millenniumData.HasZeroedPriceCase)
                                {
                                    _logger.Warning("HasZeroedPriceCase tenantId: {0}", millenniumData.Id);
                                    var pivotValue = price.preco1.Value > 0 ? price.preco1.Value : price.preco2 ?? 0;

                                    var compareAtPrice = price.preco2 ?? pivotValue;
                                    var priceMessage = new ShopifyUpdatePriceMessage
                                    {
                                        ExternalProductId = price.produto.ToString(),
                                        Value = new Product.SkuPrice
                                        {
                                            Sku = GetSkuField(price, millenniumData),
                                            Price = pivotValue,
                                            CompareAtPrice = compareAtPrice > pivotValue ? compareAtPrice : (decimal?)null
                                        },
                                        IntegrationId = millenniumData.EnableSaveIntegrationInformations ? integration.Id : (Guid?)null
                                    };
                                    var serviceBusMessage = new ServiceBusMessage(priceMessage);
                                    _ = shopifyPriceQueueClient.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid()));

                                    if (millenniumData.EnableSaveIntegrationInformations)
                                    {
                                        integration.Status = IntegrationStatus.SendendToShopifyQueue;
                                    }
                                }
                                else
                                {
                                    var compareAtPrice = price.preco2 ?? price.preco1.Value;
                                    var priceMessage = new ShopifyUpdatePriceMessage
                                    {
                                        ExternalProductId = price.produto.ToString(),
                                        Value = new Product.SkuPrice
                                        {
                                            Sku = GetSkuField(price, millenniumData),
                                            Price = price.preco1.Value,
                                            CompareAtPrice = compareAtPrice > price.preco1.Value ? compareAtPrice : (decimal?)null
                                        },
                                        IntegrationId = millenniumData.EnableSaveIntegrationInformations ? integration.Id : (Guid?)null
                                    };
                                    var serviceBusMessage = new ServiceBusMessage(priceMessage);
                                    _ = shopifyPriceQueueClient.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid()));

                                    if (millenniumData.EnableSaveIntegrationInformations)
                                    {
                                        integration.Status = IntegrationStatus.SendendToShopifyQueue;
                                    }
                                }

                                if (millenniumData.EnableSaveIntegrationInformations)
                                {
                                    try
                                    {
                                        _millenniumProductPriceIntegrationRepository.Save(integration);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex, $"Problemas ao salvar alteração na integração de preço | Error.mensagem: {ex.Message}",
                                           LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), integration, millenniumData,
                                           $"Problemas ao salvar alteração na integração de preço | Error.mensagem: {ex.Message}"));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Warning("MillenniumService:968 | ErroMessage: {0}", ex.Message);
                            }
                        }
                    }

                    if (millenniumData.ControlPriceByUpdateDate)
                    {
                        transId.MillenniumLastUpdateDate = newMillenniumLastUpdateDate ?? transId.MillenniumLastUpdateDate;
                        process.FinalUpdateDate = transId.MillenniumLastUpdateDate;
                    }
                    else
                    {
                        transId.Value = newTransIdValue ?? transId.Value;
                        process.FinalTransId = transId.Value;
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        process.FinalTransId = newTransIdValue;

                        try
                        {
                            _millenniumNewPricesProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de preço | Error.mensagem: {ex.Message}",
                                       LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                       $"Problemas ao salvar processo de integração de preço | Error.mensagem: {ex.Message}"));
                        }
                    }

                    tenant.MillenniumData.SetTransId(transId);
                    await _tenantRepository.CommitAsync(cancellationToken);

                    return new ReturnMessage { Result = Result.OK };
                }
                else
                {
                    if (millenniumData.ControlPriceByUpdateDate)
                    {
                        process.FinalUpdateDate = transId.MillenniumLastUpdateDate;
                    }
                    else
                    {
                        process.FinalTransId = transId.Value;
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewPricesProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                      LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                      $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    return new ReturnMessage { Result = Result.Error };
                }
            }
            catch (Exception ex)
            {
                if (millenniumData.EnableSaveProcessIntegrations)
                {
                    try
                    {
                        process.Exception = ex.Message;

                        _millenniumNewPricesProcessRepository.Save(process);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error(ex, $"Problemas ao salvar processo de integração de produto | Error.mensagem: {exx.Message}",
                                      LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                      $"Problemas ao salvar processo de integração de produto | Error.mensagem: {exx.Message}"));
                    }
                }

                throw ex;
            }
        }

        public async Task<ReturnMessage> ListNewStocks(MillenniumData millenniumData,
                                                        SellerCenterQueue.Queues queues,
                                                        QueueClient shopifyStockQueueClient,
                                                        CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantRepository.GetById(millenniumData.Id, cancellationToken);
            var transId = tenant.MillenniumData.GetTransId(TransIdType.SaldoDeEstoque);

            async Task<ReturnMessage<MillenniumApiListStocksResult>> getSaldoEstoque(MillenniumApiListStocksRequest body)
                => await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListStocksResult>>(body, cancellationToken);

            ReturnMessage<MillenniumApiListStocksResult> stockList;

            var lastRetrievedIdRange = _millenniumNewStockProcessRepository.GetLastIdProcess(millenniumData.Id);

            var process = new MillenniumNewStockProcess()
            {
                Id = Guid.NewGuid(),
                TenantId = millenniumData.Id,
                ProcessDate = DateTime.Now,
                Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25,
            };

            if (millenniumData.ControlStockByUpdateDate)
            {
                process.InitialUpdateDate = transId.MillenniumLastUpdateDate != null ? transId.MillenniumLastUpdateDate : new DateTime(2000, 01, 01);
            }
            else
            {
                process.InitialTransId = transId.Value;
            }

            if (millenniumData.EnableSaveProcessIntegrations)
            {
                try
                {
                    _millenniumNewStockProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                      LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                      $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                }
            }

            try
            {
                if (millenniumData.ControlStockByUpdateDate)
                {
                    stockList = await getSaldoEstoque(new MillenniumApiListStocksRequest
                    {
                        DataAtualizacao = transId.MillenniumLastUpdateDate != null ? ((DateTime)transId.MillenniumLastUpdateDate).ToString("yyyy-MM-dd HH:mm:ss") : "2000-01-01",
                        Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                    });
                }
                else
                {
                    stockList = await getSaldoEstoque(new MillenniumApiListStocksRequest
                    {
                        TransId = transId.Value,
                        Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                    });
                }

                if (stockList.Result == Result.Error)
                {
                    var returnMessage = new ReturnMessage { Result = Result.Error, Error = stockList.Error };

                    process.MillenniumResult = JsonConvert.SerializeObject(returnMessage);

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewStockProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                      LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                      $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    return returnMessage;
                }


                int? newTransIdValue = null;
                DateTime? newMillenniumLastUpdateDate = null;

                if (stockList.Data.value.Any())
                {
                    process.TotalCount = stockList.Data.value.Count();

                    var stockDataValues = stockList.Data.GetValues();

                    var newFirstAndLast = new LastFirstAndLastIds
                    {
                        FirstId = $"{stockDataValues?.FirstOrDefault().produto}{stockDataValues?.FirstOrDefault().trans_id}{ConvertEpochToDateTime(stockDataValues?.FirstOrDefault().data_atualizacao)}",
                        LastId = $"{stockDataValues?.LastOrDefault().produto}{stockDataValues?.LastOrDefault().trans_id}{ConvertEpochToDateTime(stockDataValues?.LastOrDefault().data_atualizacao)}"
                    };

                    process.FirstId = newFirstAndLast.FirstId;
                    process.LastId = newFirstAndLast.LastId;

                    if (lastRetrievedIdRange != null
                        && !string.IsNullOrEmpty(lastRetrievedIdRange.FirstId)
                        && !string.IsNullOrEmpty(lastRetrievedIdRange.LastId))
                    {
                        if (((lastRetrievedIdRange.Exception == null && lastRetrievedIdRange.FinalUpdateDate != null)
                            || (lastRetrievedIdRange.Exception == "Same registers" && lastRetrievedIdRange.FinalUpdateDate == null))
                                                 && lastRetrievedIdRange.MillenniumResult == null
                                                 && lastRetrievedIdRange.TotalCount == process.TotalCount
                                                 && lastRetrievedIdRange.FirstId == newFirstAndLast.FirstId
                                                 && lastRetrievedIdRange.LastId == newFirstAndLast.LastId
                                                 && lastRetrievedIdRange.FinalUpdateDate != null)
                        {
                            throw new Exception("Same registers");
                        }
                    }


                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewStockProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                      LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                      $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    if (millenniumData.ControlStockByUpdateDate)
                    {
                        newMillenniumLastUpdateDate = ConvertEpochToDateTime(stockList.Data.value.Last().data_atualizacao);
                        if (newMillenniumLastUpdateDate == null || (newMillenniumLastUpdateDate < transId.MillenniumLastUpdateDate))
                            throw new Exception($"new newMillenniumLastUpdateDate {newMillenniumLastUpdateDate} is less than old value {transId.MillenniumLastUpdateDate} or null");
                    }
                    else
                    {
                        newTransIdValue = stockList.Data.value.OrderByDescending(o => o.trans_id).FirstOrDefault().trans_id;
                        if (newTransIdValue <= transId.Value)
                            throw new Exception($"new transid value {newTransIdValue} is less than old value {transId.Value}");
                    }


                    List<Task> sendMessages = new List<Task>();

                    foreach (var stock in stockList.Data.value)
                    {
                        try
                        {
                            var integration = new MillenniumProductStockIntegration();

                            if (millenniumData.EnableSaveIntegrationInformations)
                            {
                                try
                                {
                                    integration = new MillenniumProductStockIntegration()
                                    {
                                        Id = Guid.NewGuid(),
                                        TenantId = millenniumData.Id,
                                        ProductId = stock.produto.ToString(),
                                        ProductSku = stock.sku,
                                        Payload = JsonConvert.SerializeObject(stock),
                                        Status = IntegrationStatus.Received,
                                        IntegrationDate = DateTime.Now,
                                        MillenniumNewStockProcessId = process.Id
                                    };

                                    _millenniumProductStockIntegrationRepository.Save(integration);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex, $"Problemas ao salvar integração de stock | Error.mensagem: {ex.Message}",
                                          LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), integration, millenniumData,
                                          $"Problemas ao salvar integração de stock | Error.mensagem: {ex.Message}"));
                                }
                            }

                            if (millenniumData.IntegrationType == Domain.Enums.IntegrationType.SellerCenter)
                            {
                                var stockMessage = new SellerCenterUpdateStockProductMessage
                                {
                                    ExternalProductId = stock.produto.ToString(),
                                    Stock = new SellerCenterUpdateStockProductMessage.ValueItem
                                    {
                                        Sku = stock.sku,
                                        Quantity = stock.saldo_vitrine_sem_reserva
                                    }
                                };
                                sendMessages.Add(queues.UpdateStockProductQueue.ScheduleMessageAsyncSafe(stockMessage, DateTime.UtcNow.AddMinutes(3)));
                            }
                            else
                            {
                                if (stock.componente_kit && millenniumData.EnableProductKit)
                                {
                                    var shopifyUpdateProductKitMessage = new ServiceBusMessage(new ShopifyUpdateProductKitMessage
                                    {
                                        ExternalProductId = stock.produto
                                    });

                                    QueueClient queue = _tenantService.GetQueueClient(tenant, ShopifyQueue.UpdateProductKit);

                                    await queue.SendAsync(shopifyUpdateProductKitMessage.GetMessage(stock.produto, 2));
                                    await queue.CloseAsync();
                                }

                                var stockMessage = new ShopifyUpdateStockMessage
                                {
                                    ExternalProductId = stock.produto.ToString(),
                                    Value = new Product.SkuStock
                                    {
                                        Sku = GetSkuField(stock, millenniumData),
                                        Quantity = stock.saldo_vitrine_sem_reserva,
                                        Locations = string.IsNullOrEmpty(stock.cod_filial)
                                            ? null
                                            : new List<Product.SkuStock.Mulilocation>
                                            {
                                               new Product.SkuStock.Mulilocation
                                               {
                                                   Quantity = stock.saldo_vitrine_sem_reserva,
                                                   ErpLocationId = stock.cod_filial
                                               }
                                            }
                                    },
                                    IntegrationId = millenniumData.EnableSaveIntegrationInformations ? integration.Id : (Guid?)null
                                };

                                var serviceBusMessage = new ServiceBusMessage(stockMessage);

                                _ = shopifyStockQueueClient.SendAsync(serviceBusMessage.GetMessage(stockMessage.Value.Sku));

                                if (millenniumData.EnableSaveIntegrationInformations)
                                {
                                    integration.Status = IntegrationStatus.SendendToShopifyQueue;

                                    try
                                    {
                                        _millenniumProductStockIntegrationRepository.Save(integration);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Warning("Problemas ao salvar alteração na integração de stock | Error.mensagem: {0}",
                                          LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), integration, millenniumData,
                                          $"Problemas ao salvar alteração na integração de stock | Error.mensagem: {ex.Message}"));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning("MillenniumService:1299 | ErrorMessage: {0}", ex.Message);
                        }
                    }

                    if (millenniumData.ControlStockByUpdateDate)
                    {
                        transId.MillenniumLastUpdateDate = newMillenniumLastUpdateDate ?? transId.MillenniumLastUpdateDate;
                        process.FinalUpdateDate = transId.MillenniumLastUpdateDate;
                    }
                    else
                    {
                        transId.Value = newTransIdValue ?? transId.Value;
                        process.FinalTransId = transId.Value;
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewStockProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                     LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                     $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    tenant.MillenniumData.SetTransId(transId);
                    await _tenantRepository.CommitAsync(cancellationToken);
                    return new ReturnMessage { Result = Result.OK };
                }
                else
                {
                    if (millenniumData.ControlStockByUpdateDate)
                    {
                        process.FinalUpdateDate = transId.MillenniumLastUpdateDate;
                    }
                    else
                    {
                        process.FinalTransId = transId.Value;
                    }

                    if (millenniumData.EnableSaveProcessIntegrations)
                    {
                        try
                        {
                            _millenniumNewStockProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}",
                                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                    $"Problemas ao salvar processo de integração de stock | Error.mensagem: {ex.Message}"));
                        }
                    }

                    return new ReturnMessage { Result = Result.Error };
                }
            }
            catch (Exception ex)
            {
                if (millenniumData.EnableSaveProcessIntegrations)
                {
                    try
                    {
                        process.Exception = ex.Message;

                        _millenniumNewStockProcessRepository.Save(process);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error(exx, $"Problemas ao salvar processo de integração de produto | Error.mensagem: {exx.Message}",
                                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), process, millenniumData,
                                    $"Problemas ao salvar processo de integração de produto | Error.mensagem: {exx.Message}"));
                    }
                }

                throw ex;
            }
        }

        DateTime? ConvertTimestampToDateTime(string time)
        {
            try
            {
                var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var timestamp = double.Parse(string.Join("", time.ToCharArray().Where(Char.IsDigit)));

                return origin.AddMilliseconds(timestamp);
            }
            catch
            {
                return null;
            }
        }

        DateTime? ConvertEpochToDateTime(string epoch)
        {
            try
            {
                var formatedEpoch = epoch.Split('(', ')')[1].Replace("-", "");
                var doubleEpoch = Double.Parse(formatedEpoch) * (1e-6);
                var span = TimeSpan.FromSeconds(doubleEpoch);

                var data = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(span.TotalMilliseconds);

                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

                return TimeZoneInfo.ConvertTimeFromUtc(data, cstZone);
            }
            catch
            {
                return null;
            }

        }

        public async Task<ReturnMessage> ListNewStockMto(MillenniumData millenniumData,
                                    SellerCenterQueue.Queues queues,
                                    QueueClient shopifyStockQueueClient,
                                    CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantRepository.GetById(millenniumData.Id, cancellationToken);
            var transId = tenant.MillenniumData.GetTransId(TransIdType.SaldoDeEstoqueMto);

            async Task<ReturnMessage<MillenniumApiListStockMtoResult>> getSaldoEstoque(MillenniumApiListStockMtoRequest body)
                => await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListStockMtoResult>>(body, cancellationToken);

            ReturnMessage<MillenniumApiListStockMtoResult> stockList;

            if (transId.Value == 0) //carga full
            {
                stockList = await getSaldoEstoque(new MillenniumApiListStockMtoRequest
                {
                    EstrategiaProducao = "2",
                    Filiais = new List<string> { "3" },
                    Top = 1
                }); ;
            }
            else
            {
                stockList = await getSaldoEstoque(new MillenniumApiListStockMtoRequest
                {
                    EstrategiaProducao = "2",
                    Filiais = new List<string> { "3" },
                    Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                });
            }


            if (stockList.Result == Result.Error)
            {
                _logger.Warning($"MillenniumService - ListNewStockMto | {stockList.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListStockMtoRequest
                {
                    EstrategiaProducao = "2",
                    Filiais = new List<string> { "3" },
                    Top = 1
                }, millenniumData));
                return new ReturnMessage { Result = Result.Error, Error = stockList.Error };
            }


            if (stockList.Data.value.Any())
            {
                var newTransIdValue = stockList.Data.value.OrderByDescending(o => o.trans_id).FirstOrDefault().trans_id;

                if (newTransIdValue <= transId.Value)
                    throw new Exception($"new transid value {newTransIdValue} is less than old value {transId.Value}");

                List<Task> sendMessages = new List<Task>();

                foreach (var product in stockList.Data.value)
                {
                    foreach (var sku in product.sku)
                    {
                        foreach (var stock in sku.capacidade_producao)
                        {
                            if (millenniumData.IntegrationType == Domain.Enums.IntegrationType.SellerCenter)
                            {
                                var stockMessage = new SellerCenterUpdateStockProductMessage
                                {
                                    ExternalProductId = product.produto.ToString(),
                                    Stock = new SellerCenterUpdateStockProductMessage.ValueItem
                                    {
                                        Sku = sku.sku,
                                        Quantity = stock.capacidade_disponivel
                                    }
                                };
                                sendMessages.Add(queues.UpdateStockProductQueue.ScheduleMessageAsyncSafe(stockMessage, DateTime.UtcNow.AddMinutes(3)));
                            }
                            else
                            {
                                var stockMessage = new ShopifyUpdateStockMessage
                                {
                                    ExternalProductId = product.produto.ToString(),
                                    Value = new Product.SkuStock
                                    {
                                        Sku = sku.sku,
                                        Quantity = stock.capacidade_disponivel,
                                        Locations = string.IsNullOrEmpty(stock.cod_filial)
                                            ? null
                                            : new List<Product.SkuStock.Mulilocation> { new Product.SkuStock.Mulilocation { Quantity = stock.capacidade_disponivel, ErpLocationId = stock.cod_filial } }

                                    }
                                };

                                var serviceBusMessage = new ServiceBusMessage(stockMessage);
                                sendMessages.Add(shopifyStockQueueClient.SendAsync(serviceBusMessage.GetMessage(stockMessage.Value.Sku)));
                            }
                        }
                    }
                }

                await Task.WhenAll(sendMessages);

                transId.Value = newTransIdValue;
                tenant.MillenniumData.SetTransId(transId);
                await _tenantRepository.CommitAsync(cancellationToken);
                return new ReturnMessage { Result = Result.OK };
            }
            else
            {
                _logger.Warning($"MillenniumService - Error in ListProduct | MillenniumApiListStockMtoResult.value is empty",
                LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListStockMtoRequest
                {
                    EstrategiaProducao = "2",
                    Filiais = new List<string> { "3" },
                    Top = 1
                }, millenniumData));
                return new ReturnMessage { Result = Result.Error };
            }
        }

        public async Task<ReturnMessage> UpdateOrder(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, CancellationToken cancellationToken)
        {
            var process = RegisterLog(millenniumData, message);

            var typeCreationOrder = _createOrder.FirstOrDefault(w => w.TypeDiscount(millenniumData));

            try
            {
                _millenniumDomainService.ValidateNoteAtributtes(millenniumData, message);

                var orderList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListOrdersResult>>(
                new MillenniumApiListOrdersRequest
                {
                    ExternalOrderId = message.ExternalID
                }, cancellationToken);

                if (orderList.Result == Result.Error)
                    return RegisterExceptionOrderList(millenniumData, message, process, orderList);

                if (orderList.Data.value.Any() == false)
                {
                    var adjustmentValue = _millenniumDomainService.CalculateAdjusmentValue(message);
                    var issuerType = await GetIssuerType(millenniumData, message);
                    var milenniumApiCreateOrderPaymentDataRequest = CreateOrderPaymentDataRequest(millenniumData, message, adjustmentValue, issuerType);
                    var methodPayment = _millenniumDomainService.IsMethodPaymentValid(message);

                    if (millenniumData.EnableExtraPaymentInformation && !methodPayment)
                        milenniumApiCreateOrderPaymentDataRequest = await GetPaymentExtraInfo(millenniumData, message, milenniumApiCreateOrderPaymentDataRequest);

                    var createOrder = typeCreationOrder.CreateOrder(millenniumData, message, milenniumApiCreateOrderPaymentDataRequest, adjustmentValue);

                    var response = await _apiActorGroup.Ask<ReturnMessage>(createOrder, cancellationToken);
                    if (response.Result == Result.Error)
                    {
                        await ErrorOnCreatedOrder(millenniumData, message, process, createOrder, response, cancellationToken);
                    }

                    SavePayloadAfterCreateOrder(millenniumData, process, createOrder);

                    return new ReturnMessage { Result = Result.OK };
                }
                else
                {
                    var order = orderList.Data.value[0];
                    //(0=Aguardando Pagamento,1=Pagamento Confirmado,2=Em Separação,3=Despachado,4=Entregue,5=Cancelado,6=Problemas,7=Embarcado,8=Falha na Entega)                    
                    var status = order.status;
                    ReturnMessage response = null;

                    if (message.Cancelled)
                    {
                        if (new List<int> { MillenniumStatusPedido.AguardandoPagamento,
                                            MillenniumStatusPedido.PagamentoConfirmado,
                                            MillenniumStatusPedido.EmSeparacao,
                                            MillenniumStatusPedido.Despachado,
                                            MillenniumStatusPedido.Entregue,
                                            MillenniumStatusPedido.Problemas,
                                            MillenniumStatusPedido.Embarcado,
                                            MillenniumStatusPedido.FalhaNaEntega }.Contains(status))
                        {
                            var MillenniumApiUpdateOrderStatusRequest = new MillenniumApiUpdateOrderStatusRequest
                            {
                                status_pedidos = new List<MillenniumApiUpdateOrderStatusRequest.Status_Pedidos>
                                   {
                                      new MillenniumApiUpdateOrderStatusRequest.Status_Pedidos
                                      {
                                          cod_pedidov = order.cod_pedidov,
                                          status = MillenniumStatusPedido.Cancelado
                                      }
                                   }
                            };

                            response = await _apiActorGroup.Ask<ReturnMessage>(MillenniumApiUpdateOrderStatusRequest, cancellationToken);
                            if (millenniumData.EnableSaveIntegrationInformations)
                            {
                                try
                                {
                                    process.Payload = $"Message Update Shopify- {JsonConvert.SerializeObject(MillenniumApiUpdateOrderStatusRequest)}";
                                    _millenniumUpdateOrderProcessRepository.Save(process);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex, $"Problemas ao salvar processo de integração de pedido");
                                }
                            }
                        }
                    }
                    else if (message.Approved)
                    {
                        if (new List<int> { MillenniumStatusPedido.AguardandoPagamento }.Contains(status))
                        {
                            var MillenniumApiUpdateOrderStatusRequest = new MillenniumApiUpdateOrderStatusRequest
                            {
                                status_pedidos = new List<MillenniumApiUpdateOrderStatusRequest.Status_Pedidos>
                                   {
                                      new MillenniumApiUpdateOrderStatusRequest.Status_Pedidos
                                      {
                                          cod_pedidov = order.cod_pedidov,
                                          status = MillenniumStatusPedido.PagamentoConfirmado
                                      }
                                }
                            };

                            response = await _apiActorGroup.Ask<ReturnMessage>(MillenniumApiUpdateOrderStatusRequest, cancellationToken);
                            if (millenniumData.EnableSaveIntegrationInformations)
                            {
                                try
                                {
                                    process.Payload = $"Message Update Shopify- {JsonConvert.SerializeObject(MillenniumApiUpdateOrderStatusRequest)}";
                                    _millenniumUpdateOrderProcessRepository.Save(process);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex, $"Problemas ao salvar processo de integração de pedido");
                                }
                            }
                        }
                    }
                }

                return new ReturnMessage { Result = Result.OK };
            }
            catch (Exception ex)
            {
                if (millenniumData.EnableSaveIntegrationInformations)
                {
                    try
                    {
                        process.Exception = ex.Message;

                        _millenniumUpdateOrderProcessRepository.Save(process);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error(exx, $"Problemas ao salvar processo de integração de pedido (1)");
                    }
                }

                throw ex;
            }
        }

        void SavePayloadAfterCreateOrder(MillenniumData millenniumData, MillenniumUpdateOrderProcess process, MillenniumApiCreateOrderRequest createOrder)
        {
            if (millenniumData.EnableSaveIntegrationInformations)
            {
                try
                {
                    process.Payload = JsonConvert.SerializeObject(createOrder);
                    _millenniumUpdateOrderProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo de integração de pedido");
                }
            }
        }

        async Task ErrorOnCreatedOrder(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, MillenniumUpdateOrderProcess process, MillenniumApiCreateOrderRequest createOrder, ReturnMessage response, CancellationToken cancellationToken)
        {
            try
            {
                if (millenniumData.EnableSaveIntegrationInformations)
                {
                    try
                    {
                        process.Payload = $"{JsonConvert.SerializeObject(createOrder)} - With error.";
                        process.MillenniumResponse = JsonConvert.SerializeObject(response.Result);
                        _millenniumUpdateOrderProcessRepository.Save(process);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Problemas ao salvar processo de integração de pedido");
                    }
                }

                if (millenniumData.Id == Tenants.PitBull)
                {
                    var error = CleanInput(response.Error.Message);

                    var tagsErro = CutString(error);

                    var shopifyUpdateOrderTagNumberMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
                    {
                        ShopifyId = message.ID,
                        IntegrationStatus = message.GetOrderStatus(),
                        OrderExternalId = message.ExternalID,
                        OrderNumber = message.Number.ToString(),
                        CustomTags = tagsErro
                    });

                    var tenant = await _tenantRepository.GetById(millenniumData.Id, cancellationToken);
                    QueueClient queue = _tenantService.GetQueueClient(tenant, ShopifyQueue.UpdateOrderNumberTagQueue);

                    await queue.SendAsync(shopifyUpdateOrderTagNumberMessage.GetMessage(message.ID));
                    await queue.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning("Erro ao enfileirar msg para {0} (UpdateOrder) | Erro: {1}", ShopifyQueue.UpdateOrderNumberTagQueue, ex.Message);
            }

            throw response.Error;
        }

        MilenniumApiCreateOrderPaymentDataRequest CreateOrderPaymentDataRequest(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, decimal adjustmentValue, string descricaoTipo)
        {
            if (millenniumData.Id == Tenants.PitBull)
            {
                return new MilenniumApiCreateOrderPaymentDataRequest
                {
                    valor_inicial = _millenniumDomainService.CalculateInitialValue(message, adjustmentValue),
                    bandeira = (int)MillenniumIssuerType.OUTROS,
                    numparc = 0,
                    parcela = 0,
                    operadora = (int)millenniumData.OperatorType,
                    transacao_aprovada = millenniumData.EnabledApprovedTransaction ? "T" : null
                };
            }

            var bandeira = _millenniumDomainService.GetBandeira(message);
            var numeroParcelas = _millenniumDomainService.GetNumeroParcelas(message, descricaoTipo, bandeira);
            var valorInicial = _millenniumDomainService.CalculateInitialValue(message, adjustmentValue);

            var milenniumApiCreateOrderPaymentDataRequest = new MilenniumApiCreateOrderPaymentDataRequest
            {
                valor_inicial = valorInicial,
                valor_liquido = _millenniumDomainService.GetValuesWithFeesOrder(valorInicial, message),
                bandeira = (int)bandeira,
                DESC_TIPO = string.IsNullOrWhiteSpace(descricaoTipo) ? null : descricaoTipo,
                numparc = numeroParcelas,
                parcela = numeroParcelas,
                operadora = (int)millenniumData.OperatorType,
                transacao_aprovada = millenniumData.EnabledApprovedTransaction ? "T" : null
            };

            if (millenniumData.SendPaymentMethod)
                milenniumApiCreateOrderPaymentDataRequest.tipo_pgto = (int)_millenniumDomainService.GetTipoPgto(message);

            return milenniumApiCreateOrderPaymentDataRequest;
        }


        MillenniumApiCreateOrderRequest CreateOrder(MillenniumData millenniumData,
                                                           ShopifySendOrderToERPMessage message,
                                                           decimal adjustmentValue,
                                                           MilenniumApiCreateOrderPaymentDataRequest milenniumApiCreateOrderPaymentDataRequest)
        {
            var itens = new List<MilenniumApiCreateOrderProductRequest>();
            for (int i = 0; i < message.Items.Count; i++)
            {
                var item = message.Items[i];
                itens.Add(new MilenniumApiCreateOrderProductRequest
                {
                    cod_filial = _millenniumDomainService.GetLocation(millenniumData, item.LocationId),
                    quantidade = item.Quantity,
                    preco = item.Price,
                    sku = item.Sku,
                    encomenda = millenniumData.GetFlagEncomenda(),
                    obs_item = item.GetFlagGift(),
                    item = (i + 1).ToString(),
                });
            }

            var createOrder = new MillenniumApiCreateOrderRequest
            {
                //TODO -> PENDENTE DEFINICAO MILLENIUM
                //cod_filial = GetLocation(millenniumData,message.Items.FirstOrDefault()?.LocationId?.ToString()),
                cod_pedidov = message.ExternalID,
                data_emissao = message.CreatedAt.ToString("yyyy-MM-dd"),
                data_entrega = message.GetDataEntregaToString(),
                total = message.Subtotal,
                v_acerto = adjustmentValue == 0 ? (decimal?)null : adjustmentValue,
                acerto = adjustmentValue == 0 ? (decimal?)null : adjustmentValue / message.Subtotal * 100,
                aprovado = message.Approved,
                n_pedido_cliente = message.ID.ToString(),
                quantidade = message.Items.Sum(i => i.Quantity),
                v_frete = message.ShippingValue,
                origem_pedido = "SITE",
                vitrine = millenniumData.VitrineId,
                obs = message.vendor,
                nome_transportadora = message.CarrierName,
                mensagens = new List<MilenniumApiCreateOrderMessage> { new MilenniumApiCreateOrderMessage { texto = message.Note } },
                produtos = itens,
                dados_cliente = new List<MilenniumApiCreateOrderCustomerRequest>
                            {
                            new MilenniumApiCreateOrderCustomerRequest
                            {
                                nome = string.Concat(message.Customer.FirstName, " ", message.Customer.LastName),
                                data_aniversario = message.Customer.BirthDate.HasValue ? message.Customer.BirthDate.Value.ToString("yyyy-MM-dd") : null,
                                ddd_cel = message.Customer.DDD,
                                cel = message.Customer.Phone,
                                e_mail = message.Customer.Email,
                                obs = message.Customer.Note,
                                vitrine = millenniumData.VitrineId,
                                cpf = !millenniumData.DisableCustomerDocument ? message.Customer.Company : string.Empty,
                                ie = message.NoteAttributes.FirstOrDefault(n => n.Name == "aditional_info_extra_billing_IE")?.Value ?? null,
                                ufie = message.NoteAttributes.FirstOrDefault(n => n.Name == "aditional_info_extra_billing_UFIE")?.Value ?? null,
                                endereco = new List<MilenniumApiCreateOrderAddressRequest>
                                {
                                    new MilenniumApiCreateOrderAddressRequest
                                    {
                                        bairro = message.Customer.BillingAddress.District,
                                        cep = message.Customer.BillingAddress.ZipCode,
                                        cidade = message.Customer.BillingAddress.City,
                                        complemento = message.Customer.BillingAddress.Complement,
                                        contato = message.Customer.BillingAddress.Contact,
                                        estado = message.Customer.BillingAddress.State,
                                        ddd = message.Customer.BillingAddress.DDD,
                                        fone = message.Customer.BillingAddress.Phone,
                                        logradouro = message.Customer.BillingAddress.Address,
                                        numero = message.Customer.BillingAddress.Number
                                    }
                                },
                                endereco_entrega = new List<MilenniumApiCreateOrderAddressRequest>
                                {
                                    new MilenniumApiCreateOrderAddressRequest
                                    {
                                        bairro = message.Customer.DeliveryAddress.District,
                                        cep = message.Customer.DeliveryAddress.ZipCode,
                                        cidade = message.Customer.DeliveryAddress.City,
                                        complemento = message.Customer.DeliveryAddress.Complement,
                                        contato = message.Customer.DeliveryAddress.Contact,
                                        estado = message.Customer.DeliveryAddress.State,
                                        ddd = message.Customer.DeliveryAddress.DDD,
                                        fone = message.Customer.DeliveryAddress.Phone,
                                        logradouro = message.Customer.DeliveryAddress.Address,
                                        numero = message.Customer.DeliveryAddress.Number
                                    }
                                }
                            }
                            },
                lancamentos = new List<MilenniumApiCreateOrderPaymentDataRequest>
                        {
                            milenniumApiCreateOrderPaymentDataRequest
                        }

            };
            return createOrder;
        }
        async Task<MilenniumApiCreateOrderPaymentDataRequest> GetPaymentExtraInfo(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, MilenniumApiCreateOrderPaymentDataRequest milenniumApiCreateOrderPaymentDataRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message.Checkout_Token) || string.IsNullOrWhiteSpace(millenniumData.StoreDomainByBrasPag))
                    throw new Exception("MillenniumService - Erro: Checkout_Token ou StoreHandle sem dados");

                var infoPayment = millenniumData.UrlExtraPaymentInformation switch
                {
                    String brasPag when brasPag.Contains("cielo.azurewebsites.net") => await GetExtraInformationBrasPag(millenniumData, message, milenniumApiCreateOrderPaymentDataRequest),
                    String moip when moip.Contains("checkoutmoip.azurewebsites.net") => await GetExtraInformationMoip(millenniumData, message, milenniumApiCreateOrderPaymentDataRequest),
                    String mercadoPago when mercadoPago.Contains("api.mercadopago.com") => await GetExtraInformationMercadoPago(millenniumData, message, milenniumApiCreateOrderPaymentDataRequest),
                    _ => null
                };

                milenniumApiCreateOrderPaymentDataRequest = infoPayment ?? throw new Exception("MillenniumService - Erro: infoPayment é null");
            }
            catch (Exception ex)
            {
                var obj = new Dictionary<string, string>()
                            {
                                {"CheckoutToken", message.Checkout_Token},
                                {"StoreHandle",  millenniumData.StoreDomainByBrasPag},
                                {"urlbase", millenniumData.UrlExtraPaymentInformation },
                                {"ErrorMessage", ex.Message }
                            };

                _logger.Error(ex, "Problemas ao consultar endpoint PaymentBrasPag | {0}", JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = (serializer, error) => error.ErrorContext.Handled = true
                }));
                throw;
            }

            return milenniumApiCreateOrderPaymentDataRequest;
        }



        ReturnMessage RegisterExceptionOrderList(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, MillenniumUpdateOrderProcess process, ReturnMessage<MillenniumApiListOrdersResult> orderList)
        {
            if (millenniumData.EnableSaveIntegrationInformations)
            {
                try
                {
                    process.Exception = Newtonsoft.Json.JsonConvert.SerializeObject(orderList.Error);

                    _millenniumUpdateOrderProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo de integração de pedido (1)");
                }
            }

            _logger.Warning($"MillenniumService - Error in UpdateOrder | {orderList.Error.Message}",
            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, millenniumData));

            return new ReturnMessage { Result = Result.Error, Error = orderList.Error };
        }

        MillenniumUpdateOrderProcess RegisterLog(MillenniumData millenniumData, ShopifySendOrderToERPMessage message)
        {
            var process = new MillenniumUpdateOrderProcess()
            {
                Id = Guid.NewGuid(),
                TenantId = millenniumData.Id,
                ProcessDate = DateTime.Now,
                OrderId = message.ID,
                ShopifyListOrderProcessReferenceId = message.ShopifyListOrderProcessId
            };

            if (millenniumData.EnableSaveIntegrationInformations)
            {
                try
                {
                    _millenniumUpdateOrderProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo de integração de pedido (1)");
                }
            }

            return process;
        }

        async Task<string> GetIssuerType(MillenniumData millenniumData, ShopifySendOrderToERPMessage message)
        {
            if (millenniumData.Id == Tenants.PitBull)
            {
                return string.Empty;
            }

            var issuerType = await _methodPaymentRepository.GetMillenniumIssuerTypeAsync(millenniumData.Id, message);
            return issuerType;
        }

        public async Task<ReturnMessage> UpdateOrderNew(MillenniumData millenniumData, CreateOrderMessage message, CancellationToken cancellationToken)
        {
            var orderList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListOrdersResult>>(
                new MillenniumApiListOrdersRequest
                {
                    ExternalOrderId = message.Data.Number.ToString()
                }, cancellationToken
            );

            message.Data.VitrineId = millenniumData.VitrineId;
            message.Data.DisableCustomerDocument = millenniumData.DisableCustomerDocument;
            message.Data.OperatorType = millenniumData.OperatorType;

            var request = _mapper.Map<MillenniumApiCreateOrderRequest>(message.Data);

            if (orderList.Result == Result.Error)
            {
                _logger.Warning($"MillenniumService - Error in UpdateOrderNew | {orderList.Error.Message}",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, millenniumData));
                return new ReturnMessage { Result = Result.Error, Error = orderList.Error };
            }


            if (orderList.Data.value.Any() == false)
            {
                var response = await _apiActorGroup.Ask<ReturnMessage>(request, cancellationToken);
                return response;
            }
            else
            {
                var order = orderList.Data.value[0];
                var status = order.status;

                if (message.Data.Buyer.ApprovalStatus == Domain.Models.SellerCenter.API.Orders.ValueObjects.ApprovalStatus.Disapproved)
                {
                    if (new List<int> { MillenniumStatusPedido.AguardandoPagamento,
                                        MillenniumStatusPedido.PagamentoConfirmado,
                                        MillenniumStatusPedido.EmSeparacao,
                                        MillenniumStatusPedido.Despachado,
                                        MillenniumStatusPedido.Entregue,
                                        MillenniumStatusPedido.Problemas,
                                        MillenniumStatusPedido.Embarcado,
                                        MillenniumStatusPedido.FalhaNaEntega }.Contains(status))
                    {
                        var response = await _apiActorGroup.Ask<ReturnMessage>(
                           new MillenniumApiUpdateOrderStatusRequest
                           {
                               status_pedidos = new List<MillenniumApiUpdateOrderStatusRequest.Status_Pedidos>
                              {
                                  new MillenniumApiUpdateOrderStatusRequest.Status_Pedidos
                                  {
                                      cod_pedidov = order.cod_pedidov,
                                      status = 5
                                  }
                              }
                           }, cancellationToken
                        );
                        return response;
                    }
                }
                else if (message.Data.Buyer.ApprovalStatus == Domain.Models.SellerCenter.API.Orders.ValueObjects.ApprovalStatus.Approved)
                {
                    if (new List<int> { MillenniumStatusPedido.AguardandoPagamento }.Contains(status))
                    {
                        var response = await _apiActorGroup.Ask<ReturnMessage>(
                           new MillenniumApiUpdateOrderStatusRequest
                           {
                               status_pedidos = new List<MillenniumApiUpdateOrderStatusRequest.Status_Pedidos>
                              {
                                  new MillenniumApiUpdateOrderStatusRequest.Status_Pedidos
                                  {
                                      cod_pedidov = order.cod_pedidov,
                                      status = 1
                                  }
                              }
                           }, cancellationToken
                        );
                        return response;
                    }
                }
            }

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> ListNewOrders(MillenniumData millenniumData, QueueClient shopifyUpdateOrderStatusQueueClient, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetById(millenniumData.Id, cancellationToken);
            var transId = tenant.MillenniumData.GetTransId(TransIdType.ListaPedidos);

            async Task<ReturnMessage<MillenniumApiListOrdersResult>> getOrderRequest(MillenniumApiListOrdersRequest body) => await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListOrdersResult>>(body, cancellationToken);

            if (millenniumData.Retry)
            {
                var retry = await getOrderRequest(new MillenniumApiListOrdersRequest
                {
                    DataInicial = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd"),
                    DataFinal = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Top = 1
                });

                if (retry.Result == Result.Error)
                {
                    _logger.Warning($"MillenniumService - Error in ListNewOrders | {retry.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListOrdersRequest
                        {
                            DataInicial = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd"),
                            DataFinal = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            Top = 1
                        }, millenniumData));
                    return new ReturnMessage { Result = Result.Error, Error = retry.Error };
                }

                if (retry.Data.value.Any())
                {
                    transId.Value = retry.Data.value.FirstOrDefault().trans_id;
                    tenant.MillenniumData.SetTransId(transId);
                    await _tenantRepository.CommitAsync(cancellationToken);
                }
            }

            var orderList = await getOrderRequest(
                new MillenniumApiListOrdersRequest
                {
                    TransId = transId.Value,
                    Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                });

            if (orderList.Result == Result.Error)
            {
                _logger.Warning("MillenniumService - Error in ListNewOrders | {0}",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(),
                    new MillenniumApiListOrdersRequest
                    {
                        TransId = transId.Value,
                        Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                    }, millenniumData, orderList.Error.Message));
                return new ReturnMessage { Result = Result.Error, Error = orderList.Error };
            }


            if (orderList.Data.value.Any())
            {
                var newTransIdValue = orderList.Data.value.OrderByDescending(o => o.trans_id).FirstOrDefault().trans_id;

                if (newTransIdValue <= transId.Value)
                    throw new Exception($"new transid value {newTransIdValue} is less than old value {transId.Value}");

                var response = new ReturnMessage { Result = Result.Error };

                switch (tenant.IntegrationType)
                {
                    case Domain.Enums.IntegrationType.Shopify:
                        response = await SendShopifyOrderStatusUpdateMessageAsync(shopifyUpdateOrderStatusQueueClient, orderList.Data.value, millenniumData, cancellationToken);
                        break;
                    case Domain.Enums.IntegrationType.SellerCenter:
                        response = await SendSellerCenterOrderStatusUpdateMessageAsync(shopifyUpdateOrderStatusQueueClient, orderList.Data.value, cancellationToken);
                        break;
                    default:
                        break;
                }

                if (response.Result == Result.Error)
                {
                    _logger.Warning($"MillenniumService - Error in ListNewOrders | {response.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListOrdersRequest
                        {
                            TransId = transId.Value,
                            Top = millenniumData.NumberOfItensPerAPIQuery > 0 ? millenniumData.NumberOfItensPerAPIQuery : 25
                        }, millenniumData));
                    return new ReturnMessage { Result = Result.Error, Error = response.Error };
                }


                transId.Value = newTransIdValue;
                tenant.MillenniumData.SetTransId(transId);
                await _tenantRepository.CommitAsync(cancellationToken);
                return new ReturnMessage { Result = Result.OK };
            }

            return new ReturnMessage { Result = Result.Error };
        }


        public async Task<ReturnMessage> ListOrder(string externalOrderId, MillenniumData millenniumData, QueueClient shopifyUpdateOrderStatusQueueClient, CancellationToken cancellationToken)
        {
            var orderList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListOrdersResult>>(
                new MillenniumApiListOrdersRequest
                {
                    ExternalOrderId = externalOrderId
                }, cancellationToken
            );

            if (orderList.Result == Result.Error)
                return new ReturnMessage { Result = Result.Error, Error = orderList.Error };

            if (orderList.Data.value.Any())
            {
                var order = orderList.Data.value[0];
                var response = await SendShopifyOrderStatusUpdateMessageAsync(shopifyUpdateOrderStatusQueueClient, new List<MillenniumApiListOrdersResult.Value> { order }, millenniumData, cancellationToken);
                if (response.Result == Result.Error)
                {
                    _logger.Warning($"MillenniumService - Error in ListOrder | {response.Error.Message}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), order, millenniumData));
                    return new ReturnMessage { Result = Result.Error, Error = response.Error };
                }

                return new ReturnMessage { Result = Result.OK };
            }
            else
            {
                throw new Exception($"order {externalOrderId} not found");
            }
        }

        /// <summary>
        /// Obtem o preco de um produto
        /// </summary>
        /// <param name="productId">codigo produto</param>
        /// <param name="millenniumData"></param>
        /// <param name="queues"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ReturnMessage> GetPriceProduct(string productId, MillenniumData millenniumData,
                                   SellerCenterQueue.Queues queues,
                                   CancellationToken cancellationToken = default)
        {
            var priceList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListPricesResult>>(
                new MillenniumApiListPricesRequest
                {
                    ProductId = Convert.ToInt32(productId),
                }, cancellationToken
            );

            if (priceList.Result == Result.Error)
                return new ReturnMessage { Result = Result.Error, Error = priceList.Error };

            if (priceList.Data.value.Any())
            {
                var message = MessageHelpers.TransformToMessage<SellerCenterUpdatePriceAndStockMessage>(priceList.Data.value, millenniumData.HasZeroedPriceCase);
                if (millenniumData.HasZeroedPriceCase)
                {
                    var listZeroed = message.Values.Any() ? message.Values.Where(x => x.Price.Equals(0)).Select(x => x.Sku) : default;
                    _logger.Info("productId {0} in routine GetPriceProduct. list of zeroed's price sku {1}", productId, listZeroed);
                }
                await queues.UpdatePriceQueue.SendAsyncSafe(message);

                return new ReturnMessage { Result = Result.OK };
            }
            else
            {
                _logger.Warning($"MillenniumService - Error in GetPriceProduct | MillenniumApiListPricesResult.value is empty",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiListPricesRequest
                    {
                        ProductId = Convert.ToInt32(productId),
                    }, millenniumData));

                return new ReturnMessage { Result = Result.Error };
            }
        }

        public async Task<ReturnMessage> ProcessProductImage(ProcessProductImageMessage message,
                                    MillenniumData millenniumData,
                                    QueueClient shopifyUpdateProductImageQueueClient,
                                    CancellationToken cancellationToken = default)
        {
            var imageIntegration = new MillenniumImageIntegration();

            if (millenniumData.EnableSaveIntegrationInformations)
            {
                try
                {
                    imageIntegration = new MillenniumImageIntegration()
                    {
                        Id = Guid.NewGuid(),
                        TenantId = millenniumData.Id,
                        IdProduto = message.IdProduto,
                        ExternalId = message.ExternalId,
                        Payload = JsonConvert.SerializeObject(message),
                        Status = IntegrationStatus.Received,
                        IntegrationDate = DateTime.Now,
                        MillenniumIntegrationProductReferenceId = message.ProductIntegrationRefenceId,
                    };

                    _millenniumImageIntegrationRepository.Save(imageIntegration);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar integração de imagem");
                }
            }

            try
            {
                var productList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiGetListIdFotoResult>>(
                                    new MillenniumApiGetListIdFotoRequest
                                    {
                                        CodProduto = message.IdProduto
                                    }, cancellationToken
                                );

                if (productList.Result == Result.Error)
                {
                    _logger.Warning($"MillenniumService - Error in ProcessProductImage | {productList.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, millenniumData));
                    return new ReturnMessage { Result = Result.Error, Error = productList.Error };
                }

                if (millenniumData.EnableSaveIntegrationInformations)
                {
                    try
                    {
                        imageIntegration.MillenniumResult = JsonConvert.SerializeObject(productList);

                        _millenniumImageIntegrationRepository.Save(imageIntegration);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Problemas ao salvar erro na integração de imagem");
                    }
                }

                var imagesUploadList = new List<(bool success, string path)> { };

                string cod_produto = message.IdProduto.ToString();

                if (!productList.Data.value.Any())
                {
                    var updateProductImageMessage = new ShopifyUpdateProductImagesMessage
                    {
                        ExternalProductId = millenniumData.SplitEnabled ? $"{message.IdProduto}_{message.ExternalId}" : cod_produto,
                        Images = new ProductImages
                        {
                            ImageUrls = new List<string>(),
                            SkuImages = new List<ProductImages.SkuImage>(),
                        },
                        ReferenceIntegrationId = message.ProductIntegrationRefenceId
                    };

                    var serviceBusMessage = new ServiceBusMessage(updateProductImageMessage);
                    await shopifyUpdateProductImageQueueClient.SendAsync(serviceBusMessage.GetMessage(updateProductImageMessage.ExternalProductId));

                    if (millenniumData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            imageIntegration.Status = IntegrationStatus.SendendToShopifyQueue;

                            _millenniumImageIntegrationRepository.Save(imageIntegration);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar integração de imagem");
                        }
                    }
                }
                else
                {
                    var i = 1;
                    foreach (var product in productList.Data.value.OrderBy(x => x.ordem))
                    {
                        if (millenniumData.SplitEnabled)
                            cod_produto = $"{message.IdProduto}_{product.cod_produto}";

                        var imageList = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiBuscaFotoResult>>(
                            new MillenniumApiBuscaFotoRequest
                            {
                                IdFoto = product.idfoto
                            }, cancellationToken
                        );

                        if (imageList.Result == Result.Error)
                        {
                            _logger.Warning($"MillenniumService - Error in ProcessProductImage | {imageList.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), new MillenniumApiBuscaFotoRequest
                            {
                                IdFoto = product.idfoto
                            }, millenniumData));
                            return new ReturnMessage { Result = Result.Error, Error = imageList.Error };
                        }

                        var image = imageList.Data.value.FirstOrDefault();
                        var nomeProduto = Regex.Replace((product.descricao ?? "sem-descricao").Trim(), "\\s", "-").ToLower();

                        var fileName = $"{nomeProduto}-{product.idfoto}_{product.cod_produto}-{i}.jpeg";
                        var blobService = _serviceProvider.GetService<BlobService>();
                        blobService.Init(_logger);

                        imagesUploadList.Add(await blobService.UploadFileBlob(BlobService.BlobFileType.images, fileName, image.foto, millenniumData.TenantCompositeKey, message.IdProduto.ToString()));

                        i++;
                    }

                    if (imagesUploadList.Any(x => x.success))
                    {
                        var updateProductImageMessage = new ShopifyUpdateProductImagesMessage
                        {
                            ExternalProductId = millenniumData.SplitEnabled ? $"{message.IdProduto}_{message.ExternalId}" : message.ExternalId,
                            Images = new ProductImages
                            {
                                ImageUrls = imagesUploadList.Where(w => w.success).Select(x => x.path).ToList(),
                                SkuImages = new List<ProductImages.SkuImage>()
                            },
                            ReferenceIntegrationId = message.ProductIntegrationRefenceId
                        };

                        var serviceBusMessage = new ServiceBusMessage(updateProductImageMessage);
                        await shopifyUpdateProductImageQueueClient.SendAsync(serviceBusMessage.GetMessage(updateProductImageMessage.ExternalProductId));

                        if (millenniumData.EnableSaveIntegrationInformations)
                        {
                            try
                            {
                                imageIntegration.Status = IntegrationStatus.SendendToShopifyQueue;

                                _millenniumImageIntegrationRepository.Save(imageIntegration);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, $"Problemas ao salvar integração de imagem");
                            }
                        }
                    }
                }

                return new ReturnMessage { Result = Result.OK };
            }
            catch (Exception ex)
            {
                if (millenniumData.EnableSaveIntegrationInformations)
                {
                    try
                    {
                        imageIntegration.Exception = ex.Message;

                        _millenniumImageIntegrationRepository.Save(imageIntegration);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error($"Problemas ao salvar integração de imagem | Error.mensagem: {exx.Message}",
                            LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), imageIntegration, millenniumData,
                            $"Problemas ao salvar integração de imagem | Error.mensagem: {exx.Message}"));
                    }
                }

                throw ex;
            }
        }

        async Task<ReturnMessage> SendShopifyOrderStatusUpdateMessageAsync(QueueClient shopifyUpdateOrderStatusQueueClient,
           List<MillenniumApiListOrdersResult.Value> orders, MillenniumData millenniumData, CancellationToken cancellationToken)
        {

            //Todo -> Criar mapper 
            List<ShopifyUpdateOrderStatusMessage> messages = orders.Select(order => new ShopifyUpdateOrderStatusMessage
            {
                OrderExternalId = order.cod_pedidov,
                Cancellation = new ShopifyUpdateOrderStatusMessage.CancellationStatus
                {
                    IsCancelled = order.cancelado == true || new List<int> { MillenniumStatusPedido.Cancelado }.Contains(order.status)
                },
                Payment = new ShopifyUpdateOrderStatusMessage.PaymentStatus
                {
                    IsPaid = new List<int> { MillenniumStatusPedido.PagamentoConfirmado,
                                             MillenniumStatusPedido.EmSeparacao,
                                             MillenniumStatusPedido.Despachado,
                                             MillenniumStatusPedido.Entregue }.Contains(order.status)
                },
                Shipping = new ShopifyUpdateOrderStatusMessage.ShippingStatus
                {
                    IsShipped = new List<int> { MillenniumStatusPedido.Despachado, MillenniumStatusPedido.Entregue }.Contains(order.status),
                    IsDelivered = new List<int> { MillenniumStatusPedido.Entregue }.Contains(order.status)
                }
            }).ToList();

            foreach (var order in orders)
            {
                _millenniumOrderStatusUpdateRepository.Save(new MillenniumOrderStatusUpdate
                {
                    Id = Guid.NewGuid(),
                    TenantId = millenniumData.Id,
                    CodPedidov = order.cod_pedidov,
                    Order = JsonConvert.SerializeObject(order),
                    OrderStatus = order.status,
                    CreationDate = DateTime.Now,
                });
            }

            //chamar para pegar o codigo de tracking
            if (orders.Any(o => new List<int> { MillenniumStatusPedido.Despachado, MillenniumStatusPedido.Entregue }.Contains(o.status)))
            {
                var orderStatus = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListOrdersStatusResult>>(
                    new MillenniumApiListOrdersStatusRequest
                    {
                        OrderIds = orders.Where(o => new List<int> { MillenniumStatusPedido.Despachado, MillenniumStatusPedido.Entregue }.Contains(o.status)).Select(o => o.pedidov).ToList()
                    }, cancellationToken
                );

                if (orderStatus.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = orderStatus.Error };

                foreach (var message in messages.Where(m => m.Shipping.IsShipped))
                {
                    message.Shipping.TrackingObject = orderStatus.Data.value.Where(o => o.cod_pedidov == message.OrderExternalId).Select(o => o.numero_objeto).FirstOrDefault();
                }
            }

            List<Task> tasks = new List<Task>();
            foreach (var message in messages)
            {
                var serviceBusMessage = new ServiceBusMessage(message);
                tasks.Add(shopifyUpdateOrderStatusQueueClient.SendAsync(serviceBusMessage.GetMessage(message.OrderExternalId)));
            }

            await Task.WhenAll(tasks);
            return new ReturnMessage { Result = Result.OK };
        }

        async Task<ReturnMessage> SendSellerCenterOrderStatusUpdateMessageAsync(QueueClient erpUpdateOrderStatusQueueClient,
           List<MillenniumApiListOrdersResult.Value> orders, CancellationToken cancellationToken)
        {
            //Todo -> Criar mapper 
            List<SellerCenterMessages.OrderActor.UpdateOrderStatusMessage> messages = orders.Select(order => new SellerCenterMessages.OrderActor.UpdateOrderStatusMessage
            {
                OrderExternalId = order.cod_pedidov,
                Cancellation = new SellerCenterMessages.OrderActor.UpdateOrderStatusMessage.CancellationStatus
                {
                    IsCancelled = order.cancelado == true || new List<int> { 5 }.Contains(order.status) //Cancelado
                },
                Payment = new SellerCenterMessages.OrderActor.UpdateOrderStatusMessage.PaymentStatus
                {
                    IsPaid = new List<int> { 1, 2, 3, 4 }.Contains(order.status) //Pagamento Confirmado, Em Separação, Despachado, Entregue
                },
                Shipping = new SellerCenterMessages.OrderActor.UpdateOrderStatusMessage.ShippingStatus
                {
                    IsShipped = new List<int> { 3, 4 }.Contains(order.status), //Despachado, Entregue
                    IsDelivered = new List<int> { 4 }.Contains(order.status) //Entregue
                }
            }).ToList();

            //chamar para pegar o codigo de tracking
            if (orders.Any(o => new List<int> { 3, 4 }.Contains(o.status)))
            {
                var orderStatus = await _apiActorGroup.Ask<ReturnMessage<MillenniumApiListOrdersStatusResult>>(
                    new MillenniumApiListOrdersStatusRequest
                    {
                        OrderIds = orders.Where(o => new List<int> { 3, 4 }.Contains(o.status)).Select(o => o.pedidov).ToList()
                    }, cancellationToken
                );

                if (orderStatus.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = orderStatus.Error };

                foreach (var message in messages.Where(m => m.Shipping.IsShipped))
                {
                    message.Shipping.TrackingObject = orderStatus.Data.value.Where(o => o.cod_pedidov == message.OrderExternalId).Select(o => o.numero_objeto).FirstOrDefault();
                }
            }

            List<Task> tasks = new List<Task>();
            foreach (var message in messages)
            {
                var serviceBusMessage = new ServiceBusMessage(message);
                tasks.Add(erpUpdateOrderStatusQueueClient.SendAsync(serviceBusMessage.GetMessage(message.OrderExternalId)));
            }

            await Task.WhenAll(tasks);
            return new ReturnMessage { Result = Result.OK };
        }

        ShopifyUpdateFullProductMessage GetFullProductMessage(MillenniumApiListProductsResult.Value product,
                                                                     List<MillenniumApiListStocksResult.Value> stockList,
                                                                     MillenniumData millenniumData,
                                                                     Guid? integrationId)
        {
            Product.Info productInfo = GetProductInfo(product, millenniumData, stockList);

            foreach (var sku in productInfo.Variants)
            {
                if (sku.Status)
                {
                    var price = product.sku.FirstOrDefault(st => st.sku == sku.OriginalSku);
                    var stock = stockList.FirstOrDefault(st => st.sku == sku.OriginalSku);

                    if (product.kit && millenniumData.EnableProductKit)
                    {
                        var stockMin = stockList.Min(x => x.saldo_vitrine_sem_reserva);
                        stock = stockList.FirstOrDefault(x => x.saldo_vitrine_sem_reserva == stockMin);
                    }

                    if (stock == null)
                    {
                        sku.Status = false;
                    }
                    else if (price == null || price.preco1 == null)
                    {
                        sku.Status = false;
                    }
                    else
                    {

                        sku.Stock = new SkuStock
                        {
                            Locations = string.IsNullOrEmpty(stock.cod_filial)
                                    ? null
                                    : new List<SkuStock.Mulilocation> { new SkuStock.Mulilocation { Quantity = stock.saldo_vitrine_sem_reserva, ErpLocationId = stock.cod_filial } },
                            Quantity = stock.saldo_vitrine_sem_reserva
                        };

                        sku.Price = PriceFill(price, millenniumData, sku);
                    }
                }
            }

            productInfo.Status = productInfo.Status == true && productInfo.Variants.Any(x => x.Status == true);

            return new ShopifyUpdateFullProductMessage
            {
                ProductInfo = productInfo,
                IntegrationId = integrationId
            };
        }

        bool IsSimpleProduct(MillenniumApiListProductsResult.Value product)
        {
            return product.tipo_prod == "AC" && product.kit == false;
        }

        BaseProduct GetPartialProductMessage(MillenniumApiListProductsResult.Value product, MillenniumData millenniumData, Guid? integrationId)
        {
            var tenant = _tenantRepository.GetByIdSync(millenniumData.Id);

            Product.Info productInfo = GetProductInfo(product, millenniumData);

            foreach (var sku in productInfo.Variants)
            {
                if (sku.Status)
                {
                    var price = product.sku.FirstOrDefault(st => st.sku == sku.OriginalSku);
                    if (price == null || price.preco1 == null)
                    {
                        _logger.Warning("MillenniumService - Error in GetPartialProductMessage | Product without price | Produto : {0} sku {1} | {2}",
                            productInfo.ExternalId, sku.OriginalSku, LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), productInfo, millenniumData));
                    }
                    else
                    {
                        sku.Price = PriceFill(price, millenniumData, sku);
                    }
                }
            }

            if (tenant.IntegrationType == Domain.Enums.IntegrationType.SellerCenter)
            {
                return new SellerCenterCreateProductMessage
                {
                    ProductInfo = productInfo
                };
            }
            else
            {
                return new ShopifyUpdatePartialProductMessage
                {
                    ProductInfo = productInfo,
                    IntegrationId = integrationId
                };
            }
        }
        SkuPrice PriceFill(MillenniumApiListProductsResult.Sku price,
                                  MillenniumData millenniumData,
                                  SkuInfo skuInfo)
        {
            if (millenniumData.HasZeroedPriceCase)
            {
                _logger.Warning("HasZeroedPriceCase tenantId: {0}", millenniumData.Id);
                var pivotValue = price.preco1.Value > 0 ? price.preco1.Value : price.preco2 ?? 0;

                var compareAtPrice = price.preco2 ?? pivotValue;
                return skuInfo.Price = new SkuPrice
                {
                    Price = pivotValue,
                    CompareAtPrice = compareAtPrice > pivotValue ? compareAtPrice : (decimal?)null
                };
            }
            else
            {
                var compareAtPrice = price.preco2 ?? price.preco1.Value;
                return skuInfo.Price = new SkuPrice
                {
                    Price = price.preco1.Value,
                    CompareAtPrice = compareAtPrice > price.preco1.Value ? compareAtPrice : (decimal?)null
                };
            }
        }
        Product.Info GetProductInfo(MillenniumApiListProductsResult.Value product,
                                           MillenniumData millenniumData,
                                           List<MillenniumApiListStocksResult.Value> stockList = null)
        {
            if (millenniumData.EnableProductKit && product.kit && product.sku.Any(a => a.componentes_sku_kit.Count <= 0))
                throw new Exception("Kit vazio");

            var activeSkus = product.ActivesSkus;
            var hasCor = millenniumData.SendDefaultCor || activeSkus.Any(s => s.cor > 0);
            var hasTamanho = millenniumData.SendDefaultTamanho || activeSkus.Any(s => !string.IsNullOrWhiteSpace(s.tamanho) && s.tamanho != "U");
            var hasEstampa = millenniumData.SendDefaultEstampa || activeSkus.Any(s => s.estampa > 0);


            var optionsName = new List<string>();
            if (hasCor)
                optionsName.Add(millenniumData.CorDescription.IfIsNullOrWhiteSpace("Cor"));
            if (hasTamanho)
                optionsName.Add(millenniumData.TamanhoDescription.IfIsNullOrWhiteSpace("Tamanho"));
            if (hasEstampa)
                optionsName.Add(millenniumData.EstampaDescription.IfIsNullOrWhiteSpace("Estampa"));

            var metaFields = new List<Metafield>();

            if (millenniumData.ExtraFieldConfigurations.Any())
            {
                JObject values = JObject.Parse(product.OriginalValue);
                var extraFields = new ExpandoObject() as IDictionary<string, Object>;

                foreach (var config in millenniumData.ExtraFieldConfigurations)
                {
                    extraFields.Add(config.Key, values.SelectTokens(config.JSPath));
                }

                metaFields.Add(new Metafield
                {
                    Key = "ExtraFields",
                    Value = Newtonsoft.Json.JsonConvert.SerializeObject(extraFields),
                    ValueType = "JSON_STRING"
                });
            }

            var productInfo = new Product.Info
            {
                ExternalId = GetExternalId(millenniumData, product),
                ExternalCode = product.cod_produto,
                SkuOriginal = product.ActivesSkus.Select(x => x.sku).FirstOrDefault(),
                GroupingReference = product.referencia,
                Title = GetTitle(product, millenniumData),
                Status = product.excluido == false && product.ativo == "T" && activeSkus.Any(),
                BodyHtml = GetBody(product, millenniumData),
                Vendor = product.desc_marca,
                OptionsName = optionsName,
                Categories = GetCategories(product.classificacoes),
                Variants = GetVariants(product, product.sku, hasCor, hasTamanho, hasEstampa, millenniumData, stockList),
                DataSellerCenter = new Product.DataSellerCenter { Variants = GetVariantOptionSellerCenter(optionsName, product, product.sku, hasCor, hasTamanho, hasEstampa, millenniumData) },
                Metafields = metaFields,
                kit = product.kit
            };

            return productInfo;
        }
        string GetExternalId(MillenniumData millenniumData, MillenniumApiListProductsResult.Value product)
        {
            if (millenniumData.SplitEnabled)
            {
                return $"{product.produto}_{product.cod_produto}";
            }
            else
            {
                return product.produto.ToString();
            }
        }
        string GetTitle(MillenniumApiListProductsResult.Value product, MillenniumData millenniumData)
        {
            var productName = millenniumData.NameField switch
            {
                "c" => product.nome_produto_site,
                "descricao_literal" => product.descricao_literal,
                "descricao1" => product.descricao1,
                "descricao_traduzida" => product.descricao_traduzida,
                "descricao_etiq" => product.descricao_etiq,
                "obs" => product.obs,
                "descricao_produto_site" => product.descricao_produto_site,
                "descricao_sf" => product.descricao_sf,
                _ => product.nome_produto_site
            };

            if (string.Equals(productName, null)) return productName;

            //Fazer uma config para isso
            if (millenniumData.Id != 70)
                productName = Regex.Replace(productName, "@|\\[|\\]", "");

            if (millenniumData.CapitalizeProductName)
                productName = string.Join(" ", productName.Split(" ").Where(x => x.Length >= 1).Select(x => char.ToUpper(x.First()) + x.Substring(1).ToLower()).ToList());

            if (millenniumData.ExcludedProductCharacters.Any())
                productName = string.Join(" ", productName.Split(" ").Where(x => x.Length >= 1).Where(x => !millenniumData.ExcludedProductCharacters.Select(y => y.ToUpper()).Contains(x.ToUpper())).ToList());

            if (millenniumData.NameSkuEnabled)
                productName = string.Concat(productName, GetTitleSku(product, millenniumData)).Trim();

            return productName;
        }
        string GetTitleSku(MillenniumApiListProductsResult.Value product, MillenniumData millenniumData)
        {
            const char SPACE = ' ';
            var result = millenniumData.NameSkuField switch
            {
                "desc_cor" => product?.sku?.FirstOrDefault().desc_cor,
                _ => string.Empty
            };
            return string.Concat(SPACE, result.Capitalize());
        }
        string GetBody(MillenniumApiListProductsResult.Value product, MillenniumData millenniumData)
        {
            return millenniumData.DescriptionField switch
            {
                "descricao_literal" => product.descricao_literal,
                "descricao1" => product.descricao1,
                "descricao_traduzida" => product.descricao_traduzida,
                "descricao_etiq" => product.descricao_etiq,
                "obs" => product.obs,
                "descricao_produto_site" => product.descricao_produto_site,
                "em_branco" => "",
                _ => product.descricao_literal
            };
        }
        List<Product.Category> GetCategories(List<MillenniumApiListProductsResult.Classificaco> classificacoes)
        {
            var response = new List<Product.Category>();

            foreach (var category in classificacoes.Where(c => c.excluido == false && !string.IsNullOrWhiteSpace(c.arvore_classificacao)))
            {
                var path = category.arvore_classificacao.Split("\\");
                var currentLevel = response;
                foreach (var item in path)
                {
                    var currentCategory = currentLevel.FirstOrDefault(c => c.Name == item);
                    if (currentCategory == null)
                    {
                        currentCategory = new Product.Category { Name = item, ChildCategories = new List<Product.Category>() };
                        currentLevel.Add(currentCategory);
                    }
                    currentLevel = currentCategory.ChildCategories;
                }
            }

            return response;
        }
        List<Product.SkuInfo> GetVariants(MillenniumApiListProductsResult.Value product,
                                                 List<MillenniumApiListProductsResult.Sku> skus,
                                                 bool hasCor,
                                                 bool hasTamanho,
                                                 bool hasEstampa,
                                                 MillenniumData millenniumData,
                                                 List<MillenniumApiListStocksResult.Value> stockList = null)
        {
            var response = new List<SkuInfo>();
            foreach (var sku in skus)
            {
                var options = new List<string>();
                if (hasCor)
                    options.Add(GetCor(sku, millenniumData));
                if (hasTamanho)
                    options.Add(GetTamanho(sku, millenniumData));
                if (hasEstampa)
                    options.Add(GetEstampa(sku, millenniumData));

                var responseSkus = new SkuInfo
                {
                    Sku = GetSkuField(sku, millenniumData),
                    OriginalSku = sku.sku,
                    Status = sku.ativo == "1",
                    WeightInKG = (sku.peso ?? 0) > 0 ? (sku.peso ?? 0) : (product.peso ?? 0),
                    Barcode = string.IsNullOrWhiteSpace(sku.barra13) == false ? sku.barra13 : sku.barra,
                    Options = options
                };

                if (millenniumData.EnableProductKit
                    && product.kit
                    && sku.componentes_sku_kit != null
                    && sku.componentes_sku_kit.Count > 0
                    && stockList != null)
                {
                    var kitsComponents = sku.componentes_sku_kit.Select(s => new SkuKit
                    {
                        Sku = s.sku,
                        ParentProduct = s.produto_pai,
                        ChildProduct = s.produto_filho,
                        Quantity = stockList.Where(w => w.sku == s.sku).Select(x => x.saldo_vitrine_sem_reserva).FirstOrDefault(),
                        CodProduct = s.cod_produto
                    }).ToList();

                    responseSkus.SkuKits.AddRange(kitsComponents);
                }

                if (millenniumData.SaleWithoutStockEnabled)
                    responseSkus.SellWithoutStock = sku?.permite_pedido_sem_estoque == "T";

                response.Add(responseSkus);
            }
            return response;
        }

        public string CleanInput(string strIn)
        {
            try
            {
                var startIndex = strIn.IndexOf("nErro");
                var endIndex = strIn.Length - strIn.IndexOf("nErro");

                strIn = new string(@strIn.Substring(startIndex, endIndex)
                                    .Replace("nErro", "Erro")
                                    .Replace(",", ".")
                                    .Replace("}", "")
                                    .Normalize(NormalizationForm.FormD)
                                    .Where(ch => char.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                                    .ToArray());

                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (RegexMatchTimeoutException)
            {
                return strIn;
            }
        }
        public List<string> CutString(string error)
        {
            var tagsErro = new List<string>();
            var quantity = error.Length / 30;
            var index = 0;
            var indexOf = 30;

            for (int i = 0; i <= quantity; i++)
            {
                if ((index + indexOf) > error.Length)
                    indexOf = error.Length - index;

                var msgErro = error.Substring(index, indexOf);
                if (i == 0)
                    tagsErro.Add($"Error-{msgErro}");
                else
                    tagsErro.Add(msgErro);

                index += msgErro.Length;
            }

            return tagsErro;
        }
        List<Product.SkuSellerCenter> GetVariantOptionSellerCenter(List<string> optionNames,
                                                                   MillenniumApiListProductsResult.Value product,
                                                                   List<MillenniumApiListProductsResult.Sku> skus,
                                                                   bool hasCor,
                                                                   bool hasTamanho,
                                                                   bool hasEstampa,
                                                                   MillenniumData millenniumData)
        {
            string GetValueInOptionName(int index) => optionNames.Where((value, idx) => idx == index).FirstOrDefault();

            var response = new List<Product.SkuSellerCenter>();
            foreach (var sku in skus)
            {
                var options = new List<string>();
                if (hasCor)
                    options.Add(GetCor(sku, millenniumData).Capitalize());
                if (hasTamanho)
                    options.Add(GetTamanho(sku, millenniumData));
                if (hasEstampa)
                    options.Add(GetEstampa(sku, millenniumData));

                var valuesVariations = options.Select((value, index) => new Product.InfoVariations { NomeVariacao = GetValueInOptionName(index).Capitalize(), ValorVariacao = value }).ToList();
                var responseSkus = new Product.SkuSellerCenter();
                responseSkus.Sku = sku.sku;
                responseSkus.Height = sku.altura;
                responseSkus.Length = sku.comprimento;
                responseSkus.Width = sku.largura;
                responseSkus.Weight = (sku.peso ?? 0) > 0 ? (sku.peso ?? 0) : (product.peso ?? 0);
                responseSkus.Barcode = string.IsNullOrWhiteSpace(sku.barra13) == false ? sku.barra13 : sku.barra;
                responseSkus.Options = new List<Product.DetailsVariations>();
                responseSkus.InfoVariations = valuesVariations;
                response.Add(responseSkus);
            }
            return response;
        }

        string GetCor(MillenniumApiListProductsResult.Sku sku, MillenniumData millenniumData)
        {
            return millenniumData.CorField switch
            {
                "cor" => sku.cor.ToString(),
                "observacao_cor" => sku.observacao_cor,
                "obs_sku1" => sku.obs_sku1,
                "obs_sku2" => sku.obs_sku2,
                "obs_sku3" => sku.obs_sku3,
                "obs_sku4" => sku.obs_sku4,
                "obs_sku5" => sku.obs_sku5,
                _ => sku.desc_cor
            };
        }
        string GetTamanho(MillenniumApiListProductsResult.Sku sku, MillenniumData millenniumData)
        {
            return millenniumData.TamanhoField switch
            {
                "tamanho" => sku.tamanho,
                "obs_sku1" => sku.obs_sku1,
                "obs_sku2" => sku.obs_sku2,
                "obs_sku3" => sku.obs_sku3,
                "obs_sku4" => sku.obs_sku4,
                "obs_sku5" => sku.obs_sku5,
                _ => sku.desc_tamanho
            };
        }
        string GetEstampa(MillenniumApiListProductsResult.Sku sku, MillenniumData millenniumData)
        {
            return millenniumData.EstampaField switch
            {
                "estampa" => sku.estampa.ToString(),
                "obs_sku1" => sku.obs_sku1,
                "obs_sku2" => sku.obs_sku2,
                "obs_sku3" => sku.obs_sku3,
                "obs_sku4" => sku.obs_sku4,
                "obs_sku5" => sku.obs_sku5,
                _ => sku.desc_estampa
            };
        }
        string GetSkuField(IFieldSku sku, MillenniumData millenniumData)
        {
            return millenniumData.SkuFieldType switch
            {
                SkuFieldType.sku => sku.sku,
                SkuFieldType.cod_produto => sku.cod_produto,
                SkuFieldType.id_externo => sku.id_externo,
                _ => sku.sku
            };
        }
        async Task<ReturnMessage<T>> GetInfoPayment<T>(string method, Dictionary<string, string> param)
        {
            var paymentResponse = await _apiActorGroup.Ask<ReturnMessage<T>>(new PaymentExtraInfoRequest
            {
                TypeOf = typeof(T).Name,
                Method = method,
                Params = param
            });

            if (paymentResponse.Result == Result.Error)
                throw new Exception("MillenniumService - Error PaymentExtraInfo", paymentResponse.Error);

            if (paymentResponse == null || paymentResponse.Data == null)
                throw new Exception("MillenniumService - Erro: PaymentExtraInfo é null");

            return paymentResponse;
        }
        async Task<MilenniumApiCreateOrderPaymentDataRequest> GetExtraInformationBrasPag(MillenniumData millenniumData,
                                                                                         ShopifySendOrderToERPMessage message,
                                                                                         MilenniumApiCreateOrderPaymentDataRequest milenniumApiCreateOrderPaymentDataRequest)
        {
            var paymentResponse = await GetInfoPayment<PaymentExtraInfoBrasPagResult>("/Payment/GetPayment", new Dictionary<string, string>()
            {
               { "shop_name", millenniumData.StoreDomainByBrasPag },
               { "checkoutid", message.Checkout_Token }
            });

            milenniumApiCreateOrderPaymentDataRequest.NSU = paymentResponse?.Data?.x_gateway_nsu;
            milenniumApiCreateOrderPaymentDataRequest.Autorizacao = paymentResponse?.Data?.x_gateway_authorization_code;
            milenniumApiCreateOrderPaymentDataRequest.Data_Aprova_Facil = ConvertTimestampToDateTime(paymentResponse?.Data?.x_gateway_approvement_date).ToString();
            milenniumApiCreateOrderPaymentDataRequest.Duplicata = paymentResponse?.Data?.x_gateway_boleto_number;

            if (millenniumData.EnableMaskedNSU && !string.IsNullOrWhiteSpace(milenniumApiCreateOrderPaymentDataRequest.NSU))
                milenniumApiCreateOrderPaymentDataRequest.NSU = milenniumApiCreateOrderPaymentDataRequest.NSU.PadLeft(9, '0');

            return milenniumApiCreateOrderPaymentDataRequest;
        }

        async Task<MilenniumApiCreateOrderPaymentDataRequest> GetExtraInformationMoip(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, MilenniumApiCreateOrderPaymentDataRequest milenniumApiCreateOrderPaymentDataRequest)
        {
            var paymentResponse = await GetInfoPayment<PaymentExtraInfoMoipResult>("/Payment/GetPayment", new Dictionary<string, string>()
            {
               { "shop_name", millenniumData.StoreDomainByBrasPag },
               { "checkoutid", message.Checkout_Token }
            });

            milenniumApiCreateOrderPaymentDataRequest.NSU = paymentResponse?.Data?.gatewayToken;
            milenniumApiCreateOrderPaymentDataRequest.Autorizacao = paymentResponse?.Data?.gatewayOrderToken;

            return milenniumApiCreateOrderPaymentDataRequest;
        }

        async Task<MilenniumApiCreateOrderPaymentDataRequest> GetExtraInformationMercadoPago(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, MilenniumApiCreateOrderPaymentDataRequest milenniumApiCreateOrderPaymentDataRequest)
        {
            var paymentId = message.NoteAttributes.FirstOrDefault(x => x.Name == "mercadoPagoNSU").Value;

            var paymentResponse = await GetInfoPayment<PaymentExtraInfoMercadoPagoResult>($"v1/payments/{paymentId}", new Dictionary<string, string>());

            milenniumApiCreateOrderPaymentDataRequest.NSU = $"{paymentResponse.Data.id}";
            milenniumApiCreateOrderPaymentDataRequest.Autorizacao = $"{paymentResponse.Data.id}";

            if (paymentResponse.Data.payment_type_id == "credit_card" || paymentResponse.Data.payment_type_id == "bank_transfer")
                milenniumApiCreateOrderPaymentDataRequest.Data_Aprova_Facil = paymentResponse.Data.date_approved?.ToString("s", CultureInfo.GetCultureInfo("en-US"));            

            if(paymentResponse?.Data?.payment_method != null)
            {
                message.PaymentData.Issuer = paymentResponse?.Data?.payment_method.id ?? "";
                message.PaymentData.PaymentType = paymentResponse?.Data?.payment_method.type ?? "";
                message.PaymentData.InstallmentQuantity = paymentResponse?.Data?.installments >= 1 ? (int)paymentResponse?.Data?.installments : 1;

                milenniumApiCreateOrderPaymentDataRequest.bandeira = (int)_millenniumDomainService.GetBandeira(message);
                milenniumApiCreateOrderPaymentDataRequest.numparc = _millenniumDomainService.GetNumeroParcelas(message, "", _millenniumDomainService.GetBandeira(message));
                milenniumApiCreateOrderPaymentDataRequest.parcela = _millenniumDomainService.GetNumeroParcelas(message, "", _millenniumDomainService.GetBandeira(message));
                milenniumApiCreateOrderPaymentDataRequest.DESC_TIPO = await GetIssuerType(millenniumData, message);
            }

            return milenniumApiCreateOrderPaymentDataRequest;
        }
    }
}

