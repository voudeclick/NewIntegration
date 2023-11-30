using Samurai.Integration.Domain.Messages.Shopify;

namespace Samurai.Integration.Domain.Models
{
    public class OrderNoteAttributesModelInfo
    {
        public string VendorShopify { get; set; }
        public decimal Multiplo { get; set; }
        public ShopifySendOrderAddressToERPMessage NotesBillingAddress { get; set; }
        public ShopifySendOrderAddressToERPMessage NotesShippingAddress { get; set; }
        public string NoteBirthDate { get; set; }
        public string Issuer { get; set; }
        public int InstallmentQuantity { get; set; }
        public decimal InstallmentValue { get; set; }
        public string GetPayment { get; set; }
    }
}
