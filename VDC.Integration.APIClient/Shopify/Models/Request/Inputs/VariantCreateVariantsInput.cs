using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class VariantCreateVariantsInput
    {
        public String barcode { get; set; }
        public decimal? price { get; set; }
        public VariantUpdateVariantsInventoryItem inventoryItem { get; set; }
        public List<VariantUpdateVariantsOptionValue> optionValues { get; set; }

    }
}
