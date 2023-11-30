using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.ViewModel
{
    public class OrderStatusTranslationViewModel
    {
        [Required]        
        public string CultureName { get; set; }
        [Required]
        [MaxLength(256)]        
        public string BuyerDisplayName { get; set; }
        [Required]
        [MaxLength(256)]        
        public string SellerDisplayName { get; set; }
    }
}
