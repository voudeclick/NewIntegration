using Newtonsoft.Json;
using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Models.SellerCenter.ViewModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class OrderStatusTranslationDto : TranslationDto
    {
        public OrderStatusTranslationDto() { }
        public OrderStatusTranslationDto(OrderStatusTranslationViewModel order) : base()
        {
            if (order is null)
            {
                CultureName = Culture.ptBR;
                BuyerDisplayName = "";
                SellerDisplayName = "";
            }
            else
            {
                object culture = Culture.ptBR;
                var requestCultureName = order?.CultureName?.Replace("-", string.Empty) ?? culture.ToString();
                Enum.TryParse(typeof(Culture), requestCultureName, out culture);

                CultureName = (Culture)culture;
                BuyerDisplayName = string.IsNullOrWhiteSpace(order.BuyerDisplayName) ? "" : order.BuyerDisplayName;
                SellerDisplayName = string.IsNullOrWhiteSpace(order.SellerDisplayName) ? "" : order.SellerDisplayName;
            }            
        }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Descrição para o cliente")]
        public string BuyerDisplayName { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Descrição para o vendedor")]
        public string SellerDisplayName { get; set; }
    }
}
