using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Akka.Actor;
using Akka.Dispatch;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

using Samurai.Integration.APIClient.Bling.Models.Requests;
using Samurai.Integration.APIClient.Bling.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models;
using Samurai.Integration.Domain.Queues;

namespace Samurai.Integration.Application.Actors.Bling.Shopify
{
    public class ShopifyBlingProductActor : BaseBlingTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActorRef _apiActorGroup;
        private readonly CancellationToken _cancellationToken;
        private readonly QueueClient _erpFullProductQueueClient;

        public ShopifyBlingProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
            : base("ShopifyBlingProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _blingData = blingData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _erpFullProductQueueClient = tenantService.GetQueueClient(_blingData, ShopifyQueue.UpdateFullProductQueue);
            }

            ReceiveAsync((Func<ShopifyListERPFullProductMessage, Task>)(async message =>
            {
                await SendShopifyUpdateFullProductMessage(message.ExternalId, cancellationToken);
            }));

            ReceiveAsync((Func<BlingListAllProductsMessage, Task>)(async message =>
            {
                await SendAllShopifyUpdateFullProductMessage(message.ProductUpdatedDate, cancellationToken);
            }));
        }

        private async Task SendShopifyUpdateFullProductMessage(string productCode, CancellationToken cancellationToken)
        {
            try
            {
                ReturnMessage<BlingApiListProductsResult.Produto> result;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var blingService = scope.ServiceProvider.GetService<BlingService>();
                    blingService.Init(_apiActorGroup, GetLog(), _blingData);
                    result = await blingService.ListProduct(productCode, cancellationToken);

                    var product = result.Data;

                    if (product == null)
                        throw new Exception($"Product {productCode} not found");

                    if (!string.IsNullOrWhiteSpace(product.codigoPai))
                    {
                        var resultPai = await blingService.ListProduct(product.codigoPai, _cancellationToken);

                        product = resultPai.Data;
                    }

                    var productMessage = await GetProductMessage(product, cancellationToken);

                    var serviceBusMessage = new ServiceBusMessage(productMessage);

                    await _erpFullProductQueueClient.SendAsync(serviceBusMessage.GetMessage(productMessage.ProductInfo.ExternalId));
                }

                Sender.Tell(new ReturnMessage());
            }
            catch (Exception ex)
            {
                Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            }
        }

        private async Task SendAllShopifyUpdateFullProductMessage(DateTime? ProductUpdatedDate, CancellationToken cancellationToken)
        {
            try
            {
                ReturnMessage<List<BlingApiListProductsResult.Produto>> result;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var blingService = scope.ServiceProvider.GetService<BlingService>();
                    blingService.Init(_apiActorGroup, GetLog(), _blingData);
                    result = await blingService.ListProducts(ProductUpdatedDate, cancellationToken);

                    if (result.Result == Result.Error)
                        throw result.Error;

                    List<Task> sendMessages = new List<Task>();

                    var products = result.Data;

                    products = products.Where(x => string.IsNullOrWhiteSpace(x.codigoPai)).ToList();

                    foreach (var product in products)
                    {
                        var productMessage = await GetProductMessage(product, cancellationToken);

                        var serviceBusMessage = new ServiceBusMessage(productMessage);

                        sendMessages.Add(_erpFullProductQueueClient.SendAsync(serviceBusMessage.GetMessage(productMessage.ProductInfo.ExternalId)));
                    }

                    await Task.WhenAll(sendMessages);
                }

                Sender.Tell(new ReturnMessage { Result = Result.OK });
            }
            catch (Exception ex)
            {
                Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            }
        }

        private async Task<ShopifyUpdateFullProductMessage> GetProductMessage(BlingApiListProductsResult.Produto product, CancellationToken cancellationToken)
        {
            var optionsName = new List<string>();

            if (product?.variacoes?.Any() == true)
            {
                try
                {
                    var varNames = product.variacoes.Select(x => x.variacao.nome).ToList();

                    if (varNames.All(x => x.Contains(";")) || varNames.All(x => x.Contains(":")))
                    {
                        var types = varNames.Select(x => { return x.Split(";"); });

                        var biggerTypes = types.OrderByDescending(x => x.Length).FirstOrDefault();

                        optionsName = biggerTypes.Select(x =>
                        {
                            return x.Split(":")[0].Trim();
                        }).ToList();
                    }
                    else
                    {
                        optionsName.Add("Opção");
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in GetProductMessage");
                }
            }

            var productInfo = new Product.Info
            {
                ExternalId = product.codigo,
                ExternalCode = product.id,
                GroupingReference = product.codigoPai,
                Title = product.descricao,
                Status = product.situacao == "Ativo",
                BodyHtml = product.descricaoCurta,
                Vendor = product.marca,
                OptionsName = optionsName,
                Images = new ProductImages
                {
                    ImageUrls = product.imagem?.Select(x => x.link)?.ToList(),
                    SkuImages = new List<ProductImages.SkuImage>()
                },
                Categories = new List<Product.Category>
                {
                    new Product.Category
                    {
                        Name = product.categoria.descricao
                    }
                },
                Variants = await GetVariants(product, cancellationToken),
            };

            return new ShopifyUpdateFullProductMessage
            {
                ProductInfo = productInfo
            };
        }

        private async Task<List<Product.SkuInfo>> GetVariants(BlingApiListProductsResult.Produto product, CancellationToken cancellationToken)
        {
            var response = new List<Product.SkuInfo>();

            if (product.variacoes?.Any() == true)
            {
                foreach (var sku in product.variacoes)
                {
                    var values = new List<string>();

                    if (sku.variacao.nome.Contains(";") || sku.variacao.nome.Contains(":"))
                    {
                        try
                        {
                            values = sku?.variacao?.nome?.Split(";")?.Select(x => { return x.Split(":"); })?.Select(x => x[1]?.Trim())?.ToList();
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, "Error in GetVariants");
                        }
                    }
                    else
                    {
                        values.Add(sku.variacao.nome);
                    }

                    var productResult = await _apiActorGroup.Ask<ReturnMessage<BlingApiListProductsResult>>(
                        new BlngApiListProductsRequest
                        {
                            ProductCode = sku.variacao.codigo
                        }, cancellationToken
                    );

                    var skuProduct = productResult?.Data?.retorno?.produtos?.FirstOrDefault();

                    if (skuProduct == null)
                        throw new Exception($"Sku product {sku.variacao.codigo} not found.");

                    var responseSkus = new Product.SkuInfo
                    {
                        Sku = sku.variacao.codigo,
                        Stock = new Product.SkuStock
                        {
                            Quantity = Decimal.ToInt32(skuProduct.produto.estoqueAtual),
                            Sku = sku.variacao.codigo,
                            Locations = GetStockWarehouses(skuProduct.produto)
                        },
                        Price = new Product.SkuPrice
                        {
                            Price = Convert.ToDecimal(skuProduct.produto.preco, new CultureInfo("en-US")),
                            Sku = sku.variacao.codigo
                        },
                        WeightInKG = Convert.ToDecimal(skuProduct.produto.pesoBruto, new CultureInfo("en-US")),
                        Status = true,
                        Options = values
                    };

                    response.Add(responseSkus);
                }
            }
            else
            {
                response.Add(new Product.SkuInfo
                {
                    Sku = product.codigo,
                    Options = new List<string>(),
                    Price = new Product.SkuPrice
                    {
                        Price = Convert.ToDecimal(product.preco, new CultureInfo("en-US")),
                        Sku = product.codigo
                    },
                    WeightInKG = Convert.ToDecimal(product.pesoBruto, new CultureInfo("en-US")),
                    Status = product.situacao == "Ativo",
                    Barcode = product.gtin,
                    Stock = new Product.SkuStock
                    {
                        Quantity = Decimal.ToInt32(product.estoqueAtual),
                        Locations = GetStockWarehouses(product)
                    }
                });
            }
            return response;
        }

        private List<Product.SkuStock.Mulilocation> GetStockWarehouses(BlingApiListProductsResult.Produto product) 
        {

            if (_blingData.EnabledMultiLocation) 
            {
                if (product.depositos.Any()) {

                    return product.depositos.Select(x => new Product.SkuStock.Mulilocation
                    {
                        Quantity = Convert.ToInt32(Convert.ToDecimal(x.deposito.saldo, new CultureInfo("en-us"))),
                        ErpLocationId = x.deposito.id
                    }).ToList();
                }
                
            }

            return default;
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _erpFullProductQueueClient.CloseAsyncSafe();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new ShopifyBlingProductActor(serviceProvider, cancellationToken, blingData, apiActorGroup));
        }
    }
}
