using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public enum ApprovalStatus
    {
        [Display(Name = "Aprovado")]
        Approved,
        [Display(Name = "Aguardando aprovação")]
        WaitingForApproval,
        [Display(Name = "Reprovado")]
        Disapproved
    }
}