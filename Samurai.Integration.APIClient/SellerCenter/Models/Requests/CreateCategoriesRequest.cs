using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class CreateCategoriesRequest
    {
        
        public Guid Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Categoria Pai
        /// </summary>
        public Guid? ParentId { get; set; } = null;

        public List<TranslationDto> Translations { get; set; }
    }
}
