using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums.Millennium
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SkuFieldType
    {
        sku = 1,
        cod_produto = 2,
        id_externo = 3,
    }
}
