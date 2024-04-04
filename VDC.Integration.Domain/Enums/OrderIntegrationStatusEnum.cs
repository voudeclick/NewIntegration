using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderIntegrationStatusEnum
    {
        Aprovado = 1,
        AguardandoAprovacao = 2,
        Reprovado = 3
    }
}
