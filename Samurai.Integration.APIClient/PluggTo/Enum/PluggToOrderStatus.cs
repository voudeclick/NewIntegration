using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Enum
{
    public enum PluggToOrderStatus
    {
        approved, //Aprovado:
        pending, // Pendente
        shipped, //Entrega Aceita (Enviado)
        shipping_error, //(Erro na entrega)
        partial_payment, // (Pagamento Parcial)
        invoice_error, // (Erro ao gerar nota fiscal)
        invoiced, //Faturado
        shipping_informed, //Entrega informada
        delivered, //(Entregue)
        canceled, //(Cancelado)

    }
}
