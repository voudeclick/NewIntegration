using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum PhoneType
    {
        [Display(Name = "Fixo")]
        Home,
        [Display(Name = "Trabalho")]
        Work,
        [Display(Name = "Celular")]
        Mobile
    }
}