using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Order
{
    public class UpdateOrderRequest
    {
        public long OrderId { get; set; }

        [JsonProperty("Order")]
        public TrayOrderUpdate Order { get; set; }

        [JsonProperty("Order")]
        public TrayOrderCancelUpdate OrderCancel { get; set; }

        [JsonProperty("Order")]
        public TrayOrderStatusUpdate OrderStatus { get; set; }

    }
    public class TrayOrderCancelUpdate
    {

        [JsonProperty("store_note")]
        public string StoreNote { get; set; }
    }

    public class TrayOrderUpdate
    {
        [JsonProperty("sending_code")]
        public string SendingCode { get; set; }

        [JsonProperty("sending_date")]
        public string SendingDate { get; set; }

        [JsonProperty("store_note")]
        public string StoreNote { get; set; }
    }
    public class TrayOrderStatusUpdate
    {
        [JsonProperty("status_id")]
        public long StatusId { get; set; }
    }

}
