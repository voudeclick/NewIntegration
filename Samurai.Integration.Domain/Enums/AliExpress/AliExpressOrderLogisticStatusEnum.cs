using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Enums.AliExpress
{
    public enum AliExpressOrderLogisticStatusEnum
    {
        WAIT_SELLER_SEND_GOODS, //Aguardando o envio do vendedor
        SELLER_SEND_PART_GOODS, // Enviado parcialmente pelo vendedor
        SELLER_SEND_GOODS, // O vendedor enviou
        BUYER_ACCEPT_GOODS, // O comprador confirmou o recebimento
        NO_LOGISTICS, // Sem dados de logistica
    }
}
