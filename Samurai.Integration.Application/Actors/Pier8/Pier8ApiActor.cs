using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Pier8;
using Samurai.Integration.APIClient.Pier8.Models;
using Samurai.Integration.APIClient.Pier8.Models.Requests.Pedido;
using Samurai.Integration.APIClient.Pier8.Models.Response;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Pier8;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Pier8
{
    public class Pier8ApiActor : BasePier8TenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly string _url;
        private readonly Pier8ApiClient _client;

        public Pier8ApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken,
                Pier8DataMessage pier8DataMessageData,
                string url,
                Credentials credentials)
            : base("Pier8ApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _pier8DataMessage = pier8DataMessageData;
            _url = url;
            using (var scope = _serviceProvider.CreateScope())
            {
                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
                _client = new Pier8ApiClient(httpClientFactory, _url, credentials,_log);
            }

            ReceiveAsync((Func<ConsultaPedidoRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting ConsultaPedidoRequest");
                    string url = "/pier8v3/ws/consultaPedido.php";
                    var response = await _client.Post<ConsultaPedidoResponse>(url, message, _cancellationToken);
                    LogDebug("Ending ConsultaPedidoRequest");
                    Sender.Tell(new ReturnMessage<ConsultaPedidoResponse> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Pier8ApiActor - Error in ConsultaPedidoRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage<ConsultaPedidoResponse> { Result = Result.Error, Error = ex });
                }
            }));

        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, Pier8DataMessage pier8Data, string url, Credentials credentials)
        {
            return Akka.Actor.Props.Create(() => new Pier8ApiActor(serviceProvider, cancellationToken, pier8Data, url, credentials));
        }
    }
}
