using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums.Bling
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderFieldBlingType
    {
        number = 1,
        id_shopify = 2
    }
}
