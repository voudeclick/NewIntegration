using System.Text.Json.Serialization;

namespace VDC.Integration.Domain.Enums.Millennium
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransIdType
    {
        ListaVitrine = 1,
        PrecoDeTabela = 2,
        SaldoDeEstoque = 3,
        ListaPedidos = 4,
        SaldoDeEstoqueMto = 5

    }
}
