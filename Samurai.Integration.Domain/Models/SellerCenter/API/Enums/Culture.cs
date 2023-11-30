using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Enums
{
    public enum Culture
    {
        // Annotation used to mantain culture standard on data-base and use enum standard at same time
        [EnumMember(Value = "en-US")]
        [Display(Name = "en-US")]        
        enUS,
        [EnumMember(Value = "pt-BR")]
        [Display(Name = "pt-BR")]
        ptBR,
        [EnumMember(Value = "es-ES")]
        [Display(Name = "es-ES")]
        esES,
        [EnumMember(Value = "ja-JP")]
        [Display(Name = "ja-JP")]
        jaJP
    }
}
