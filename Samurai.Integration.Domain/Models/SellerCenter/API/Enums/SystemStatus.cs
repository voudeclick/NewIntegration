using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SystemStatus
    {
        [Display(Name = "Aguardando Regitro Externo")]
        WaitingExternalRegistration,
        [Display(Name = "Recebido")]
        Received,
        [Display(Name = "Aguardando Pagamento")]
        WaitingPayment,
        [Display(Name = "Pago")]
        Paid,
        [Display(Name = "Enviado")]
        Sent,
        [Display(Name = "Entregue")]
        Delivered,
        [Display(Name = "Cancelado")]
        Canceled,
        [Display(Name = "Erro")]
        Error,
        [Display(Name = "Parcialmente Enviado")]
        PartiallySent,
        [Display(Name = "Parcialmente Entregue")]
        PartiallyDelivered,
        [Display(Name = "Parcialmente Cancelado")]
        PartiallyCancelled,
        [Display(Name = "Status Customizado")]
        Custom
    }
}
