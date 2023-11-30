using Newtonsoft.Json;
using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class UpdateVariationOptionRequest
    {
        public UpdateVariationOptionRequest()
        {
            Translations = new List<TranslationDto>();
            AvailableValues = new List<AvailableValues>();
        }

        [JsonIgnore]
        public string Id { get; set; }

        public string Name { get; set; }
        public List<TranslationDto> Translations { get; set; }

        public List<AvailableValues> AvailableValues { get; set; }

    }
    
}
