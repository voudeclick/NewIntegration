using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class GetSellerResponse
    {
        public Values Value { get; set; }
        public class Values
        {
            public int prefixId { get; set; }
            public string Name { get; set; }
        }
    }   
}
