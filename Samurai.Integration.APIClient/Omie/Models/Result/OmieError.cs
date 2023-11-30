using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.APIClient.Omie.Models.Result
{
    public class OmieError
    {
        public string faultstring { get; set; }
        public string faultcode { get; set; }

        public bool IsWarning()
        {
            return new List<string>
            {
                "Pedido não cadastrado para o Código de Integração",
                "Não existem registros para a página",
                "Esta requisição já foi processada ou está sendo processada e você pode tentar novamente",                
                "pode tentar novamente",
                "cadastrado para o",
                "A chave de acesso está inválida ou o aplicativo está suspenso"
            }.Any(e => faultstring.ToLower().Contains(e.ToLower()));
        }
        public bool IsError()
        {
            return !IsWarning();
        }
    }
}
