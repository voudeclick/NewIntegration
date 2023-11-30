﻿using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class CreateCategoryResponse
    {
        public Values Value { get; set; }
        public class Values
        {
            public Guid? Id { get; set; }

            public Guid? TenantId { get; set; }

            public Guid? ParentId { get; set; }

            public string Name { get; set; }

            public List<TranslationDto> Translations { get; set; }

        }
    }
}
