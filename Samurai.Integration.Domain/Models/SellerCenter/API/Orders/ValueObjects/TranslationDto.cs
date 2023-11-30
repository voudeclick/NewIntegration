using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class TranslationDto
    {
        [Required]
        [Display(Name = "Idioma")]

        [JsonConverter(typeof(StringEnumConverter))]
        public Culture CultureName { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Nome")]
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }
        public string Model { get; set; }

        public string AlternateImageUrl { get; set; }
    }
}