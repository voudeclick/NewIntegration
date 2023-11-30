using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class CreateManufacturersRequest
    {
        public CreateManufacturersRequest()
        {
            Translations = new List<TranslationDto>();
        }
        public string Name { get; set; }
        public List<TranslationDto> Translations { get; set; }
    }
}
