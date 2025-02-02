using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class VariantUpdateVariantsInventoryItem
    {
        public String sku {  get; set; }
        public VariantUpdateVariantsInventoryItemMeasurement measurement { get; set; }

    }
}
