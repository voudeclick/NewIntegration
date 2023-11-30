using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class GetVariationByFilterRequest
    {
        public string CultureName { get => "pt-BR"; }
        public string Name { get; set; }
        public int PageSize { get => 10000; }
        public string PageIndex { get => "0"; }
    }
}
