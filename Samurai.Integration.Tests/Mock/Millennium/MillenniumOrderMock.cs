using Newtonsoft.Json;
using Samurai.Integration.Domain.Messages.Shopify;

namespace Samurai.Integration.Tests.Mock.Millennium
{
    public class MillenniumOrderMock
    {
        public MillenniumOrderMock()
        {
            var jsonOrder = $@"  {{
                                     ""ID"": 4726826565856,
                                     ""ExternalID"": ""HI27952"",
                                     ""Name"": ""#27952"",
                                     ""Number"": 27952,
                                     ""Approved"": false,
                                     ""Shipped"": false,
                                     ""TrackingNumber"": null,
                                     ""Delivered"": false,
                                     ""Cancelled"": false,
                                     ""CreatedAt"": ""2022-04-13T09:10:48-03:00"",
                                     ""DaysToDelivery"": 8,
                                     ""Subtotal"": 187.90,
                                     ""Total"": 210.07,
                                     ""DiscountsValues"": 0.0,
                                     ""InterestValue"": 0.00,
                                     ""TaxValue"": 0.00,
                                     ""ShippingValue"": 22.17,
                                     ""CarrierName"": ""Correios Sedex"",
                                     ""IsPickup"": false,
                                     ""Checkout_Token"": ""982232f3164f67c11ce43836dae5364a"",
                                     ""PickupAdditionalData"": null,
                                     ""Note"": null,
                                     ""vendor"": """",
                                     ""Items"": [
                                       {{
                                         ""LocationId"": null,
                                         ""Id"": 12106664640736,
                                         ""Sku"": ""194_8_0_41"",
                                         ""Name"": ""BOTA ADVENTURE MASCULINA SANDRO MOSCOLONI OUTCROSS MARROM - COFFEE / 41"",
                                         ""Quantity"": 1,
                                         ""Price"": 187.90,
                                         ""DiscountValue"": 0.0,
                                         ""ProductGift"": false
                                       }}
                                     ],
                                     ""Customer"": {{
                                       ""FirstName"": ""Ademir"",
                                       ""LastName"": ""Agostinho"",
                                       ""DDD"": ""11"",
                                       ""Phone"": ""987918086"",
                                       ""Email"": ""ademir.agostinho@yahoo.com.br"",
                                       ""Note"": null,
                                       ""Company"": ""142.473.178-06"",
                                       ""BirthDate"": null,
                                       ""BillingAddress"": {{
                                         ""Address"": ""Rua Forte de Araxá"",
                                         ""Number"": ""105"",
                                         ""Complement"": ""EMPRESA"",
                                         ""District"": ""PARQUE SÃO LOURENÇO"",
                                         ""ZipCode"": ""08340170"",
                                         ""City"": ""SÃO PAULO"",
                                         ""State"": ""SP"",
                                         ""Contact"": ""Ademir Agostinho"",
                                         ""DDD"": ""11"",
                                         ""Phone"": ""98791-8086"",
                                         ""Country"": ""Brazil"",
                                         ""CountryCode"": ""BR""
                                       }},
                                       ""DeliveryAddress"": {{
                                         ""Address"": ""Rua Forte de Araxá"",
                                         ""Number"": ""105"",
                                         ""Complement"": ""EMPRESA"",
                                         ""District"": ""PARQUE SÃO LOURENÇO"",
                                         ""ZipCode"": ""08340170"",
                                         ""City"": ""SÃO PAULO"",
                                         ""State"": ""SP"",
                                         ""Contact"": ""Ademir Agostinho"",
                                         ""DDD"": ""11"",
                                         ""Phone"": ""98791-8086"",
                                         ""Country"": ""Brazil"",
                                         ""CountryCode"": ""BR""
                                       }}
                                     }},
                                     ""PaymentData"": {{
                                       ""Issuer"": null,
                                       ""InstallmentQuantity"": 1,
                                       ""InstallmentValue"": 0.0,
                                       ""PaymentType"": ""boleto""
                                     }},
                                     ""NoteAttributes"": [
                                       {{
                                         ""Name"": ""aditional_info_extra_billing_cpfcnpj"",
                                         ""Value"": ""142.473.178-06""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_billing_endereco"",
                                         ""Value"": ""Rua Forte de Araxá""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_billing_address_number"",
                                         ""Value"": ""105""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_billing_address_complemento"",
                                         ""Value"": ""Empresa""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_billing_address_bairro"",
                                         ""Value"": ""Parque São Lourenço""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_billing_address_cidade"",
                                         ""Value"": ""São Paulo""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_cpfcnpj"",
                                         ""Value"": ""142.473.178-06""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_endereco"",
                                         ""Value"": ""Rua Forte de Araxá""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_numero"",
                                         ""Value"": ""105""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_complemento"",
                                         ""Value"": ""Empresa""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_bairro"",
                                         ""Value"": ""Parque São Lourenço""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_cidade"",
                                         ""Value"": ""São Paulo""
                                       }},
                                       {{
                                         ""Name"": ""shipping_payment_type"",
                                         ""Value"": ""boleto""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_entrega_nome"",
                                         ""Value"": ""Correios Sedex""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_entrega_prazo"",
                                         ""Value"": ""8 dias úteis""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_entrega_valor"",
                                         ""Value"": ""R$ 22,17""
                                       }},
                                       {{
                                         ""Name"": ""aditional_info_extra_installment_count"",
                                         ""Value"": ""1""
                                       }}
                                     ],
                                     ""DeliveryCount"": 0,
                                     ""DisableCustomerDocument"": false,
                                     ""VitrineId"": 0,
                                     ""AdjustmentValue"": 0.00,
                                     ""OperatorType"": 0,
                                     ""ShopifyListOrderProcessId"": null
                                }}";

            order = JsonConvert.DeserializeObject<ShopifySendOrderToERPMessage>(jsonOrder);
        }

        public ShopifySendOrderToERPMessage order { get; set; }
    }
}
