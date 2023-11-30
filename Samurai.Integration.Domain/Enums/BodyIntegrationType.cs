using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BodyIntegrationType
    {
        Always = 0,
        Creation = 1,
        Never = 2
    }
}
