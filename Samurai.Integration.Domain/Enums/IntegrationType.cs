using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IntegrationType
    {
        Shopify = 1,
        SellerCenter = 2,
        Tray = 3
    }
}
