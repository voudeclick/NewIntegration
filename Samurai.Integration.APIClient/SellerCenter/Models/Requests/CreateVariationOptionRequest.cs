using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class CreateVariationOptionRequest
    {
        /// <summary>
        /// Nome da variacao
        /// </summary>
        public string Name { get; set; }
        public List<TranslationDto> Translations { get; set; }

        public List<AvailableValues> AvailableValues { get; set; }

        public CreateVariationOptionRequest()
        {
            Translations = new List<TranslationDto>();
            AvailableValues = new List<AvailableValues>();
        }
    }
}
