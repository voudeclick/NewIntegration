using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Nexaas.Models.Results
{
    public class NexaasApiListAllProductsResult
    {
        public List<Product> products { get; set; }
        public class Product
        {
            public long id { get; set; }
        }
    }
}
