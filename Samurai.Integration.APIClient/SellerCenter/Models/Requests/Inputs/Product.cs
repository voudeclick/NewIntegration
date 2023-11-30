using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs
{
    public class Product
    {
        public Product()
        {
            Translations = new List<TranslationDto>();
            Variations = new List<Variation>();
            Categories = new List<Category>();


        }
        // Aggregation properties
        public Guid? Id { get; set; }

        /// <summary>
        /// Codigo do produto. Valor aleatorio 
        /// </summary>
        public string ClientCode { get; set; }

        public Guid? ManufacturerId { get; set; }

        public string SellerId { get; set; }

        public bool IsDigital { get; set; }

        public bool HasVariations { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Diameter { get; set; }

        public int ApprovalStatus { get; set; }

        public List<TranslationDto> Translations { get; set; }

        public List<Variation> Variations { get; set; }

        public List<Category> Categories { get; set; }

        public List<Image> Images { get; set; }
    }
}

