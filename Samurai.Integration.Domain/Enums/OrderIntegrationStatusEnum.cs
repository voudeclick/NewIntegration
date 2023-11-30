using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderIntegrationStatusEnum
    {
        Aprovado=1,
        AguardandoAprovacao=2,
        Reprovado=3
    }
}
