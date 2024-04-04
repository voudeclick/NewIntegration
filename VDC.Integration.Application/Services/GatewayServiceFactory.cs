using System.Collections.Generic;

namespace VDC.Integration.Application.Services
{
    public static class GatewayServiceFactory 
    {
        public static IGatewayService GetService(List<string> gatewayNames)
        {
            if(gatewayNames.Contains("mercado_pago"))
                return new MercadoPagoService();
            
            return null;
        }
    }
}
