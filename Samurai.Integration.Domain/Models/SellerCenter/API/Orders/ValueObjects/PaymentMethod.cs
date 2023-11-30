using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum PaymentMethod
    {
        [Display(Name = "Cartão de Crédito")]
        CreditCard,
        Boleto,
        [Display(Name = "Cartão de Débito")]
        Debit
    }
}
