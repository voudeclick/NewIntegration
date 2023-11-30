using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Bling.Models.Results
{
    public class BlingApiOrderResult
    {
        public Retorno retorno { get; set; }
    }

    public class Retorno
    {
        public List<PedidoWrapper> pedidos { get; set; }
        public List<ErroWrapper> erros { get; set; }
    }

    public class PedidoWrapper
    {
        public Pedido pedido { get; set; }
    }

    public class Pedido
    {
        public string numero { get; set; }
        public long idPedido { get; set; }
        public string situacao { get; set; }
        public string numeroPedidoLoja { get; set; }
        public CodigosRastreamento codigos_rastreamento { get; set; }
        public List<Volume> volumes { get; set; }
    }

    public class CodigosRastreamento
    {
        public string codigo_rastreamento { get; set; }
    }

    public class VolumeWrapper
    {
        public Volume volume { get; set; }
    }

    public class Volume
    {
        public string servico { get; set; }
        public string codigoRastreamento { get; set; }
    }
}
