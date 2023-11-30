using Samurai.Integration.Domain.Models.SellerCenter.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class BuyerPhoneDto
    {
        public BuyerPhoneDto(){}
        public BuyerPhoneDto(PhoneViewModel phoneView)
        {
            Type = (PhoneType)Enum.Parse(typeof(PhoneType),phoneView.Type.ToString());
            CountryCode = phoneView.CountryCode;
            AreaCode = phoneView.AreaCode;
            Number = phoneView.Number;
            Extension = phoneView.Extension;
        }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Tipo")]
        public PhoneType Type { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Código do país")]
        public string CountryCode { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "DDD")]
        public string AreaCode { get; set; }

        [Required]
        [MaxLength(256)]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Telefone")]
        public string Number { get; set; }

        [MaxLength(256)]
        [Display(Name = "Ramal")]
        public string Extension { get; set; }
    }
}