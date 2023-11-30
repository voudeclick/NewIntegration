using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Attribute
{
    public class GetAttributeByFilterResponse : BaseResponse
    {
        public List<Property> Properties { get; set; }
    }

    public class Property
    {
        public int Id { get; set; }

        [JsonProperty("active_display")]
        public int ActiveDisplay { get; set; }

        public string Name { get; set; }
        public int Position { get; set; }
        public int Display { get; set; }

        [JsonProperty("has_product")]
        public int HasProduct { get; set; }

        public List<PropertyValue> PropertyValues { get; set; }
    }

    public class PropertyValue
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("property_id")]
        public int PropertyId { get; set; }
    }

}
