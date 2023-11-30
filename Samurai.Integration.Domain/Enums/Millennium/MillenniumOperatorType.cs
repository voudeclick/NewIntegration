using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums.Millennium
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MillenniumOperatorType
    {
        CIELO = 0,
        REDE = 1,
        GETNET = 2,
        OUTROS = 3,
        RED_CARD = 4
    }
}
