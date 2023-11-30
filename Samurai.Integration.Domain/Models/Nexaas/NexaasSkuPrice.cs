using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Models.Nexaas
{
    public class NexaasSkuPrice
    {
        public long id { get; set; }
        public decimal? price { get; set; }
        public decimal? discount { get; set; }
        public decimal? sale_price { get; set; }
        public long product_sku_id { get; set; }
        public long price_table_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public NexaasPriceTable price_table { get; set; }
    }

    public class NexaasPriceTable
    {
        public long id { get; set; }
        public string name { get; set; }
        public DateTime? start_on { get; set; }
        public DateTime? end_on { get; set; }
        public bool active { get; set; }
        public long? organization_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public List<NexaasSaleChannel> sale_channels { get; set; }
    }

    public class NexaasSaleChannel
    {
        public long id { get; set; }
        public string name { get; set; }
        public bool pickup_point { get; set; }
        public long? organization_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public long account_id { get; set; }
    }
}
