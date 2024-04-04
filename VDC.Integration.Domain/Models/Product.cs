using System;
using System.Collections.Generic;

namespace VDC.Integration.Domain.Models
{
    public class Product
    {
        public class Info
        {
            public long? ShopifyId { get; set; }
            public string ExternalId { get; set; }
            public string ExternalCode { get; set; }
            public string SkuOriginal { get; set; }
            public string GroupingReference { get; set; }
            public bool? Status { get; set; }
            public bool? kit { get; set; }
            public string Title { get; set; }
            public string BodyHtml { get; set; }
            public string VendorId { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public ProductImages Images { get; set; }
            public List<SkuInfo> Variants { get; set; }
            public List<Category> Categories { get; set; }
            public List<Metafield> Metafields { get; set; }
            public List<string> OptionsName { get; set; }
            public string LocationId { get; set; }
            public bool HasMultiLocation { get; set; }
        }

        public class DetailsVariations
        {
            /// <summary>
            /// Guid da variacao. Ex: Cor
            /// </summary>
            public Guid? VariationOptionId { get; set; }
            /// <summary>
            /// Guid do valor da variacao. Ex: Azul
            /// </summary>
            public Guid? VariationOptionAvailableValueId { get; set; }
            public string ImageUrl { get; set; }

        }
        public class InfoVariations
        {
            public string NomeVariacao { get; set; }
            public string ValorVariacao { get; set; }
        }
        public class ListVariations
        {
            public string NomeVariacao { get; set; }
            public List<string> Values { get; set; }
        }
        public class SkuInfo
        {
            public long? ShopifyId { get; set; }
            public string Sku { get; set; }
            public string OriginalSku { get; set; }
            public bool Status { get; set; }
            public List<string> OptionsName { get; set; }
            public List<string> Options { get; set; }
            public decimal? WeightInKG { get; set; }
            public string Barcode { get; set; }
            public SkuPrice Price { get; set; }
            public SkuStock Stock { get; set; }
            public List<SkuKit> SkuKits { get; set; } = new List<SkuKit>();
            public bool SellWithoutStock { get; set; } = false;
        }

        public class SkuKit
        {
            public int ParentProduct { get; set; }
            public int ChildProduct { get; set; }
            public int Quantity { get; set; }
            public string Sku { get; set; }
            public string CodProduct { get; set; }
        }

        public class SkuPrice
        {
            public long? ShopifyId { get; set; }
            public string Sku { get; set; }
            public decimal? CompareAtPrice { get; set; }
            public decimal Price { get; set; }
            public SkuStock Stock { get; set; }
        }

        public class SkuStock
        {
            public long? ShopifyId { get; set; }
            public string Sku { get; set; }
            public int Quantity { get; set; }
            public bool DecreaseStock { get; set; } = false;
            public List<Mulilocation> Locations { get; set; } = new List<Mulilocation>();
            public class Mulilocation
            {
                public string ErpLocationId { get; set; }
                public int Quantity { get; set; }
            }
        }

        public class Category
        {
            public Category()
            {
                ChildCategories = new List<Category>();
            }
            public string Id { get; set; }
            public string Name { get; set; }
            public List<Category> ChildCategories { get; set; }
        }

        public class Metafield
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string ValueType { get; set; }
        }
    }
}
