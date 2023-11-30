using Newtonsoft.Json;
using Samurai.Integration.APIClient.Tray.Models.Requests.Inputs;
using Samurai.Integration.APIClient.Tray.Models.Response.Inputs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Inputs
{
    public class ProductVariation
    {
        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("ean")]
        public string Ean { get; set; }

        [JsonProperty("order")]
        public int? Order { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("cost_price")]
        public double? CostPrice { get; set; }

        [JsonProperty("stock")]
        public long? Stock { get; set; }

        [JsonProperty("minimum_stock")]
        public int? MinimumStock { get; set; }

        [JsonProperty("weight")]
        public long? Weight { get; set; }

        [JsonProperty("length")]
        public long? Length { get; set; }

        [JsonProperty("width")]
        public long? Width { get; set; }

        [JsonProperty("height")]
        public long? Height { get; set; }

        //[JsonProperty("start_promotion")]
        //public DateTime? StartPromotion { get; set; }

        //[JsonProperty("end_promotion")]
        //public DateTime? EndPromotion { get; set; }

        //[JsonProperty("promotional_price")]
        //public decimal PromotionalPrice { get; set; }

        [JsonProperty("picture_source_1")]
        public string PictureSource1 { get; set; }

        [JsonProperty("picture_source_2")]
        public string PictureSource2 { get; set; }

        [JsonProperty("picture_source_3")]
        public string PictureSource3 { get; set; }

        [JsonProperty("picture_source_4")]
        public string PictureSource4 { get; set; }

        [JsonProperty("picture_source_5")]
        public string PictureSource5 { get; set; }

        [JsonProperty("picture_source_6")]
        public string PictureSource6 { get; set; }

        //[JsonProperty("quantity_sold")]
        //public int QuantitySold { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("type_1")]
        public string Type1 { get; set; }

        [JsonProperty("value_1")]
        public string Value1 { get; set; }

        [JsonProperty("type_2")]
        public string Type2 { get; set; }

        [JsonProperty("value_2")]
        public string Value2 { get; set; }

        public void From(VariationModel variationExist)
        {
            NumberFormatInfo FormatInfo = new NumberFormatInfo
            {
                CurrencyGroupSeparator = ".",
                CurrencySymbol = "$"
            };

            Ean = !string.IsNullOrEmpty(Ean) && Ean != variationExist.Ean ? Ean : null;
            Order = !string.IsNullOrEmpty(Order.ToString()) && Order.ToString() != variationExist.Order ? Order : null;
            Price = Price != double.Parse(variationExist.Price, NumberStyles.Currency, FormatInfo) ? Price : null;
            CostPrice = CostPrice != double.Parse(variationExist.CostPrice, NumberStyles.Currency, FormatInfo) ? CostPrice : null;
            Stock = Stock.ToString() != variationExist.Stock && Stock != 0 ? Stock : null;
            MinimumStock = MinimumStock.ToString() != variationExist.MinimumStock && MinimumStock != 0 ? MinimumStock : null;
            Weight = Weight.ToString() != variationExist.Weight && Weight != 0 ? Weight : null;
            Length = Length.ToString() != variationExist.Length && Length != 0 ? Length : null;
            Width = Width.ToString() != variationExist.Width && Width != 0 ? Width : null;
            Height = Height.ToString() != variationExist.Height && Height != 0 ? Height : null;
            Reference = Reference != variationExist.Reference ? Reference : null;
        }

        public bool HasUpdateVariant(VariationModel variationExist)
        {
            NumberFormatInfo FormatInfo = new NumberFormatInfo
            {
                CurrencyGroupSeparator = ".",
                CurrencySymbol = "$"
            };

            var variantUpdate = (!string.IsNullOrEmpty(Ean) && Ean != variationExist.Ean) ||
                           (!string.IsNullOrEmpty(Order.ToString()) && Order.ToString() != variationExist.Order) ||
                           (Price != double.Parse(variationExist.Price, NumberStyles.Currency, FormatInfo)) ||
                           (CostPrice != double.Parse(variationExist.CostPrice, NumberStyles.Currency, FormatInfo)) ||
                           (Stock.ToString() != variationExist.Stock && Stock != 0) ||
                           (MinimumStock.ToString() != variationExist.MinimumStock && MinimumStock != 0) ||
                           (Weight.ToString() != variationExist.Weight && Weight != 0) ||
                           (Length.ToString() != variationExist.Length && Length != 0) ||
                           (Width.ToString() != variationExist.Width && Width != 0) ||
                           (Height.ToString() != variationExist.Height && Height != 0) ||
                           (Reference.ToString() != Reference);

            var typeValue1Update = false;
            var typeValue2Update = false;

            if (variationExist.Sku != null && variationExist.Sku.Count() > 0)
                typeValue1Update = Type1?.Trim().ToUpper() != variationExist.Sku[0]?.Type?.Trim().ToUpper() || Value1?.Trim().ToUpper() != variationExist.Sku[0]?.Value?.Trim().ToUpper();

            if (variationExist.Sku != null && variationExist.Sku.Count() > 1)
                typeValue2Update = Type2?.Trim().ToUpper() != variationExist.Sku[1]?.Type?.Trim().ToUpper() || Value2?.Trim().ToUpper() != variationExist.Sku[1]?.Value?.Trim().ToUpper();


            return variantUpdate || typeValue1Update || typeValue2Update;
        }
    }
}