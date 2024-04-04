using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IntegrationType
    {
        Shopify = 1
    }
}
