using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Omie.Models.Request;
using VDC.Integration.APIClient.Omie.Models.Request.CategoriaCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.CenarioImposto;
using VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.ConsultaEstoque;
using VDC.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.EtapasFaturamento;
using VDC.Integration.APIClient.Omie.Models.Request.FamiliaCadastro;
using VDC.Integration.APIClient.Omie.Models.Request.LocalEstoque;
using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento;
using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto;
using VDC.Integration.APIClient.Omie.Models.Request.ProdutoCadastro;
using VDC.Integration.APIClient.Omie.Models.Result;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Messages.Omie;

namespace VDC.Integration.Application.Actors.Omie
{
    public class OmieApiActor : BaseOmieTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IHttpClientFactory _httpClientFactory;

        public OmieApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, OmieData omieData)
            : base("OmieApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _omieData = omieData;
            using (var scope = _serviceProvider.CreateScope())
            {
                _httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
            }

            ReceiveAsync<ListarCategoriasOmieRequest>(Receive);
            ReceiveAsync<ListarClientesOmieRequest>(Receive);
            ReceiveAsync<ListarClientesResumidoOmieRequest>(Receive);
            ReceiveAsync<UpsertClienteOmieRequest>(Receive);
            ReceiveAsync<ListarResumoContasCorrentesOmieRequest>(Receive);
            ReceiveAsync<ListarLocaisEstoqueOmieRequest>(Receive);
            ReceiveAsync<ConsultarFamiliaOmieRequest>(Receive);
            ReceiveAsync<PesquisarFamiliasOmieRequest>(Receive);
            ReceiveAsync<PosicaoEstoqueOmieRequest>(Receive);
            ReceiveAsync<ListarProdutosOmieRequest>(Receive);
            ReceiveAsync<ConsultarPedidoOmieRequest>(Receive);
            ReceiveAsync<IncluirPedidoOmieRequest>(Receive);
            ReceiveAsync<AlterarPedidoOmieRequest>(Receive);
            ReceiveAsync<ListarEtapasFaturamentoOmieRequest>(Receive);
            ReceiveAsync<TrocarEtapaPedidoOmieRequest>(Receive);
            ReceiveAsync<CancelarPedidoVendaOmieRequest>(Receive);
            ReceiveAsync<ListarCenariosOmieRequest>(Receive);
        }

        public async Task Receive<I, O>(BaseOmieRequest<I, O> message)
            where I : BaseOmieInput
            where O : BaseOmieOutput, new()
        {
            try
            {
                var client = message.CreateClient(_httpClientFactory, _omieData.AppKey, _omieData.AppSecret, _log);
                var response = await client.Post(message, _cancellationToken);
                Sender.Tell(new ReturnMessage<O> { Result = Result.OK, Data = response });
            }
            catch (OmieException ex)
            {
                if (ex.Error.IsError())
                    LogError(ex, "(OmieException Error)  Error in {0} | Error: {1}", message.GetType().Name, ex.Error?.faultstring);
                else
                    LogWarning("(OmieException Warning) Error in {0} | Error: {1}", message.GetType().Name, ex.Error?.faultstring);

                Sender.Tell(new ReturnMessage<O> { Result = Result.Error, Error = ex });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in {0}", message.GetType().Name);
                Sender.Tell(new ReturnMessage<O> { Result = Result.Error, Error = ex });
            }
        }


        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, OmieData omieData)
        {
            return Akka.Actor.Props.Create(() => new OmieApiActor(serviceProvider, cancellationToken, omieData));
        }
    }
}
