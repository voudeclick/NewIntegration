using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BodyIntegrationType
    {
        Always = 0,
        Creation = 1,
        Never = 2
    }
}
