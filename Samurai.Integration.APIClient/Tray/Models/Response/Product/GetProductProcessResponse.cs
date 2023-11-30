using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Product
{
    public class GetProductProcessResponse
    {
        public Guid Id { get; set; }
        public string StoreId { get; set; }
        public long ProductIdAli { get; set; }
        public string ProductName { get; set; }
        public string TrayProductName { get; set; }
        public long? TrayProductId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
