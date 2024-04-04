using VDC.Integration.Domain.Entities.Database.MethodPayment;

namespace VDC.Integration.Domain.Models.Millennium
{
    public class MethodPaymentTypeViewModel
    {
        public int Id { get; set; }
        public string PaymentTypeShopify { get; set; }
        public string PaymentTypeMillenniun { get; set; }
        public long TenantId { get; set; }

        public MethodPaymentTypeViewModel() { }

        public MethodPaymentTypeViewModel(MethodPayment methodPayment)
        {
            Id = methodPayment.Id;
            PaymentTypeShopify = methodPayment.PaymentTypeShopify;
            PaymentTypeMillenniun = methodPayment.PaymentTypeMillenniun;
            TenantId = methodPayment.TenantId;
        }
    }
}