using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs
{
    public class VariationOption
    {
        [Display(Name = "Nome da opção de variação")]
        public string VariationOptionName { get; set; }

        [Display(Name = "Valor da opção de variação")]
        public string Value { get; set; }

        [Display(Name = "Opção")]
        public Guid? VariationOptionId { get; set; }

        [Display(Name = "Valor")]
        public Guid? VariationOptionAvailableValueId { get; set; }

        public string ImageUrl { get; set; }

    } 
}
