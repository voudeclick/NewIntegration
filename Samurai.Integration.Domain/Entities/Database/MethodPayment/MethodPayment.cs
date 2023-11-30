using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Samurai.Integration.Domain.Entities.Database
{
    public class MethodPayment 
    {
        [Key]
        public int Id { get; private set; }

        [Required(ErrorMessage = "PaymentTypeShopify is required")]
        public string  PaymentTypeShopify { get; private set; }

        [Required(ErrorMessage = "PaymentTypeMillenniun is required")]
        public string PaymentTypeMillenniun { get; private set; }
        
        public long TenantId { get; private set; }

        [Required]
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }

        public MethodPayment() { }

        public void Up(string paymentTypeShopify, string paymentTypeMillenniun, long tenantId)
        {
            if (!string.IsNullOrWhiteSpace(paymentTypeShopify) && !string.IsNullOrWhiteSpace(paymentTypeMillenniun) && tenantId > 0)
            {
                PaymentTypeShopify = paymentTypeShopify;
                PaymentTypeMillenniun = paymentTypeMillenniun;
                TenantId = tenantId;
            }
        }
    }
}
