using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class GetVariationsByIdResponse
    {
        public Values Value { get; set; }
        public class Values
        {

            public Guid? Id { get; set; }

            public Guid? TenantId { get; set; }

            public string Name { get; set; }

            public List<TranslationDto> Translations { get; set; }

            public List<AvailableValues> AvailableValues { get; set; }

            public List<string> GetAvailableValues => AvailableValues.Select(x => x.Value).Distinct().ToList();

            public Guid? GetIdAvailableValuesByName(string name) => AvailableValues.Where(x => x.Value.ToUpper() == name.ToUpper()).Select(x=> x.Id).FirstOrDefault();

        }
    }
}
