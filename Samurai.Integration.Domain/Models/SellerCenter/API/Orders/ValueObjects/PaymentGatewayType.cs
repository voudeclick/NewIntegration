using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum PaymentGatewayType
    {
        [Display(Name = "Externo")]
        External,
        Moip,
        [Display(Name = "Pago")]
        PaidPayments,
        Braspag
    }
}
