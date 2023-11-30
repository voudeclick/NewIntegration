using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Enums.Millennium
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MillenniumIssuerType
    {
        MASTERCARD = 0,
        VISA = 1,
        SOROCRED = 2,
        AMEX = 3,
        DINERS = 4,
        HIPERCARD = 5,
        ELO = 6,
        BRADESCO = 7,
        ITAUSHOPLINE = 8,
        BANCOBRASIL = 9,
        HSBC = 10,
        OUTROS = 11,
        AMERICAN_EXPRESS = 12,
        DISCOVER = 13,
        BOLETO_BANCARIO = 14,
        AURA = 15,
        VISA_ELECTRON = 16,
        CARTAO_DA_LOJA = 17,
        PAYPAL = 18,
        BCASH = 19,
        BANRICOMPRAS = 20,
        EVOLUCARD = 21,
        VALE = 22,
        PROMISSORY = 23,
        MERCADO_PAGO = 24,
        DEBITO_ONLINE = 25,
        DEBITO_ITAU = 26,
        DEBITO_BRADESCO = 27,
        DEB_BCO_DO_BRASIL = 28,
        DEBITO_BANRISUL = 29,
        LEVPAY = 30,
        BOLETO_SHOPIFY = 31,
        CARTAODECREDITO_SHOPIFY =32,
    }
}
