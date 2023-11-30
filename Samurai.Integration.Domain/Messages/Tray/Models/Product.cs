using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Samurai.Integration.Domain.Messages.Tray.Models.Product.Info;

namespace Samurai.Integration.Domain.Messages.Tray.Models
{
    public class Product
    {
        public class Info
        {
            public long Id { get; set; }
            public Guid AppTrayProductId { get; set; }
            public string Ean { get; set; }
            public string Name { get; set; }
            //public string Ncm { get; set; }
            public string Description { get; set; }
            public string DescriptionSmall { get; set; }
            public double? Price { get; set; }
            public double? CostPrice { get; set; }
            //public double PromotionalPrice { get; set; }
            //public DateTime? StartPromotion { get; set; }
            //public DateTime? EndPromotion { get; set; }
            public int? IpiValue { get; set; }
            public Manufacture Brand { get; set; }
            public string Model { get; set; }
            public long? Weight { get; set; }
            public long? Length { get; set; }
            public long? Width { get; set; }
            public long? Height { get; set; }
            public long? Stock { get; set; }
            public Category Category { get; set; }
            public int? Available { get; set; }
            public string Availability { get; set; }
            public int? AvailabilityDays { get; set; }
            public string Reference { get; set; }
            public List<int> RelatedCategories { get; set; }
            //public string ReleaseDate { get; set; }
            public string PictureSource1 { get; set; }
            public string PictureSource2 { get; set; }
            public string PictureSource3 { get; set; }
            public string PictureSource4 { get; set; }
            public string PictureSource5 { get; set; }
            public string PictureSource6 { get; set; }
            public string VirtualProduct { get; set; }

            public List<MetatagModel> Metatag { get; set; }
            public class MetatagModel
            {
                public string Type { get; set; }
                public string Content { get; set; }
                public int Local { get; set; }
            }

            public List<Variation> Variations { get; set; }

            public List<Attribute> Attributes { get; set; }

            //public bool Delete { get; set; } = false;
        }

        public class Manufacture
        {
            public string Brand { get; set; }
            public string Slug { get; set; }
        }
        public class Attribute
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class ListAttributes
        {
            public string Name { get; set; }
            public List<string> Values { get; set; }
        }

        public class Category
        {
            public long? Id { get; set; }
            public string Name { get; set; }
        }
        public class Variation
        {
            public long Id { get; set; }
            public Guid AppTrayProductVariationId { get; set; }
            public string Ean { get; set; }
            public int? Order { get; set; }
            public double? Price { get; set; }
            public double? CostPrice { get; set; }
            public long? Stock { get; set; }
            public int? MinimumStock { get; set; }
            public long? Weight { get; set; }
            public long? Length { get; set; }
            public long? Width { get; set; }
            public long? Height { get; set; }
            //public DateTime? StartPromotion { get; set; }
            //public DateTime? EndPromotion { get; set; }
            //public decimal PromotionalPrice { get; set; }
            public string Type1 { get; set; }
            public string Value1 { get; set; }
            public string Type2 { get; set; }
            public string Value2 { get; set; }
            public string PictureSource1 { get; set; }
            public string PictureSource2 { get; set; }
            public string PictureSource3 { get; set; }
            public string PictureSource4 { get; set; }
            public string PictureSource5 { get; set; }
            public string PictureSource6 { get; set; }
            //public int QuantitySold { get; set; }
            public long ProductId { get; set; }
            public string Reference { get; set; }
            public bool Active { get; set; }
        }

        public class SummaryTray
        {
            private List<Info> _products;
            public List<Variation> Variations { get; set; }
            public List<Category> Categories { get; set; }
            public List<Manufacture> Manufacturers { get; set; }
            //public List<ListAttributes> Attributes { get; set; }

            public SummaryTray()
            {
                Categories = new List<Category>();
                Manufacturers = new List<Manufacture>();
                //Attributes = new List<ListAttributes>();
                Variations = new List<Variation>();
            }

            public SummaryTray(List<Info> products)
            {
                _products = products;

                Variations = GetVariants;
                Categories = GetCategories;
                Manufacturers = GetManufacturers;
                //Attributes = GetAttributes;
            }

            private List<Variation> GetVariants => _products.SelectMany(x => x.Variations).ToList();
            private List<Category> GetCategories => RemoveDuplicatesCategories(_products.Select(x => x.Category).ToList());
            private List<Manufacture> GetManufacturers => _products.Select(x => x.Brand).Distinct().ToList();
            //private List<ListAttributes> GetAttributes => RemoveDuplicatesAttributes(_products.SelectMany(x => x.Attributes).ToList());

            private List<Category> RemoveDuplicatesCategories(List<Category> categories)
            {
                return categories.GroupBy(x => new { x.Id, x.Name }).Select(x => x.First()).ToList();

            }

            private List<ListAttributes> RemoveDuplicatesAttributes(List<Attribute> attributes)
            {
                return attributes.GroupBy(x => x.Name).Select(x => new ListAttributes()
                {
                    Name = x.Key,
                    Values = x.Select(y => y.Value).Distinct().ToList()
                }).ToList();
            }

        }
    }
}
