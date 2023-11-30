using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.Domain.Models
{
    public class LocationItem
    {
        /// <summary>
        /// Millenium | Omie | Nexaas
        /// </summary>
        public string ErpLocation { get; set; }
        /// <summary>
        /// Shopify | Sellecernter
        /// </summary>
        public string EcommerceLocation { get; set; }
    }
}
