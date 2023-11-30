namespace Samurai.Integration.Domain.Models.Millennium
{    
    public static class MillenniumStatusPedido
    {
        //(0=Aguardando Pagamento,1=Pagamento Confirmado,2=Em Separação,3=Despachado,4=Entregue,5=Cancelado,6=Problemas,7=Embarcado,8=Falha na Entega)

        public static int AguardandoPagamento => 0;
        public static int PagamentoConfirmado => 1;
        public static int EmSeparacao => 2;
        public static int Despachado => 3;
        public static int Entregue => 4;
        public static int Cancelado => 5;
        public static int Problemas => 6;
        public static int Embarcado => 7;
        public static int FalhaNaEntega => 8;
    }
}
