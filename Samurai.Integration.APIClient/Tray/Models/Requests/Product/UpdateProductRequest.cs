using Newtonsoft.Json;
using Samurai.Integration.APIClient.Tray.Models.Response.Product;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Product
{
    public class UpdateProductRequest : Inputs.Product
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public void From(Response.Inputs.ProductModel productExist, Domain.Messages.Tray.Models.Product.Info productUpdate)
        {
            NumberFormatInfo FormatInfo = new NumberFormatInfo
            {
                CurrencyGroupSeparator = ".",
                CurrencySymbol = "$"
            };

            Id = productExist.Id.ToString();
            Ean = !string.IsNullOrEmpty(productUpdate.Ean) && productUpdate.Ean != productExist.Ean ? productUpdate.Ean : null;
            Name = !string.IsNullOrEmpty(productUpdate.Name) && productUpdate.Name != productExist.Name ? productUpdate.Name : null;
            Description = !string.IsNullOrEmpty(productUpdate.Description) && productUpdate.Description != productExist.Description ? productUpdate.Description : null;
            Price = productUpdate.Price != double.Parse(productExist.Price, NumberStyles.Currency, FormatInfo) ? productUpdate.Price : null;
            CostPrice = productUpdate.CostPrice != double.Parse(productExist.CostPrice, NumberStyles.Currency, FormatInfo) ? productUpdate.CostPrice : null;
            //IpiValue = productUpdate.IpiValue.ToString() != productExist.IpiValue ? productUpdate.IpiValue : null;
            Brand = !string.IsNullOrEmpty(productUpdate.Brand?.Brand) && productUpdate.Brand?.Brand != productExist.Brand ? productUpdate.Brand?.Brand : null;
            Model = !string.IsNullOrEmpty(productUpdate.Model) && productUpdate.Model != productExist.Model ? productUpdate.Model : null;
            Weight = productUpdate.Weight.ToString() != productExist.Weight ? productUpdate.Weight : null;
            Length = productUpdate.Length.ToString() != productExist.Length ? productUpdate.Length : null;
            Width = productUpdate.Width.ToString() != productExist.Width ? productUpdate.Width : null;
            Height = productUpdate.Height.ToString() != productExist.Height ? productUpdate.Height : null;
            Stock = productUpdate.Stock.ToString() != productExist.Stock ? productUpdate.Stock : null;
            CategoryId = productUpdate.Category?.Id.ToString() != productExist.CategoryId ? productUpdate.Category?.Id : null;
            Available = productUpdate.Available.ToString() != productExist.Available ? productUpdate.Available : null;
            Reference = productUpdate.Reference != productExist.Reference ? productUpdate.Reference : null;
            RelatedCategories = productUpdate.RelatedCategories.Any(x => !productExist.RelatedCategories.Select(y => y).Contains(x.ToString())) ? productUpdate.RelatedCategories : null;
            //VirtualProduct = "0"; //Produto Normal,
            Availability = productUpdate.Availability != productExist.Availability ? productUpdate.Availability : null;
            AvailabilityDays = productUpdate.AvailabilityDays.ToString() != productExist.AvailabilityDays ? productUpdate.AvailabilityDays : null;
            //PictureSource1 = !string.IsNullOrEmpty(productUpdate.PictureSource1) ? productUpdate.PictureSource1 : null;
            //PictureSource2 = !string.IsNullOrEmpty(productUpdate.PictureSource1) ? productUpdate.PictureSource2 : null;
            //PictureSource3 = !string.IsNullOrEmpty(productUpdate.PictureSource1) ? productUpdate.PictureSource3 : null;
            //PictureSource4 = !string.IsNullOrEmpty(productUpdate.PictureSource1) ? productUpdate.PictureSource4 : null;
            //PictureSource5 = !string.IsNullOrEmpty(productUpdate.PictureSource1) ? productUpdate.PictureSource5 : null;
            //PictureSource6 = !string.IsNullOrEmpty(productUpdate.PictureSource1) ? productUpdate.PictureSource6 : null;
        }

         public bool HasProductUpdate(Response.Inputs.ProductModel productExist, Domain.Messages.Tray.Models.Product.Info productUpdate)
        {
            NumberFormatInfo FormatInfo = new NumberFormatInfo
            {
                CurrencyGroupSeparator = ".",
                CurrencySymbol = "$"
            };

            return ((!string.IsNullOrEmpty(productUpdate.Ean) && productUpdate.Ean != productExist.Ean) ||
                    (!string.IsNullOrEmpty(productUpdate.Name) && productUpdate.Name?.Trim() != productExist.Name?.Trim()) ||
                    (!string.IsNullOrEmpty(productUpdate.Description) && productUpdate.Description?.Trim() != productExist.Description?.Trim()) ||
                    (productUpdate.Price != double.Parse(productExist.Price, NumberStyles.Currency, FormatInfo)) ||
                    (productUpdate.CostPrice != double.Parse(productExist.CostPrice, NumberStyles.Currency, FormatInfo)) ||
                    //(productUpdate.IpiValue.ToString() != productExist.IpiValue) ||
                    (!string.IsNullOrEmpty(productUpdate.Brand?.Brand) && productUpdate.Brand?.Brand != productExist.Brand) ||
                    (!string.IsNullOrEmpty(productUpdate.Model) && productUpdate.Model != productExist.Model) ||
                    (productUpdate.Weight.ToString() != productExist.Weight) ||
                    (productUpdate.Length.ToString() != productExist.Length) ||
                    (productUpdate.Width.ToString() != productExist.Width) ||
                    (productUpdate.Height.ToString() != productExist.Height) ||
                    (productUpdate.Stock.ToString() != productExist.Stock) ||
                    (productUpdate.Category?.Id.ToString() != productExist.CategoryId) ||
                    (productUpdate.Available.ToString() != productExist.Available) ||
                    (productUpdate.Reference != productExist.Reference) ||
                    (productUpdate.RelatedCategories.Any(x => !productExist.RelatedCategories.Select(y => y).Contains(x.ToString()))) ||
                    (productUpdate.Availability != productExist.Availability)
                    //(productUpdate.AvailabilityDays.ToString() != productExist.AvailabilityDays)
                  );



        }
    }
}
