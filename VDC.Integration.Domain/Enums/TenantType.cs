using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TenantType
    {
        Millennium = 1,
        Omie = 3
    }
}
