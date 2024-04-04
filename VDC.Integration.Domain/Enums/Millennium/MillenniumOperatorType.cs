using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums.Millennium
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
