using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum TrackingPackagePostageStatus
    {
        [Display(Name = "Postagem Pendente")]
        PendingPostage,
        [Display(Name = "Não Postado")]
        NotPosted,
        [Display(Name = "Postado")]
        Posted,
        [Display(Name = "Entregue")]
        Delivered
    }
}
