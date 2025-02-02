using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class VariantUpdateVariantsInput
    {

        public String id {  get; set; }
        public String barcode { get; set; }
        public decimal? compareAtPrice { get; set; }
        public decimal? price { get; set; }
        public VariantUpdateVariantsInventoryItem inventoryItem { get; set; }
        public List<VariantUpdateVariantsOptionValue> optionValues { get; set; }

    }
}
