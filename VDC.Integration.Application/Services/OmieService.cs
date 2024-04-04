using Akka.Actor;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.ConsultaEstoque;
using VDC.Integration.APIClient.Omie.Models.Request.ConsultaEstoque.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento;
using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto;
using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using VDC.Integration.APIClient.Omie.Models.Request.ProdutoCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.ProdutoCadastro.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result;
using VDC.Integration.APIClient.Omie.Models.Result.ClienteCadastro;
using VDC.Integration.APIClient.Omie.Models.Result.ConsultaEstoque;
using VDC.Integration.APIClient.Omie.Models.Result.FamiliaCadastro;
using VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaFaturamento;
using VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;
using VDC.Integration.APIClient.Omie.Models.Result.ProdutoCadastro;
using VDC.Integration.Application.Tools;
using VDC.Integration.Domain.Entities.Database.Integrations.Omie;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Messages.Omie;
using VDC.Integration.Domain.Messages.ServiceBus;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Models;
using VDC.Integration.EntityFramework.Repositories;
using VDC.Integration.EntityFramework.Repositories.Omie;

namespace VDC.Integration.Application.Services
{
    public class OmieService
    {
        private ILoggingAdapter _logger;
        private readonly TenantRepository _tenantRepository;
        private IActorRef _apiActorGroup;
        private readonly IServiceProvider _serviceProvider;
        private OmieOrderIntegrationRepository _omieOrderIntegrationRepository;

        public OmieService(TenantRepository tenantRepository, IServiceProvider serviceProvider)
        {
            _tenantRepository = tenantRepository;
            _serviceProvider = serviceProvider;
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter logger)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;
            _omieOrderIntegrationRepository = _serviceProvider.GetService<OmieOrderIntegrationRepository>();

        }

        public async Task<ReturnMessage> ListProduct(long familyId,
                                    OmieData omieData,
                                    QueueClient shopifyFullProductQueueClient,
                                    CancellationToken cancellationToken = default)
        {
            if (familyId <= 0)
            {
                throw new Exception($"familyId must not be empty");
            }

            var family = await _apiActorGroup.Ask<ReturnMessage<ConsultarFamiliaOmieRequestOutput>>(
                new ConsultarFamiliaOmieRequest(
                    new ConsultarFamiliaOmieRequestInput
                    {
                        codigo = familyId
                    }
                ), cancellationToken
            );

            if (family.Result == Result.Error)
            {
                _logger.Warning("OmieService - Error in ListProduct | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ConsultarFamiliaOmieRequestInput
                {
                    codigo = familyId
                }, omieData), family.Error.Message);
                return new ReturnMessage { Result = Result.Error, Error = family.Error };
            }

