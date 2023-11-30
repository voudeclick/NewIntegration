using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum PersonType
    {
        [Display(Name = "Física")]
        Person,
        [Display(Name = "Jurídica")]
        Company
    }
}