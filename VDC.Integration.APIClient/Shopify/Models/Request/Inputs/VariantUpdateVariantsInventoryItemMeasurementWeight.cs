using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class VariantUpdateVariantsInventoryItemMeasurementWeight
    {
        public string unit { get; set; }
        public decimal? value { get; set; }
    }
}
