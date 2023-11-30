using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Nexaas
{
    public class NexaasStockSku
    {
        public long id { get; set; }
        public long stock_id { get; set; }
        public long product_sku_id { get; set; }
        public int in_stock_quantity { get; set; }
        public int? in_transit_quantity { get; set; }
        public int? reserved_quantity { get; set; }
        public int available_quantity { get; set; }
        public bool negative_stock { get; set; }
        public int? minimum_limit { get; set; }
        public string batch { get; set; }
        public DateTime? batch_expiration_date { get; set; }
        public decimal replacement_cost { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public NexaasSku product_sku { get; set; }
        public NexaasStock stock { get; set; }
    }

    public class NexaasStock
    {
        public long id { get; set; }
        public long organization_id { get; set; }
        public string name { get; set; }
        public string document { get; set; }
        public bool active { get; set; }
        public string zip_code { get; set; }
        public int emites_id { get; set; }
        public string serie_nfe { get; set; }
        public string street { get; set; }
        public string number { get; set; }
        public string complement { get; set; }
        public string city { get; set; }
        public string neighborhood { get; set; }
        public string state { get; set; }
        public DateTime created_at { get; set; }
        public string inscricao_estadual { get; set; }
        public List<NexaasSaleChannel> sale_channels { get; set; }
    }
}