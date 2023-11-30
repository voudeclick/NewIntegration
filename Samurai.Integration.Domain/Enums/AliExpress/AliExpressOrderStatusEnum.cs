using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Enums.AliExpress
{
    public enum AliExpressOrderStatusEnum
    {
        PLACE_ORDER_SUCCESS, //Aguardando o comprador pagar
        IN_CANCEL, //Cancelamento da solicitação do comprador
        WAIT_SELLER_SEND_GOODS, //Aguardando sua remessa
        SELLER_PART_SEND_GOODS, //Enviado parcialmente
        WAIT_BUYER_ACCEPT_GOODS, //Aguardando o comprador receber a mercadoria
        FUND_PROCESSING, //Os compradores concordam, processamento de fundos
        IN_ISSUE, //Pedidos em disputas
        IN_FROZEN, //Pedidos em congelamento
        WAIT_SELLER_EXAMINE_MONEY, //Aguardando sua confirmação de valor
        RISK_CONTROL,  //Os pedidos são feitos em 24 horas de controle de risco, iniciando 24 horas após a finalização do pagamento online do comprador.
        FINISH //FINISH
    }
}

