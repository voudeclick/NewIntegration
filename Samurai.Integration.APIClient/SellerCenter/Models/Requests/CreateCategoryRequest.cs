using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class CreateCategoryRequest
    {
        public CreateCategoryRequest()
        {
            Translations = new List<TranslationDto>();
        }
        public string Name { get; set; }
        public Guid? ParentId { get; set; }

        public List<TranslationDto> Translations { get; set; }
    }
}
