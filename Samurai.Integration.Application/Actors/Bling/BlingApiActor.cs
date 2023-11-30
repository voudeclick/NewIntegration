using Akka.Actor;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Bling;
using Samurai.Integration.APIClient.Bling.Models.Requests;
using Samurai.Integration.APIClient.Bling.Models.Results;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Results.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Bling
{
    public class BlingApiActor : BaseBlingTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly BlingApiClient _client;

        public BlingApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData)
            : base("BlingApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _blingData = blingData;

            using (var scope = _serviceProvider.CreateScope())
            {
                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
                _client = new BlingApiClient(httpClientFactory, _blingData.ApiBaseUrl, _blingData.APIKey, _log);
            }


            ReceiveAsync((Func<BlngApiListProductsRequest, Task>)(async message =>
            {
                try
                {
                    var url = string.IsNullOrWhiteSpace(message.ProductCode) ? "Api/v2/produtos/json" : $"Api/v2/produto/{message.ProductCode}/json";

                    var param = new Dictionary<string, string>
                    {
                        { "estoque", "S" },
                        { "imagem", "S" }
                    };

                    url = QueryHelpers.AddQueryString(url, param);

                    if (message.ProductUpdatedDate.HasValue)
                    {
                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);
                        url = String.Concat(url, $"&filters=dataAlteracao[{message.ProductUpdatedDate.Value:dd/MM/yyyy HH:mm:ss} TO {now:dd/MM/yyyy}]");                        
                    }

                    BlingApiListProductsResult response = null;
                    var products = new List<BlingApiListProductsResult.ProdutoWrapper>();

                    var page = 1;

                    while (true)
                    {
                        try
                        {
                            response = await _client.Get<BlingApiListProductsResult>(url, _cancellationToken);

                            if (response?.retorno?.produtos?.Any() == false)
                                break;

                            if (!string.IsNullOrWhiteSpace(message.CategoriaId)) //vitrine virtual
                                products.AddRange(response.retorno.produtos.Where(x => x.produto.categoria.id == message.CategoriaId));
                            else
                                products.AddRange(response.retorno.produtos);


                            if (response?.retorno?.produtos?.Count < 100)
                                break;

                            page += 1;
                            url = $"Api/v2/produtos/page={page}/json";

                            if (page % 5 == 0)
                                Thread.Sleep(2000);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    Sender.Tell(new ReturnMessage<BlingApiListProductsResult>
                    {
                        Result = Result.OK,
                        Data = new BlingApiListProductsResult
                        {
                            retorno = new BlingApiListProductsResult.Retorno
                            {
                                produtos = products
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<BlingApiListProductsResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<BlingApiCreateOrderRequest, Task>)(async message =>
            {
                try
                {
                    string url = "Api/v2/pedido/json";

                    var response = await _client.PostXML<BlingApiOrderResult>(url, message, _cancellationToken);

                    Sender.Tell(new ReturnMessage<BlingApiOrderResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex,LoggerDescription.FromOrder(_blingData.Id.ToString(), message.NumeroLoja, "BlingApiCreateOrderRequest", message, ex));
                    Sender.Tell(new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<BlingApiListOrderResquest, Task>)(async message =>
            {
                try
                {
                    string url = $"Api/v2/pedido/{message.NumeroPedido}/json";

                    var response = await _client.Get<BlingApiOrderResult>(url, _cancellationToken);

                    Sender.Tell(new ReturnMessage<BlingApiOrderResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<BlingApiUpdateOrderRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"Api/v2/pedido/{message.NumeroPedido}/json";

                    var response = await _client.PutXML<BlingApiOrderResult>(url, message, _cancellationToken);

                    Sender.Tell(new ReturnMessage<BlingApiOrderResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex, LoggerDescription.FromOrder(_blingData.Id.ToString(), message.NumeroPedido, "BlingApiUpdateOrderRequest", message, ex));

                    Sender.Tell(new ReturnMessage<BlingApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<BlingGetAllSituationRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"Api/v2/situacao/{message.Modulo}/json/";

                    var response = await _client.Get<BlingGetAllSituationResult>(url, _cancellationToken);

                    Sender.Tell(new ReturnMessage<BlingGetAllSituationResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<BlingGetAllSituationResult> { Result = Result.Error, Error = ex });
                }
            }));
            ReceiveAsync((Func<BlingUpdateStatusOrderRequest, Task>)(async message =>
            {
                try
                {
                    string url = $"Api/v2/pedido/{message.IdPedido}/json/";

                    var response = await _client.PutXML<BlingUpdateStatusOrderResult>(url, message, _cancellationToken);

                    Sender.Tell(new ReturnMessage<BlingUpdateStatusOrderResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex,LoggerDescription.FromOrder(_blingData.Id.ToString(), message.IdPedido, "BlingUpdateStatusOrderRequest", message, ex));

                    Sender.Tell(new ReturnMessage<BlingUpdateStatusOrderResult> { Result = Result.Error, Error = ex });
                }
            }));
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData)
        {
            return Akka.Actor.Props.Create(() => new BlingApiActor(serviceProvider, cancellationToken, blingData));
        }
    }
}
