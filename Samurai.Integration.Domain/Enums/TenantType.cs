using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TenantType
    {
        Millennium = 1,
        Nexaas = 2,
        Omie = 3,
        Bling = 4,
        PluggTo = 5,
        AliExpress = 6
    }
}
