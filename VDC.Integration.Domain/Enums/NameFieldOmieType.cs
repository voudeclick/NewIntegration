using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NameFieldOmieType
    {
        descricao = 1,
        descricao_familia = 2
    }
}
