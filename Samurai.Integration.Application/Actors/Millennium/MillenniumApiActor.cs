using Akka.Actor;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.API;
using Samurai.Integration.APIClient.Millennium;
using Samurai.Integration.APIClient.Millennium.Models.Requests;
using Samurai.Integration.APIClient.Millennium.Models.Results;
using Samurai.Integration.Domain.Entities;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Models.Millennium;
using Samurai.Integration.Domain.Results.Logger;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.longegration.APIClient.Millennium.Models.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Millennium
{
    public class MillenniumApiActor : BaseMillenniumTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly MillenniumLogin _login;
        private readonly MillenniumApiClient _client;
        private readonly APIClientGeneric _apiClientGeneric;
        private readonly MillenniumSessionToken _millenniumSessionToken;

        public MillenniumApiActor(IServiceProvider serviceProvider,
                                  CancellationToken cancellationToken,
                                  MillenniumData millenniumData,
                                  MillenniumLogin login,
                                  MillenniumSessionToken millenniumSessionToken) : base("MillenniumApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _millenniumData = millenniumData;
            _login = login;
            _millenniumSessionToken = millenniumSessionToken;
            using (var scope = _serviceProvider.CreateScope())
            {
                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

                _client = new MillenniumApiClient(httpClientFactory, millenniumData, _millenniumData.Url, _login.Login, _login.Password, _millenniumSessionToken, _log);

                if (_millenniumData.EnableExtraPaymentInformation)
                    _apiClientGeneric = new APIClientGeneric(httpClientFactory, _millenniumData.Id.ToString(), _millenniumData.UrlExtraPaymentInformation, _millenniumData.MercadoPago.Authorization);

            }

            ReceiveAsync((Func<MillenniumApiListProductsRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/millenium_eco/produtos/listavitrine";

                    var param = new Dictionary<string, string>() { };
                    param.Add("vitrine", _millenniumData.VitrineId.ToString());
                    param.Add("lista_preco", message.ListaPreco.ToString().ToLower());
                    if (message.Top != null)
                        param.Add("$top", message.Top.ToString());
                    if (message.ProductId != null)
                        param.Add("produto", message.ProductId.ToString());
                    if (message.TransId != null)
                        param.Add("trans_id", message.TransId.ToString());
                    if (!string.IsNullOrEmpty(message.DataAtualizacao))
                        param.Add("DATA_ATUALIZACAO", message.DataAtualizacao);

                    var response = await _client.GetContent<MillenniumApiListProductsResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);

                    if (_millenniumData.HasTenantLogging)
                        LogInfo(LoggerDescription.FromProduct(_millenniumData.Id.ToString(), message?.ProductId?.ToString(), "MillenniumApiListProductsRequest", message, response));

                    Sender.Tell(new ReturnMessage<MillenniumApiListProductsResult> { Result = Result.OK, Data = response.Item1 });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Exception:74 | {0}", ex.Message);
                    Sender.Tell(new ReturnMessage<MillenniumApiListProductsResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiListPricesRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/millenium_eco/produtos/precodetabela";

                    var param = new Dictionary<string, string>() { };
                    param.Add("vitrine", _millenniumData.VitrineId.ToString());
                    if (message.Top != null)
                        param.Add("$top", message.Top.ToString());
                    if (message.ProductId != null)
                        param.Add("produto", message.ProductId.ToString());
                    if (message.TransId != null)
                        param.Add("trans_id", message.TransId.ToString());
                    if (!string.IsNullOrEmpty(message.DataAtualizacao))
                        param.Add("DATA_ATUALIZACAO_INICIAL", message.DataAtualizacao);

                    var response = await _client.GetContent<MillenniumApiListPricesResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);

                    if (_millenniumData.HasTenantLogging)
                        LogInfo(LoggerDescription.FromProduct(_millenniumData.Id.ToString(), message?.ProductId?.ToString(), "MillenniumApiListPricesRequest", message, response));

                    Sender.Tell(new ReturnMessage<MillenniumApiListPricesResult> { Result = Result.OK, Data = response.Item1 });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<MillenniumApiListPricesResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiListStocksRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/millenium_eco/produtos/saldodeestoque";

                    var param = new Dictionary<string, string>() { };
                    param.Add("vitrine", _millenniumData.VitrineId.ToString());
                    if (message.Top != null)
                        param.Add("$top", message.Top.ToString());
                    if (message.ProductId != null)
                        param.Add("produto", message.ProductId.ToString());
                    if (message.TransId != null)
                        param.Add("trans_id", message.TransId.ToString());
                    if (!string.IsNullOrEmpty(message.DataAtualizacao))
                        param.Add("DATA_ATUALIZACAO_INICIAL", message.DataAtualizacao);

                    var response = await _client.GetContent<MillenniumApiListStocksResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);


                    Sender.Tell(new ReturnMessage<MillenniumApiListStocksResult> { Result = Result.OK, Data = response.Item1 });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<MillenniumApiListStocksResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiCreateOrderRequest, Task>)(async message =>
            {
                try
                {
                    message.vitrine = _millenniumData.VitrineId;

                    string url = "api/millenium_eco/pedido_venda/inclui";

                    await _client.Post(url, message, _cancellationToken);

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<PaymentExtraInfoRequest, Task>)(async message =>
            {
                try
                {
                    //Pensar numa forma melhor                    
                    var response = await _apiClientGeneric.Get(QueryHelpers.AddQueryString(message.Method, message.Params), _cancellationToken);

                    if (message.TypeOf == "PaymentExtraInfoBrasPagResult")
                        Sender.Tell(new ReturnMessage<PaymentExtraInfoBrasPagResult> { Result = Result.OK, Data = JsonConvert.DeserializeObject<PaymentExtraInfoBrasPagResult>(response) });
                    else if (message.TypeOf == "PaymentExtraInfoMoipResult")
                        Sender.Tell(new ReturnMessage<PaymentExtraInfoMoipResult> { Result = Result.OK, Data = JsonConvert.DeserializeObject<PaymentExtraInfoMoipResult>(response) });
                    else if (message.TypeOf == "PaymentExtraInfoMercadoPagoResult")
                        Sender.Tell(new ReturnMessage<PaymentExtraInfoMercadoPagoResult> { Result = Result.OK, Data = JsonConvert.DeserializeObject<PaymentExtraInfoMercadoPagoResult>(response) });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiUpdateOrderStatusRequest, Task>)(async message =>
            {
                try
                {
                    message.vitrine = _millenniumData.VitrineId;

                    string url = "api/millenium_eco/pedido_venda/processastatus";

                    var response = await _client.Post<MillenniumApiUpdateOrderStatusResult>(url, message, _cancellationToken);

                    var errors = response.acoes.Where(a => a.acao == 100);
                    if (errors.Any())
                        throw new Exception($"{string.Join(',', errors.Select(e => e.erro))}");

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiListOrdersRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/millenium_eco/pedido_venda/listapedidos";

                    var param = new Dictionary<string, string>() { };
                    param.Add("vitrine", _millenniumData.VitrineId.ToString());
                    param.Add("nao_lista_detalhe", "true");
                    if (message.Top != null)
                        param.Add("$top", message.Top.ToString());
                    if (message.ExternalOrderId != null)
                        param.Add("cod_pedidov", message.ExternalOrderId.ToString());
                    if (message.TransId != null)
                        param.Add("trans_id", message.TransId.ToString());
                    if (message.DataInicial != null)
                        param.Add("data_emissao_inicial", message.DataInicial);
                    if (message.DataFinal != null)
                        param.Add("data_emissao_final", message.DataFinal);

                    var response = await _client.Get<MillenniumApiListOrdersResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);

                    Sender.Tell(new ReturnMessage<MillenniumApiListOrdersResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<MillenniumApiListOrdersResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiListOrdersStatusRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/millenium_eco/pedido_venda/consultastatus";

                    var param = new Dictionary<string, string>() { };
                    param.Add("vitrine", _millenniumData.VitrineId.ToString());
                    param.Add("data_atualizacao_inicial", DateTime.Today.AddMonths(-6).ToString("yyyy-MM-dd"));
                    param.Add("list_pedidov", string.Concat("(", string.Join(',', message.OrderIds), ")"));

                    var response = await _client.Get<MillenniumApiListOrdersStatusResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    Sender.Tell(new ReturnMessage<MillenniumApiListOrdersStatusResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<MillenniumApiListOrdersStatusResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiGetListIdFotoRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/millenium_eco/produtos/listaidfotos";

                    var param = new Dictionary<string, string>() { };
                    param.Add("produto", message.CodProduto.ToString());
                    if (message.Top != null)
                        param.Add("$top", message.Top.ToString());

                    var response = await _client.Get<MillenniumApiGetListIdFotoResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    Sender.Tell(new ReturnMessage<MillenniumApiGetListIdFotoResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<MillenniumApiGetListIdFotoResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiBuscaFotoRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/millenium_eco/produtos/buscafoto";

                    var param = new Dictionary<string, string>() { };
                    param.Add("idfoto", message.IdFoto.ToString());

                    var response = await _client.Get<MillenniumApiBuscaFotoResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    Sender.Tell(new ReturnMessage<MillenniumApiBuscaFotoResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<MillenniumApiBuscaFotoResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumApiListStockMtoRequest, Task>)(async message =>
            {
                try
                {
                    string url = "api/MILLENIUM!INSECTA_ESTRATEGIA_FILIAL/PRODUTOS/ListaCapacidadeProducao";

                    var param = new Dictionary<string, string>() { };
                    param.Add("vitrine", _millenniumData.VitrineId.ToString());
                    param.Add("$format", "json");
                    if (message.Top != null)
                        param.Add("$top", message.Top.ToString());
                    if (!string.IsNullOrEmpty(message.EstrategiaProducao))
                        param.Add("estrategia_de_producao", message.EstrategiaProducao);
                    if (message.TransId != null)
                        param.Add("trans_id", message.TransId.ToString());
                    if (message.Filiais != null)
                        param.Add("filiais", $"({string.Join(",", message.Filiais)})");

                    var response = await _client.Get<MillenniumApiListStockMtoResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    Sender.Tell(new ReturnMessage<MillenniumApiListStockMtoResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<MillenniumApiListStockMtoResult> { Result = Result.Error, Error = ex });
                }
            }));
        }

        public static Props Props(IServiceProvider serviceProvider,
                                  CancellationToken cancellationToken,
                                  MillenniumData millenniumData,
                                  MillenniumLogin login,
                                  MillenniumSessionToken millenniumSessionToken)
        {
            return Akka.Actor.Props.Create(() => new MillenniumApiActor(serviceProvider, cancellationToken, millenniumData, login, millenniumSessionToken));
        }
    }
}
