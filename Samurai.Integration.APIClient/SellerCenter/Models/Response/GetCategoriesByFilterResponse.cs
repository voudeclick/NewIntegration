using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class GetCategoriesByFilterResponse
    {
        public List<Values> Value { get; set; }
        public class Values
        {
            public Guid? Id { get; set; }

            public Guid? TenantId { get; set; }

            public Guid? ParentId { get; set; }

            public string Name { get; set; }

            public List<TranslationDto> Translations { get; set; }

        }
        public Values ParentCategories(string name) => Value.Where(x => x.ParentId is null && x.Name.ToLower()==name.ToLower()).FirstOrDefault();
        public Values ChildCategories(string name, Guid? parentId) => Value.Where(x => x.Name.ToLower() == name.ToLower() && x.ParentId == parentId).FirstOrDefault();

    }
}
