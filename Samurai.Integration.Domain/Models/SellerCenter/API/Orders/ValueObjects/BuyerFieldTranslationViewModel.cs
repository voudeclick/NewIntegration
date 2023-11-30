using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class BuyerFieldTranslationViewModel
    {
        [Required]
        [Display(Name = "Idioma")]
        public Culture CultureName { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Tradução")]
        public string Value { get; set; }
    }
}