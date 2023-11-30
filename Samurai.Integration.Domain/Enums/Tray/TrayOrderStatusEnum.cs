using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Enums.Tray
{
    public enum TrayOrderStatusEnum
    {
        AguardandoPagamento = 1,
        PedidoPagoAguardandoEnvio = 2,
        PedidoEnviado = 3,
        PedidoEntregue = 4,
        PedidoCancelado = 5,
        ParcialmenteCancelado = 8
    }
}
