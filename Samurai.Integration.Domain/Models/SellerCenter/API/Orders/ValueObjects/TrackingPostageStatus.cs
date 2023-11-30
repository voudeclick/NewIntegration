using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum TrackingPostageStatus
    {
        [Display(Name = "Postagem Pendente")]
        PendingPostage,
        [Display(Name = "Não Postado")]
        NotPosted,
        [Display(Name = "Parcialmente Postado")]
        PartiallyPosted,
        [Display(Name = "Postado")]
        Posted,
        [Display(Name = "Parcialmente Entregue")]
        PartiallyDelivered,
        [Display(Name = "Entregue")]
        Delivered
    }
}
