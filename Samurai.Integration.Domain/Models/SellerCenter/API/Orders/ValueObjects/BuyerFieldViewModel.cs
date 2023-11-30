using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class BuyerFieldViewModel
    {
        public Guid BuyerFieldDefinitionId { get; set; }

        [Display(Name = "Nome")]
        public string FieldName { get; set; }

        [Display(Name = "Tipo")]
        public CustomFieldType FieldType { get; set; }

        [Display(Name = "Obrigatório")]
        public bool Required { get; set; }

        [Display(Name = "Traduções")]
        public List<BuyerFieldTranslationViewModel> Translations { get; set; }
    }
}