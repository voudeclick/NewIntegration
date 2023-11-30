using Samurai.Integration.Domain.Extensions;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs
{
    public class Variation
    {
        public Variation()
        {
            Options = new List<VariationOption>();
        }
        public string SKU { get; set; }

        [Display(Name = "Código de Barras")]
        public string BarCode { get; set; }

        [Display(Name = "Digital?")]
        public bool IsDigital { get; set; }

        [Display(Name = "Peso")]
        public decimal? Weight { get; set; }

        [Display(Name = "Altura")]
        public decimal? Height { get; set; }

        [Display(Name = "Comprimento")]
        public decimal? Length { get; set; }

        [Display(Name = "Largura")]
        public decimal? Width { get; set; }

        [Display(Name = "Diâmetro")]
        public decimal? Diameter { get; set; }

        public List<VariationOption> Options { get; set; }

        public void UpdateFrom(Variation variationUpdate)
        {
            BarCode = variationUpdate.BarCode;
            IsDigital = variationUpdate.IsDigital;
            Weight = variationUpdate.Weight;
            Height = variationUpdate.Height;
            Length = variationUpdate.Length;
            Width = variationUpdate.Width;
            Diameter = variationUpdate.Diameter;

            Options.Merge(variationUpdate.Options,
                          option => option.VariationOptionId,
                          optionUpdate => optionUpdate.VariationOptionId,
                          optionUpdate => optionUpdate,
                          (option, optionUpdate) =>
                          {
                              option.VariationOptionAvailableValueId = optionUpdate.VariationOptionAvailableValueId;
                              option.Value = optionUpdate.Value;
                              option.ImageUrl = optionUpdate.ImageUrl;
                          });
        }
    }
}
