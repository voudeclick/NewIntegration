namespace Samurai.Integration.Domain.Models
{
    public class OrderNoteAttributesModelMercadoPagoInfo
    {
        public string PaymentMethod { get; set; }
        public string PaymentType { get; set; }
        public string PaymentInstallment { get; set; }
        public string ShippingFullAddress { get; set; }
        public string ShippingStreetName { get; set; }
        public string ShippingStreetNumber { get; set; }
        public string ShippingNeighborhood { get; set; }
        public string ShippingStreetComplement { get; set; }

        public int GetPaymentInstallment()
        {
            int.TryParse(PaymentInstallment, out var paymentInstallment);

            return paymentInstallment;
        }
    }
}
