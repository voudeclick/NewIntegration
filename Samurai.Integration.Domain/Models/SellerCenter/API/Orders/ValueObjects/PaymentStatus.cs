using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum PaymentStatus
    {
        [Display(Name = "Pendente")]
        Pending,
        [Display(Name = "Autorizado")]
        Authorized,
        [Display(Name = "Cancelado")]
        Cancelled,
        [Display(Name = "Reembolsado")]
        Refunded,
        [Display(Name = "Estornado")]
        Reversed,
        [Display(Name = "Concluído")]
        ConcludedWithErrors
    }
}
