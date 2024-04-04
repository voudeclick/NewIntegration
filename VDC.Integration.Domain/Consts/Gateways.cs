namespace VDC.Integration.Domain.Consts
{
    public static class Gateways
    {
        public const string Cielo = "cielo";
        public const string BaseCielo = "https://cielo.azurewebsites.net";
        public const string MethodCielo = "Payment/GetNoteAttributes";

        public const string Moip = "checkout_moip";
        public const string BaseMoip = "http://checkoutmoip.azurewebsites.net";
        public const string MethodMoip = "Payment/GetNoteAttributes";
    }
}
