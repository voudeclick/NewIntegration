using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class BuyerAddressViewModel
    {
        [Required]
        [MaxLength(256)]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required]
        [MaxLength(256)]
        [DataType(DataType.PostalCode)]
        [Display(Name = "CEP")]
        public string ZipCode { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Logradouro")]
        public string Street { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Número")]
        public string Number { get; set; }

        [MaxLength(256)]
        [Display(Name = "Complemento")]
        public string Complement { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Bairro")]
        public string District { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Cidade")]
        public string City { get; set; }

        [Required]
        [MaxLength(2)]
        [Display(Name = "Estado")]
        public string State { get; set; }

        [MaxLength(3)]
        [Display(Name = "País")]
        public string Country { get; set; }
    }
}