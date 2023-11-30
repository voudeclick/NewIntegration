using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.SellerCenter.ProductActor
{
    public class ProcessVariationOptionsMessage
    {
        public List<Variations> Variants { get; set; }

        public class Variations
        {
            public string NomeVariacao { get; set; }
            public List<string> Values { get; set; }
        }
    }
}
