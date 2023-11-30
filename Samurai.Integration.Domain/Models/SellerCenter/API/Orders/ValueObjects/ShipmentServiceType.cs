using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum ShipmentServiceType
    {
        [Display(Name = "Externo")]
        External,
        [Display(Name = "Retirar na loja")]
        PickupInStore,
        Correios,
        JadLog
    }
}
