using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Nexaas.Models.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NexaasWebhookType
    {
        StockSku = 0,
        Stock = 1, 
        Manufacturer = 2,
        ProductBrand = 3,
        ProductSku = 4,
        ProductCategory = 5,
        Order  = 6,
        Transfer = 7
    }
}
