using Samurai.Integration.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Models
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
            public DataSellerCenter DataSellerCenter { get; set; }

            [JsonIgnore]
            public SkuSellerCenter GetInfo => DataSellerCenter?.Variants?.FirstOrDefault();
            public string LocationId { get; set; }
            public bool HasMultiLocation { get; set; }


        }

        public class DataSellerCenter
        {
            public DataSellerCenter()
            {
                SummarySeller = new SummarySellerCenter();
                Variants = new List<SkuSellerCenter>();

            }
            public SummarySellerCenter SummarySeller { get; set; }

            public List<SkuSellerCenter> Variants { get; set; }
        }

        public class SkuSellerCenter
        {
            public SkuSellerCenter()
            {
                Options = new List<DetailsVariations>();
            }

            public string Sku { get; set; }
            public string Barcode { get; set; }
            public bool IsDigital { get; set; }
            /// <summary>
            /// Peso
            /// </summary>
            public decimal? Weight { get; set; }
            /// <summary>
            /// Altura
            /// </summary>
            public decimal? Height { get; set; }
            /// <summary>
            /// Comprimento
            /// </summary>
            public decimal? Length { get; set; }
            public decimal? Width { get; set; }

            /// <summary>
            /// Diametro
            /// </summary>
            public decimal? Diameter => Height + Length + Width;
            public List<InfoVariations> InfoVariations { get; set; }
            public List<DetailsVariations> Options { get; set; }

            public SkuPrice SkuPrice { get; set; }
            public SkuStock SkuStock { get; set; }
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

        public class SummarySellerCenter
        {
            public SummarySellerCenter()
            {
                Variants = new List<ListVariations>();
                Categories = new List<Category>();
                Manufacturers = new List<string>();
            }

            public SummarySellerCenter(List<Info> products)
            {
                _products = products;
                Variants = GetVariants;
                Categories = RemoveDuplicatesCategories(GetCategories);
                Manufacturers = GetManufacturers;

                //summarySeller.Variants = summarySeller.Variants.DistinctVariations();
            }
            private List<Info> _products;
            private List<Category> GetCategories => _products.SelectMany(x => x.Categories).ToList();
            private List<ListVariations> GetVariants => _products.SelectMany(x => x.DataSellerCenter.Variants).ToList().DistinctVariations();
            private List<string> GetManufacturers => _products.Select(x => x.Vendor).Distinct().ToList();

            public List<ListVariations> Variants { get; set; }
            public List<Category> Categories { get; set; }
            public List<string> Manufacturers { get; set; }

            private List<Category> RemoveDuplicatesCategories(List<Category> categories)
            {
                if (categories.Count <= 0) return categories;

                var auxCategories = new List<Category>();

                foreach (var categoryGroups in categories.GroupBy(x => x.Name))
                {
                    var childCategories = categoryGroups.SelectMany(x => x.ChildCategories).ToList();

                    var item = new Category { Name = categoryGroups.Key, ChildCategories = RemoveDuplicatesCategories(childCategories) };

                    auxCategories.Add(item);
                }

                return auxCategories;

            }
        }
    }
}
