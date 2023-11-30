using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models
{
    public class SellerApiAdresses
    {
        /// <summary>
        /// Lista de URLs da API SellerCenter
        /// </summary>
        public string ApiProducts { get; set; }
        public string ApiOrders { get; set; }
        public string ApiControlAccess { get; set; }
        public string ApiSellers { get; set; }
    }
}
