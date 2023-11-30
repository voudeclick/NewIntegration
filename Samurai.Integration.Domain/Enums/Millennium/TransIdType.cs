using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums.Millennium
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