            if (family.Data != null)
            {
                var productSkuList = new List<ProdutoCadastroResult>();
                var stockList = new Dictionary<long, PosicaoEstoqueOmieRequestOutput>();
                if (family.Data.inativo != "S")
                {
                    {
                        var page = 0;
                        ReturnMessage<ListarProdutosOmieRequestOutput> result = null;
                        do
                        {
                            page++;
                            await Task.Delay(1000);
                            result = await _apiActorGroup.Ask<ReturnMessage<ListarProdutosOmieRequestOutput>>(
                                new ListarProdutosOmieRequest(
                                    new ListarProdutosOmieRequestInput
                                    {
                                        pagina = page,
                                        registros_por_pagina = 50,
                                        filtrar_apenas_marketplace = "S",
                                        exibir_caracteristicas = "S",
                                        exibir_obs = "S",
                                        apenas_importado_api = "N",
                                        filtrar_apenas_omiepdv = "N",
                                        filtrar_apenas_familia = family.Data.codigo.ToString()
                                    }
                                ), cancellationToken
                            );


                            if (result.Result == Result.Error)
                            {
                                if (result.Error.Message.Contains("existem registros para"))
                                    return new ReturnMessage { Result = Result.OK };

                                _logger.Warning("OmieService - Error in ListProduct | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ListarProdutosOmieRequestInput
                                {
                                    pagina = page,
                                    registros_por_pagina = 50,
                                    filtrar_apenas_marketplace = "S",
                                    exibir_caracteristicas = "S",
                                    filtrar_apenas_familia = family.Data.codigo.ToString()
                                }, omieData), result.Error.Message);
                                return new ReturnMessage { Result = Result.Error, Error = result.Error };
                            }


                            productSkuList.AddRange(result.Data.produto_servico_cadastro);
                        } while (result.Data.total_de_paginas > page);
                    }

                    foreach (var sku in productSkuList)
                    {
                        if (sku.inativo != "S")
                        {
                            await Task.Delay(1000);
                            var result = await _apiActorGroup.Ask<ReturnMessage<PosicaoEstoqueOmieRequestOutput>>(
                                    new PosicaoEstoqueOmieRequest(
                                        new PosicaoEstoqueOmieRequestInput
                                        {
                                            id_prod = sku.codigo_produto,
                                            codigo_local_estoque = omieData.CodigoLocalEstoque
                                        }
                                    ), cancellationToken
                                );

                            if (result.Result == Result.Error)
                            {
                                _logger.Warning("OmieService - Error in ListProduct | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new PosicaoEstoqueOmieRequestInput
                                {
                                    id_prod = sku.codigo_produto,
                                    codigo_local_estoque = omieData.CodigoLocalEstoque
                                }, omieData), result.Error.Message);

                                return new ReturnMessage { Result = Result.Error, Error = result.Error };
                            }


                            stockList.Add(sku.codigo_produto, result.Data);
                        }
                    }
                }

                if (productSkuList.Count <= 0)
                    throw new Exception($"not found products for family {familyId}");


                var fullProductMessage = GetFullProductMessage(omieData, family.Data, productSkuList, stockList);
                var serviceBusMessage = new ServiceBusMessage(fullProductMessage);
                await shopifyFullProductQueueClient.SendAsync(serviceBusMessage.GetMessage(fullProductMessage.ProductInfo.ExternalId));

                return new ReturnMessage { Result = Result.OK };
            }
            else
            {
                throw new Exception($"family {familyId} not found");
            }
        }

        private ShopifyUpdateFullProductMessage GetFullProductMessage(OmieData omieData, ConsultarFamiliaOmieRequestOutput family, List<ProdutoCadastroResult> productSkuList, Dictionary<long, PosicaoEstoqueOmieRequestOutput> stockList)
        {
            var activeSkus = productSkuList.Where(s => s.inativo != "S").ToList();
            var firstSku = activeSkus.Where(x => x.caracteristicas != null).FirstOrDefault() ?? activeSkus.FirstOrDefault();


            var shopifyUpdateFullProductMessage = new ShopifyUpdateFullProductMessage
            {
                ProductInfo = new Product.Info
                {
                    ExternalId = family.codigo.ToString(),
                    ExternalCode = family.codFamilia,
                    Title = GetTitle(productSkuList.FirstOrDefault(), omieData) ?? family.nomeFamilia,
                    Status = family.inativo != "S" && activeSkus.Any(),
                    BodyHtml = (omieData.EscapeBodyPipe ? firstSku?.descr_detalhada.Replace('|', '\n') : firstSku?.descr_detalhada).HtmlDecode(),
                    Vendor = firstSku?.marca.HtmlDecode(),
                    Images = new ProductImages
                    {
                        ImageUrls = activeSkus.Where(s => s.imagens?.Any() == true).SelectMany(s => s.imagens).Select(i => i.url_imagem).ToList(),
                        SkuImages = activeSkus.Select(s => new ProductImages.SkuImage
                        {
                            Sku = GetShopifySkuCode(omieData, s.codigo_produto, s.codigo),
                            SkuImageUrl = s.imagens?.Select(i => i.url_imagem).FirstOrDefault()
                        }).ToList()
                    },
                    OptionsName = firstSku?.caracteristicas?.Where(c => omieData.VariantCaracteristicas?.Any(v => string.Equals(c.cNomeCaract.HtmlDecode(), v, StringComparison.OrdinalIgnoreCase)) == true).OrderBy(c => omieData.VariantCaracteristicas.FindIndex(v => string.Equals(c.cNomeCaract.HtmlDecode(), v, StringComparison.OrdinalIgnoreCase))).Select(c => c.cNomeCaract.HtmlDecode()).ToList() ?? new List<string>(),
                    Categories = GetCategories(omieData, firstSku),
                    Variants = GetVariants(omieData, productSkuList, stockList)
                }
            };

            return shopifyUpdateFullProductMessage;
        }

        private List<Product.Category> GetCategories(OmieData omieData, ProdutoCadastroResult sku)
        {
            var response = new List<Product.Category>();

            if (sku != null && omieData.CategoryCaracteristicas?.Any() == true)
            {
                foreach (var category in omieData.CategoryCaracteristicas)
                {
                    var value = sku.caracteristicas?.Where(c => c.cNomeCaract.HtmlDecode() == category).Select(c => c.cConteudo.HtmlDecode()).FirstOrDefault();
                    if (value != null)
                    {
                        foreach (var splitCategory in value.Split(';'))
                        {
                            var path = splitCategory.Split(">>");
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
                    }
                }
            }

            return response;
        }

        private List<Product.SkuInfo> GetVariants(OmieData omieData, List<ProdutoCadastroResult> productSkuList, Dictionary<long, PosicaoEstoqueOmieRequestOutput> stockList)
        {
            var response = new List<Product.SkuInfo>();
            foreach (var sku in productSkuList)
            {
                var responseSkus = new Product.SkuInfo();
                responseSkus.Sku = GetShopifySkuCode(omieData, sku.codigo_produto, sku.codigo);
                responseSkus.Status = sku.inativo != "S";
                responseSkus.WeightInKG = sku.peso_bruto;
                responseSkus.Barcode = sku.ean;
                responseSkus.Options = sku.caracteristicas?.Where(c => omieData.VariantCaracteristicas?.Any(v => string.Equals(c.cNomeCaract.HtmlDecode(), v, StringComparison.OrdinalIgnoreCase)) == true).OrderBy(c => omieData.VariantCaracteristicas.FindIndex(v => string.Equals(c.cNomeCaract.HtmlDecode(), v, StringComparison.OrdinalIgnoreCase))).Select(c => c.cConteudo.HtmlDecode()).ToList() ?? new List<string>();
                responseSkus.Price = new Product.SkuPrice { Price = sku.valor_unitario, CompareAtPrice = null };
                if (responseSkus.Status)
                {
                    var stock = stockList[sku.codigo_produto];
                    responseSkus.Stock = new Product.SkuStock { Quantity = (int)Math.Floor(Math.Max(stock.saldo - stock.pendente - stock.estoque_minimo, 0)) };
                }
                response.Add(responseSkus);
            }
            return response;
        }

        public async Task<ReturnMessage> ListPartialProduct(OmieListPartialProductMessage message, OmieData omieData, QueueClient shopifyPartialProductQueueClient, QueueClient shopifyPartialSkuQueueClient, CancellationToken cancellationToken)
        {
            ProdutoCadastroResult sku = null;
            {
                var result = await _apiActorGroup.Ask<ReturnMessage<ListarProdutosOmieRequestOutput>>(
                                new ListarProdutosOmieRequest(
                                    new ListarProdutosOmieRequestInput
                                    {
                                        registros_por_pagina = 1,
                                        filtrar_apenas_marketplace = "S",
                                        exibir_caracteristicas = "S",
                                        exibir_obs = "S",
                                        produtosPorCodigo = new List<ListarProdutosOmieRequestInput.ProdutoCodigo>
                                        {
                                            new ListarProdutosOmieRequestInput.ProdutoCodigo{ codigo_produto = message.ProductSkuId }
                                        }
                                    }
                                ), cancellationToken
                            );

                if (result.Result == Result.Error)
                {
                    _logger.Warning("OmieService - Error in ListPartialProduct | {0} | Error: {1}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ListarProdutosOmieRequestInput
                    {
                        registros_por_pagina = 1,
                        filtrar_apenas_marketplace = "S",
                        exibir_caracteristicas = "S",
                        produtosPorCodigo = new List<ListarProdutosOmieRequestInput.ProdutoCodigo>
                                        {
                                            new ListarProdutosOmieRequestInput.ProdutoCodigo{ codigo_produto = message.ProductSkuId }
                                        }
                    }, omieData), result.Error);
                    return new ReturnMessage { Result = Result.Error, Error = result.Error };
                }
                sku = result.Data.produto_servico_cadastro.FirstOrDefault();
            }

            if (sku == null || sku.codigo_familia == 0 || sku.inativo == "S")
            {
                var partialSkuMessage = new ServiceBusMessage(new ShopifyUpdatePartialSkuMessage
                {
                    SkuInfo = new Product.SkuInfo
                    {
                        Sku = GetShopifySkuCode(omieData, message.ProductSkuId, message.ProductSkuCode),
                        Status = false
                    }
                });
                await shopifyPartialSkuQueueClient.SendAsync(partialSkuMessage.GetMessage(message.ProductSkuId));
            }
            else
            {
                var partialProductMessage = new ServiceBusMessage(new ShopifyUpdatePartialProductMessage
                {
                    ProductInfo = new Product.Info
                    {
                        ExternalId = sku.codigo_familia.ToString(),
                        Status = true,
                        Title = GetTitle(sku, omieData) ?? sku.descricao_familia,
                        BodyHtml = (omieData.EscapeBodyPipe ? sku.descr_detalhada.Replace('|', '\n') : sku.descr_detalhada).HtmlDecode(),
                        Vendor = sku.marca.HtmlDecode(),
                        Categories = GetCategories(omieData, sku)
                    }
                });

                if (omieData.Id == 71)
                {
                    _logger.Warning("GetCategories:3 " + JsonConvert.SerializeObject(new ShopifyUpdatePartialProductMessage
                    {
                        ProductInfo = new Product.Info
                        {
                            ExternalId = sku.codigo_familia.ToString(),
                            Status = true,
                            Title = GetTitle(sku, omieData) ?? sku.descricao_familia,
                            BodyHtml = (omieData.EscapeBodyPipe ? sku.descr_detalhada.Replace('|', '\n') : sku.descr_detalhada).HtmlDecode(),
                            Vendor = sku.marca.HtmlDecode(),
                            Categories = GetCategories(omieData, sku)
                        }
                    }));
                }

                await shopifyPartialProductQueueClient.SendAsync(partialProductMessage.GetMessage(sku.codigo_familia));


                var options = sku.caracteristicas?.Where(c => omieData.VariantCaracteristicas?.Any(v => string.Equals(c.cNomeCaract.HtmlDecode(), v, StringComparison.OrdinalIgnoreCase)) == true).OrderBy(c => omieData.VariantCaracteristicas.FindIndex(v => string.Equals(c.cNomeCaract.HtmlDecode(), v, StringComparison.OrdinalIgnoreCase))).ToList() ?? new List<ProdutoCadastroResult.Caracteristica>();

                var partialSkuMessage = new ServiceBusMessage(new ShopifyUpdatePartialSkuMessage
                {
                    ExternalProductId = sku.codigo_familia.ToString(),
                    SkuInfo = new Product.SkuInfo
                    {
                        Sku = GetShopifySkuCode(omieData, message.ProductSkuId, message.ProductSkuCode),
                        Status = true,
                        WeightInKG = sku.peso_bruto,
                        Barcode = sku.ean,
                        OptionsName = options.Select(c => c.cNomeCaract.HtmlDecode()).ToList(),
                        Options = options.Select(c => c.cConteudo.HtmlDecode()).ToList(),
                        Price = new Product.SkuPrice
                        {
                            Price = sku.valor_unitario,
                            CompareAtPrice = null
                        }
                    }
                });
                await shopifyPartialSkuQueueClient.SendAsync(partialSkuMessage.GetMessage(sku.codigo_produto));
            }

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> ListStockSku(OmieListStockSkuMessage message, OmieData omieData, QueueClient shopifyStockQueueClient, CancellationToken cancellationToken)
        {
            var result = await _apiActorGroup.Ask<ReturnMessage<PosicaoEstoqueOmieRequestOutput>>(
                                    new PosicaoEstoqueOmieRequest(
                                        new PosicaoEstoqueOmieRequestInput
                                        {
                                            id_prod = message.ProductSkuId,
                                            codigo_local_estoque = omieData.CodigoLocalEstoque
                                        }
                                    ), cancellationToken
                                );

            if (result.Result == Result.Error)
            {
                _logger.Warning("OmieService - Error in  ListStockSku |Erro message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new PosicaoEstoqueOmieRequestInput
                {
                    id_prod = message.ProductSkuId,
                    codigo_local_estoque = omieData.CodigoLocalEstoque
                }, omieData), result.Error.Message);
                return new ReturnMessage { Result = Result.Error, Error = result.Error };
            }

            var stock = result.Data;
            var stockMessage = new ShopifyUpdateStockMessage
            {
                Value = new Product.SkuStock
                {
                    Sku = GetShopifySkuCode(omieData, message.ProductSkuId, message.ProductSkuCode),
                    Quantity = (int)Math.Floor(Math.Max(stock.saldo - stock.pendente - stock.estoque_minimo, 0))
                }
            };
            var serviceBusMessage = new ServiceBusMessage(stockMessage);
            await shopifyStockQueueClient.SendAsync(serviceBusMessage.GetMessage(message.ProductSkuId));

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> UpdateOrder(OmieData omieData, ShopifySendOrderToERPMessage message, CancellationToken cancellationToken)
        {
            var process = new OmieUpdateOrderProcess
            {
                Id = new Guid(),
                TenantId = omieData.Id,
                ProcessDate = DateTime.UtcNow,
                OrderId = message.ID,
                ShopifyListOrderProcessReferenceId = message.ShopifyListOrderProcessId,
            };

            try
            {
                _omieOrderIntegrationRepository.Save(process);

                var queryOrder = await _apiActorGroup.Ask<ReturnMessage<ConsultarPedidoOmieRequestOutput>>(
                                        new ConsultarPedidoOmieRequest(
                                            new ConsultarPedidoOmieRequestInput
                                            {
                                                codigo_pedido_integracao = message.ExternalID
                                            }
                                        ), cancellationToken
                                    );

                var currentData = queryOrder;

                if (currentData.Result == Result.Error)
                {
                    if (currentData.Error is OmieException)
                    {
                        var omieError = ((OmieException)currentData.Error).Error;
                        if (omieError.IsError())
                        {
                            _logger.Warning("OmieService - Error in UpdateOrder | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData));

                            return new ReturnMessage { Result = Result.Error, Error = new Exception(currentData.Error.Message) };
                        }
                    }
                    else
                    {
                        _logger.Warning("OmieService - Error in UpdateOrder | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData));
                        return new ReturnMessage { Result = Result.Error, Error = new Exception(currentData.Error.Message) };
                    }
                }

                if (currentData.Data?.pedido_venda_produto == null)
                {
                    if (string.IsNullOrWhiteSpace(message.Customer.Company))
                        throw new Exception($"cpf/cnpj obrigatório");

                    var items = new List<IncluirPedidoOmieRequestInput.Item>();

                    message.Items.ForEach(i =>
                    {
                        (var id, var code) = SplitShopifySkuCode(i.Sku);
                        items.Add(new IncluirPedidoOmieRequestInput.Item
                        {
                            ide = new IncluirPedidoOmieRequestInput.IDE
                            {
                                codigo_item_integracao = i.Id.ToString(),
                            },
                            inf_adic = new IncluirPedidoOmieRequestInput.InformacoesAdicionaisItem
                            {
                                codigo_local_estoque = omieData.CodigoLocalEstoque,
                                nao_gerar_financeiro = omieData.NaoGerarFinanceiro ? "S" : null
                            },
                            produto = new IncluirPedidoOmieRequestInput.Produto
                            {
                                codigo_produto = id,
                                codigo = string.IsNullOrEmpty(code) ? i.Sku : code,
                                descricao = i.Name,
                                quantidade = i.Quantity,
                                valor_unitario = i.Price,
                                valor_desconto = i.DiscountValue > 0 ? i.DiscountValue : null,
                                tipo_desconto = i.DiscountValue > 0 ? "V" : null
                            }
                        });
                    });

                    var order = await CreateOrderToOmie(omieData, message, items, cancellationToken);

                    if (order.Result == Result.Error)
                    {
                        process.OmieResponse = order?.Error?.Message;
                        _omieOrderIntegrationRepository.Save(process);

                        _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), order?.Error?.Message);
                        return new ReturnMessage { Result = Result.Error, Error = new Exception(order?.Error?.Message) };
                    }

                    var OrderResult = await _apiActorGroup.Ask<ReturnMessage<IncluirPedidoOmieRequestOutput>>(
                                   new IncluirPedidoOmieRequest(
                                       order.Data
                                   ), cancellationToken
                               );

                    process.Payload = JsonConvert.SerializeObject(order);
                    _omieOrderIntegrationRepository.Save(process);

                    if (OrderResult?.Result == Result.Error)
                    {
                        process.OmieResponse = OrderResult?.Error?.Message;
                        _omieOrderIntegrationRepository.Save(process);

                        _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), OrderResult?.Error?.Message);
                        return new ReturnMessage { Result = Result.Error, Error = new Exception(OrderResult?.Error?.Message) };
                    }

                }
                else
                {
                    var currentOrder = currentData.Data.pedido_venda_produto;

                    if (!message.Cancelled && currentOrder.cabecalho.etapa == "10")
                    {
                        var items = new List<IncluirPedidoOmieRequestInput.Item>();

                        var refundsLineItems = message?.Refunds?.Where(x => x.refund_line_items != null)?.SelectMany(s => s.refund_line_items)?.ToList();
                        var allItems = message.Items;

                        refundsLineItems = refundsLineItems?.Where(x => allItems.Any(a =>
                        {
                            return a.Id == x.line_item.id && (a.Quantity == refundsLineItems.Count(c => c.line_item.id == a.Id));
                        }))?.ToList();

                        if (refundsLineItems != null && refundsLineItems.Count > 0)
                        {
                            allItems?.RemoveAll(x => refundsLineItems.Any(c => c.line_item_id == x.Id));

                            refundsLineItems.ForEach(refundsLineItems =>
                            {
                                (var id, var code) = SplitShopifySkuCode(refundsLineItems.line_item.sku);
                                items.Add(new IncluirPedidoOmieRequestInput.Item
                                {
                                    ide = new IncluirPedidoOmieRequestInput.IDE
                                    {
                                        codigo_item_integracao = refundsLineItems.line_item.id.ToString(),
                                        acao_item = "E"
                                    },
                                    inf_adic = new IncluirPedidoOmieRequestInput.InformacoesAdicionaisItem
                                    {
                                        codigo_local_estoque = omieData.CodigoLocalEstoque,
                                        nao_gerar_financeiro = omieData.NaoGerarFinanceiro ? "S" : null
                                    },
                                    produto = new IncluirPedidoOmieRequestInput.Produto
                                    {
                                        codigo_produto = id,
                                        codigo = string.IsNullOrEmpty(code) ? refundsLineItems.line_item.sku : code,
                                        descricao = refundsLineItems.line_item.name,
                                        quantidade = refundsLineItems.line_item.quantity,
                                        valor_unitario = refundsLineItems.line_item.price,
                                    }
                                });
                            });
                        }

                        var newItems = allItems?.Where(x => !currentOrder.det.Any(a =>
                        {
                            (var id, var code) = SplitShopifySkuCode(x.Sku);
                            return a.produto.codigo_produto == id && a.produto.quantidade == x.Quantity;
                        }))?.ToList();

                        var refundsLineItemsAux = message?.Refunds?.Where(x => x.refund_line_items != null)?.SelectMany(s => s.refund_line_items)?.ToList();
                        if (refundsLineItemsAux.Any(r => newItems.Any(n => n.Sku == r.line_item.sku)))
                        {
                            newItems.ForEach(item =>
                            {
                                var qauntRefunds = refundsLineItemsAux.Count(s => s.line_item.sku == item.Sku);
                                if (qauntRefunds > 0)
                                {
                                    var originalQuantity = item.Quantity;
                                    item.Quantity -= qauntRefunds;
                                    item.DiscountValue = item.DiscountValue / originalQuantity * item.Quantity;
                                }
                            });
                        }

                        if (newItems != null && newItems.Count > 0)
                        {
                            newItems.ForEach(i =>
                            {
                                (var id, var code) = SplitShopifySkuCode(i.Sku);
                                items.Add(new IncluirPedidoOmieRequestInput.Item
                                {
                                    ide = new IncluirPedidoOmieRequestInput.IDE
                                    {
                                        codigo_item_integracao = i.Id.ToString(),
                                    },
                                    inf_adic = new IncluirPedidoOmieRequestInput.InformacoesAdicionaisItem
                                    {
                                        codigo_local_estoque = omieData.CodigoLocalEstoque,
                                        nao_gerar_financeiro = omieData.NaoGerarFinanceiro ? "S" : null
                                    },
                                    produto = new IncluirPedidoOmieRequestInput.Produto
                                    {
                                        codigo_produto = id,
                                        codigo = string.IsNullOrEmpty(code) ? i.Sku : code,
                                        descricao = i.Name,
                                        quantidade = i.Quantity,
                                        valor_unitario = i.Price,
                                        valor_desconto = i.DiscountValue > 0 ? i.DiscountValue : null,
                                        tipo_desconto = i.DiscountValue > 0 ? "V" : null
                                    }
                                });
                            });
                        }

                        if (items.Count > 0)
                        {
                            var order = await CreateOrderToOmie(omieData, message, items, cancellationToken);

                            if (order.Result == Result.Error)
                            {
                                process.OmieResponse = order?.Error?.Message;
                                _omieOrderIntegrationRepository.Save(process);

                                _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), order?.Error?.Message);
                                return new ReturnMessage { Result = Result.Error, Error = new Exception(order?.Error?.Message) };
                            }

                            var OrderResult = await _apiActorGroup.Ask<ReturnMessage<IncluirPedidoOmieRequestOutput>>(
                                       new AlterarPedidoOmieRequest(
                                           order.Data
                                       ), cancellationToken
                                   );

                            process.Payload = JsonConvert.SerializeObject(order);
                            _omieOrderIntegrationRepository.Save(process);

                            if (OrderResult?.Result == Result.Error)
                            {
                                process.OmieResponse = OrderResult?.Error?.Message;
                                _omieOrderIntegrationRepository.Save(process);

                                _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), OrderResult?.Error?.Message);
                                return new ReturnMessage { Result = Result.Error, Error = new Exception(OrderResult?.Error?.Message) };
                            }
                        }
                    }

                    if (message.Cancelled)
                    {
                        if (currentOrder.infoCadastro?.cancelado != "S")
                        {
                            var cancelOrderResult = await _apiActorGroup.Ask<ReturnMessage<CancelarPedidoVendaOmieRequestOutput>>(
                                                new CancelarPedidoVendaOmieRequest(
                                                    new CancelarPedidoVendaOmieRequestInput
                                                    {
                                                        nCodPed = currentOrder.cabecalho.codigo_pedido
                                                    }
                                                ), cancellationToken
                                            );

                            if (cancelOrderResult.Result == Result.Error)
                            {
                                _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), cancelOrderResult.Error.Message);
                                return new ReturnMessage { Result = Result.Error, Error = cancelOrderResult.Error };
                            }
                        }
                    }

                    else if (message.Approved)
                    {
                        if (!string.IsNullOrWhiteSpace(omieData.CodigoEtapaPaymentConfirmed)
                                && !string.IsNullOrWhiteSpace(currentOrder.cabecalho.etapa)
                                && int.Parse(currentOrder.cabecalho.etapa) < int.Parse(omieData.CodigoEtapaPaymentConfirmed))
                        {

                            var setAsPaidOrderResult = await _apiActorGroup.Ask<ReturnMessage<TrocarEtapaPedidoOmieRequestOutput>>(
                                                new TrocarEtapaPedidoOmieRequest(
                                                    new TrocarEtapaPedidoOmieRequestInput
                                                    {
                                                        codigo_pedido = currentOrder.cabecalho.codigo_pedido,
                                                        etapa = omieData.CodigoEtapaPaymentConfirmed
                                                    }
                                                ), cancellationToken
                                            );

                            _logger.Info("OmieService:660 - setAsPaidOrderResult: | {0}", JsonConvert.SerializeObject(setAsPaidOrderResult, new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Include,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                Error = (serializer, error) => error.ErrorContext.Handled = true
                            }));

                            if (setAsPaidOrderResult.Result == Result.Error)
                            {
                                _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), setAsPaidOrderResult?.Error?.Message);
                                return new ReturnMessage { Result = Result.Error, Error = setAsPaidOrderResult?.Error };
                            }

                        }
                    }
                }

                return new ReturnMessage { Result = Result.OK };
            }
            catch (Exception ex)
            {
                _logger.Warning("error on update order: {0} | ex: {1}", message.ExternalID, ex.ToString());
                process.Exception = ex.ToString();
                _omieOrderIntegrationRepository.Save(process);
                throw;
            }
        }

        private async Task<ReturnMessage<IncluirPedidoOmieRequestInput>> CreateOrderToOmie(OmieData omieData, ShopifySendOrderToERPMessage message, List<IncluirPedidoOmieRequestInput.Item> items, CancellationToken cancellationToken)
        {
            var updateClientResult = await UpsertClienteOmie(omieData, message, cancellationToken);

            if (updateClientResult.Result == Result.Error)
            {
                _logger.Warning("OmieService - Error in CreateOrderToOmie | Erro message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), updateClientResult.Error.Message);
                return new ReturnMessage<IncluirPedidoOmieRequestInput> { Result = Result.Error, Error = new Exception(updateClientResult.Error.Message) };
            }

            var freteMapping = omieData.FreteMapping.Where(m => string.Equals(m.ShopifyCarrierTitle, message.CarrierName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            var meioPagamento = string.Empty;

            if (message.PaymentData.PaymentType == null)
                throw new OmieException(new OmieError() { faultstring = "payment_type is null" }, "payment_type is null");

            if (message.PaymentData.PaymentType.ToUpper().Equals("ticket") || message.PaymentData.PaymentType.ToLower().Equals("ticket"))
                meioPagamento = "15"; //Boleto Bancário
            else if (message.PaymentData.PaymentType.ToUpper().Equals("CREDIT_CARD") || message.PaymentData.PaymentType.ToLower().Equals("credit_card"))
                meioPagamento = "03"; //Cartão de Crédito
            else if (message.PaymentData.PaymentType.ToUpper().Equals("bank_transfer") || message.PaymentData.PaymentType.ToLower().Equals("bank_transfer"))
                meioPagamento = "17"; //Pix
            else
                meioPagamento = "99"; //Outros

            var qtdParcelas = 0;
            if (omieData.ParcelaUnica)
                qtdParcelas = omieData.CodigoParcelaMapping.FirstOrDefault()?.QuantidadeParcela ?? 1;
            else
                qtdParcelas = message.PaymentData.PaymentType.ToUpper().Equals("PAYPAL") ? 1 : Math.Max(message.PaymentData.InstallmentQuantity, 1);

            var valorParcela = message.PaymentData.InstallmentValue > 0 ? message.PaymentData.InstallmentValue : message.Total / qtdParcelas;

            return new ReturnMessage<IncluirPedidoOmieRequestInput>
            {
                Result = Result.OK,
                Data = new IncluirPedidoOmieRequestInput
                {
                    cabecalho = new IncluirPedidoOmieRequestInput.Cabecalho
                    {
                        codigo_pedido_integracao = message.ExternalID,
                        origem_pedido = "API",
                        quantidade_itens = message.Items.Count(),
                        codigo_cliente = updateClientResult.Data.codigo_cliente_omie,
                        etapa = message.Approved && !string.IsNullOrWhiteSpace(omieData.CodigoEtapaPaymentConfirmed) ? omieData.CodigoEtapaPaymentConfirmed : "10",
                        data_previsao = DateTime.Today.ToString("dd/MM/yyyy"),
                        qtde_parcelas = qtdParcelas,
                        codigo_parcela = GetCodigoParcela(qtdParcelas, omieData),
                        codigo_cenario_impostos = omieData.CodigoCenarioImposto
                    },
                    frete = freteMapping == null ? null : new IncluirPedidoOmieRequestInput.Frete
                    {
                        codigo_transportadora = freteMapping.OmieCodigoTransportadora,
                        modalidade = message.ShippingValue == 0 ? "0" : "1",
                        valor_frete = message.ShippingValue,
                        previsao_entrega = (message.CreatedAt.AddDays(message.DaysToDelivery) >= DateTime.Today ? message.CreatedAt.AddDays(message.DaysToDelivery) : DateTime.Today).ToString("dd/MM/yyyy"),
                        quantidade_volumes = omieData.ParcelaUnica ? 1 : default
                    },
                    informacoes_adicionais = new IncluirPedidoOmieRequestInput.InformacoesAdicionais
                    {
                        codigo_categoria = omieData.CodigoCategoria,
                        codigo_conta_corrente = omieData.CodigoContaCorrente ?? 0,
                        numero_pedido_cliente = message.Name,
                        consumidor_final = "S",
                        enviar_email = omieData.SendNotaFiscalEmailToClient || omieData.ExtraNotaFiscalEmails.Any() ? "S" : "N",
                        utilizar_emails = string.Join(',', omieData.ExtraNotaFiscalEmails.Append(message.Customer.Email)),
                        outros_detalhes = new IncluirPedidoOmieRequestInput.OutrosDetalhes
                        {
                            cCnpjCpfOd = message.Customer.Company.CleanDocument(),
                            cEnderecoOd = message.Customer.DeliveryAddress.Address?.Truncate(60),
                            cNumeroOd = message.Customer.DeliveryAddress.Number,
                            cBairroOd = message.Customer.DeliveryAddress.District,
                            cComplementoOd = message.Customer.DeliveryAddress.Complement?.Truncate(60),
                            cCEPOd = message.Customer.DeliveryAddress.ZipCode,
                            cEstadoOd = message.Customer.DeliveryAddress.State,
                            cCidadeOd = message.Customer.DeliveryAddress.City
                        }
                    },
                    det = items,
                    lista_parcelas = new IncluirPedidoOmieRequestInput.ListaParcelas
                    {
                        parcela = Enumerable.Range(1, qtdParcelas).Select(p => new IncluirPedidoOmieRequestInput.Parcela
                        {
                            numero_parcela = p,
                            data_vencimento = omieData.GetQtdDiasVencimentoParcela(p) != null
                                ? DateTime.Now.AddDays(omieData.GetQtdDiasVencimentoParcela(p).Value).ToString("dd/MM/yyyy")
                                : DateTime.Now.AddMonths(p).ToString("dd/MM/yyyy"),
                            valor = valorParcela,
                            percentual = p != qtdParcelas ? Math.Round((decimal)100 / qtdParcelas, 2) : 100 - (Math.Round((decimal)100 / qtdParcelas, 2) * (qtdParcelas - 1)),
                            meio_pagamento = meioPagamento,
                            nao_gerar_boleto = "S"
                        }).ToList()
                    }
                }
            };
        }

        private async Task<ReturnMessage<UpsertClienteOmieRequestOutput>> UpsertClienteOmie(OmieData omieData, ShopifySendOrderToERPMessage message, CancellationToken cancellationToken)
        {
            var queryClient = await _apiActorGroup.Ask<ReturnMessage<ListarClientesResumidoOmieRequestOutput>>(
                                new ListarClientesResumidoOmieRequest(
                                    new ListarClientesResumidoOmieRequestInput
                                    {
                                        registros_por_pagina = 1,
                                        clientesFiltro = new ClientesFiltro
                                        {
                                            cnpj_cpf = message.Customer.Company.CleanDocument()
                                        }
                                    }
                                ), cancellationToken
                            );

            if (queryClient.Result == Result.Error)
            {
                if (queryClient.Error is OmieException)
                {
                    var omieError = ((OmieException)queryClient.Error).Error;
                    if (omieError.IsError())
                    {
                        _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), queryClient.Error.Message);
                        return new ReturnMessage<UpsertClienteOmieRequestOutput> { Result = Result.Error, Error = new Exception(queryClient.Error.Message) };
                    }
                }
                else
                {
                    _logger.Warning("OmieService - Error in UpdateOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), queryClient.Error.Message);
                    return new ReturnMessage<UpsertClienteOmieRequestOutput> { Result = Result.Error, Error = new Exception(queryClient.Error.Message) };
                }
            }

            var client = new UpsertClienteOmieRequestInput
            {
                codigo_cliente_integracao = message.Customer.Company.CleanDocument(),
                razao_social = string.Concat(message.Customer.FirstName, " ", message.Customer.LastName).Truncate(60),
                cnpj_cpf = message.Customer.Company.CleanDocument(),
                email = message.Customer.Email,
                nome_fantasia = string.Concat(message.Customer.FirstName, " ", message.Customer.LastName),
                estado = message.Customer.BillingAddress.State,
                cidade = message.Customer.BillingAddress.City,
                cep = message.Customer.BillingAddress.ZipCode,
                bairro = message.Customer.BillingAddress.District,
                endereco = message.Customer.BillingAddress.Address.Truncate(60),
                endereco_numero = message.Customer.BillingAddress.Number.Truncate(10),
                complemento = message.Customer.BillingAddress.Complement?.Truncate(60),
                telefone1_ddd = message.Customer.BillingAddress.DDD,
                telefone1_numero = message.Customer.BillingAddress.Phone
            };

            if (queryClient.Data?.clientes_cadastro_resumido?.Any() == true)
            {
                //existing cliente
                client.codigo_cliente_omie = queryClient.Data.clientes_cadastro_resumido.First().codigo_cliente;
                client.codigo_cliente_integracao = queryClient.Data.clientes_cadastro_resumido.First().codigo_cliente_integracao;
            }
            else
            {
                //new cliente
                client.tags = new List<Tag> { new Tag { tag = "E-COMMERCE" } };
            }

            var updateClientResult = await _apiActorGroup.Ask<ReturnMessage<UpsertClienteOmieRequestOutput>>(
                                new UpsertClienteOmieRequest(
                                    client
                                ), cancellationToken
                            );

            if (updateClientResult.Result == Result.Error)
            {
                _logger.Warning("OmieService - Error in UpdateOrder | Erro message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, omieData), updateClientResult.Error.Message);
                return new ReturnMessage<UpsertClienteOmieRequestOutput> { Result = Result.Error, Error = new Exception(updateClientResult.Error.Message) };
            }

            return updateClientResult;
        }

        private string GetCodigoParcela(int qtdeParcelas, OmieData omieData)
        {
            if (omieData.CodigoParcelaFixa)
                return "999";

            var qtdeParcelasPermitidas = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 10, 12, 24, 36, 48 };

            if (omieData.CodigoParcelaMapping.Count > 0)
            {
                var parcela = omieData.CodigoParcelaMapping.Where(x => x.QuantidadeParcela == qtdeParcelas).FirstOrDefault();
                if (parcela != null)
                    return parcela.CodigoParcela;
            }

            if (qtdeParcelas <= 1)
                return "S28"; //a Vista/30

            if (qtdeParcelasPermitidas.Contains(qtdeParcelas))
                return qtdeParcelas.ToString().PadLeft(3, '0');

            return "";
        }

        public async Task<ReturnMessage> ListOrder(string externalOrderId, OmieData omieData, QueueClient shopifyUpdateOrderStatusQueueClient, CancellationToken cancellationToken)
        {
            PedidoVendaResult currentData = null;

            if (!string.IsNullOrWhiteSpace(externalOrderId))
            {
                var queryOrder = await _apiActorGroup.Ask<ReturnMessage<ConsultarPedidoOmieRequestOutput>>(
                                      new ConsultarPedidoOmieRequest(
                                          new ConsultarPedidoOmieRequestInput
                                          {
                                              codigo_pedido_integracao = externalOrderId
                                          }
                                      ), cancellationToken
                                  );

                if (queryOrder.Result == Result.Error)
                {
                    _logger.Warning("OmieService - Error in ListOrder | Error message: {1} | {0}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ConsultarPedidoOmieRequestInput
                    {
                        codigo_pedido_integracao = externalOrderId
                    }, omieData), queryOrder.Error.Message);

                    await Task.Delay(2000);
                    return new ReturnMessage { Result = Result.Error, Error = queryOrder.Error };
                }

                if (queryOrder.Data.pedido_venda_produto != null)
                    currentData = queryOrder.Data.pedido_venda_produto;
            }

            if (currentData != null && currentData.cabecalho != null)
            {
                if (currentData.cabecalho.origem_pedido == "API")
                {
                    int.TryParse(currentData.cabecalho.etapa, out int etapa);
                    int.TryParse(omieData.CodigoEtapaPaymentConfirmed, out int codigoEtapaPaymentConfirmed);

                    var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderStatusMessage
                    {
                        OrderExternalId = currentData.cabecalho.codigo_pedido_integracao,
                        Cancellation = new ShopifyUpdateOrderStatusMessage.CancellationStatus
                        {
                            IsCancelled = currentData.infoCadastro?.cancelado == "S"
                        },
                        Payment = new ShopifyUpdateOrderStatusMessage.PaymentStatus
                        {
                            IsPaid = etapa >= codigoEtapaPaymentConfirmed
                        },
                        Shipping = new ShopifyUpdateOrderStatusMessage.ShippingStatus
                        {
                            IsShipped = etapa == 70, //Entrega
                            IsDelivered = etapa == 80, //Finalizado
                            TrackingObject = currentData.frete?.codigo_rastreio
                        }
                    }); ;

                    await shopifyUpdateOrderStatusQueueClient.SendAsync(serviceBusMessage.GetMessage(currentData.cabecalho.codigo_pedido_integracao));
                }
                return new ReturnMessage { Result = Result.OK };
            }
            else
            {
                throw new Exception($"order {externalOrderId} not found found");
            }
        }

        public string GetShopifySkuCode(OmieData omieData, long skuId, string skuCode)
        {
            if (omieData.AppendSkuCode)
                return $"{skuId}-{skuCode}";
            else
                return $"{skuId}";
        }

        public (long id, string code) SplitShopifySkuCode(string skuCode)
        {
            if (skuCode.Contains('-'))
            {
                var id = long.Parse(skuCode.Substring(0, skuCode.IndexOf('-')));
                var code = skuCode.Substring(skuCode.IndexOf('-') + 1);
                return (id, code);
            }
            else
            {
                return (long.Parse(skuCode), "");
            }
        }

        private string GetTitle(ProdutoCadastroResult product, OmieData omieData)
        {

            var productName = omieData.NameField switch
            {
                NameFieldOmieType.descricao => product.descricao,
                NameFieldOmieType.descricao_familia => product.descricao_familia,
                _ => product.descricao_familia
            };

            if (!string.IsNullOrEmpty(omieData.NormalizeProductName))
            {
                productName = (Regex.Replace(productName, @$"(?:{Regex.Escape(omieData.NormalizeProductName)}.*)", "")).Trim();
            }
            return productName;

        }
    }
}
