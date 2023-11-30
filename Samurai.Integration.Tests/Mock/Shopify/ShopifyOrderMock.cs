using Newtonsoft.Json;
using Samurai.Integration.Domain.Shopify.Models.Results.REST;

namespace Samurai.Integration.Tests.Mock.Shopify
{
    public class ShopifyOrderMock
    {
        public ShopifyOrderMock()
        {
            var jsonOrder = $@"{{
  ""order"": {{
    ""id"": 4937459302562,
    ""admin_graphql_api_id"": ""gid://shopify/Order/4937459302562"",
    ""app_id"": 580111,
    ""browser_ip"": ""186.226.156.130"",
    ""buyer_accepts_marketing"": true,
    ""cancel_reason"": ""customer"",
    ""cancelled_at"": ""2022-03-30T15:26:41-03:00"",
    ""cart_token"": null,
    ""checkout_id"": 22155123589282,
    ""checkout_token"": ""d280623f5144be5b6688356952b11a00"",
    ""client_details"": {{
      ""accept_language"": ""pt-BR,pt;q=0.9"",
      ""browser_height"": 722,
      ""browser_ip"": ""186.226.156.130"",
      ""browser_width"": 1519,
      ""session_hash"": null,
      ""user_agent"": ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36""
    }},
    ""closed_at"": null,
    ""confirmed"": true,
    ""contact_email"": ""elisabethmbmeyer@gmail.com"",
    ""created_at"": ""2022-03-30T10:23:23-03:00"",
    ""currency"": ""BRL"",
    ""current_subtotal_price"": 0.00,
    ""current_subtotal_price_set"": {{
      ""shop_money"": {{
        ""amount"": 0.00,
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""current_total_discounts"": ""0.00"",
    ""current_total_discounts_set"": {{
      ""shop_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""current_total_duties_set"": null,
    ""current_total_price"": 0.00,
    ""current_total_price_set"": {{
      ""shop_money"": {{
        ""amount"": 0.00,
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""current_total_tax"": ""0.00"",
    ""current_total_tax_set"": {{
      ""shop_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""customer_locale"": ""pt-BR"",
    ""device_id"": null,
    ""discount_codes"": [
      {{
        ""code"": ""desc-20220330132308a1d3"",
        ""amount"": ""8.50"",
        ""type"": ""fixed_amount""
      }}
    ],
    ""email"": ""elisabethmbmeyer@gmail.com"",
    ""estimated_taxes"": false,
    ""financial_status"": ""refunded"",
    ""fulfillment_status"": null,
    ""gateway"": ""manual"",
    ""landing_site"": ""/"",
    ""landing_site_ref"": null,
    ""location_id"": null,
    ""name"": ""#1536"",
    ""note"": null,
    ""note_attributes"": [
      {{
        ""name"": ""aditional_info_extra_billing_address_number"",
        ""value"": ""372""
      }},
      {{
        ""name"": ""aditional_info_extra_billing_address_bairro"",
        ""value"": ""Ceramarte""
      }},
      {{
        ""name"": ""aditional_info_extra_billing_address_complemento"",
        ""value"": ""apto 304""
      }},
      {{
        ""name"": ""aditional_info_extra_billing_address_cidade"",
        ""value"": ""Santa Catarina""
      }},
      {{
        ""name"": ""shipping_payment_type"",
        ""value"": ""Boleto""
      }},
      {{
        ""name"": ""aditional_info_extra_entrega_prazo"",
        ""value"": ""-""
      }},
      {{
        ""name"": ""shipping_shipping_method_name"",
        ""value"": ""CORREIOS - ENTREGA EM ATÉ 20 DIAS ÚTEIS""
      }},
      {{
        ""name"": ""shipping_shipping_method_price"",
        ""value"": ""25.00""
      }},
      {{
        ""name"": ""aditional_info_extra_endereco"",
        ""value"": ""Rodolfo Tureck""
      }},
      {{
        ""name"": ""aditional_info_extra_billing_endereco"",
        ""value"": ""Rodolfo Tureck""
      }},
      {{
        ""name"": ""aditional_info_extra_numero"",
        ""value"": ""372""
      }},
      {{
        ""name"": ""aditional_info_extra_bairro"",
        ""value"": ""Ceramarte""
      }},
      {{
        ""name"": ""aditional_info_extra_complemento"",
        ""value"": ""apto 304""
      }},
      {{
        ""name"": ""aditional_info_extra_cidade"",
        ""value"": ""Santa Catarina""
      }},
      {{
        ""name"": ""aditional_info_extra_billing_cpfcnpj"",
        ""value"": ""035.543.609-45""
      }},
      {{
        ""name"": ""aditional_info_extra_device"",
        ""value"": ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36""
      }},
      {{
        ""name"": ""aditional_info_brands"",
        ""value"": ""Boleto""
      }},
      {{
        ""name"": ""aditional_info_extra_address_state"",
        ""value"": ""SC""
      }},
      {{
        ""name"": ""aditional_info_extra_billing_address_state"",
        ""value"": ""SC""
      }}
    ],
    ""number"": 536,
    ""order_number"": 1536,
    ""order_status_url"": ""https://www.oppa.com.br/49452777634/orders/068801b58f95ffe6544dc528e36de9a2/authenticate?key=1faa4f03863d9a1797e522555f33c0f1"",
    ""original_total_duties_set"": null,
    ""payment_gateway_names"": [
      ""checkout_moip"",
      ""manual""
    ],
    ""phone"": null,
    ""presentment_currency"": ""BRL"",
    ""processed_at"": ""2022-03-30T10:23:22-03:00"",
    ""processing_method"": ""offsite"",
    ""reference"": null,
    ""referring_site"": """",
    ""source_identifier"": null,
    ""source_name"": ""web"",
    ""source_url"": null,
    ""subtotal_price"": 161.55,
    ""subtotal_price_set"": {{
      ""shop_money"": {{
        ""amount"": 161.55,
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""161.55"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""tags"": ""ExtId-1536-Intg, IntgSt-Cancelled-Intg, IsIntg-True-Intg, Num-1536-Intg"",
    ""tax_lines"": [],
    ""taxes_included"": false,
    ""test"": false,
    ""token"": ""068801b58f95ffe6544dc528e36de9a2"",
    ""total_discounts"": ""8.50"",
    ""total_discounts_set"": {{
      ""shop_money"": {{
        ""amount"": ""8.50"",
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""8.50"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""total_line_items_price"": 170.05,
    ""total_line_items_price_set"": {{
      ""shop_money"": {{
        ""amount"": 170.05,
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""170.05"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""total_outstanding"": ""0.00"",
    ""total_price"": 186.55,
    ""total_price_set"": {{
      ""shop_money"": {{
        ""amount"": 186.55,
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""186.55"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""total_price_usd"": 39.20,
    ""total_shipping_price_set"": {{
      ""shop_money"": {{
        ""amount"": 25.00,
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""25.00"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""total_tax"": ""0.00"",
    ""total_tax_set"": {{
      ""shop_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }},
      ""presentment_money"": {{
        ""amount"": ""0.00"",
        ""currency_code"": ""BRL""
      }}
    }},
    ""total_tip_received"": ""0.00"",
    ""total_weight"": 2560,
    ""updated_at"": ""2022-03-30T15:26:48-03:00"",
    ""user_id"": null,
    ""billing_address"": {{
      ""first_name"": ""Elisabeth"",
      ""address1"": ""Rodolfo Tureck,372,Ceramarte"",
      ""phone"": ""47 99927-4221"",
      ""city"": ""Santa Catarina"",
      ""zip"": ""89295-000"",
      ""province"": ""Santa Catarina"",
      ""country"": ""Brazil"",
      ""last_name"": ""Meyer"",
      ""address2"": ""apto 304"",
      ""company"": ""035.543.609-45"",
      ""latitude"": -26.258635,
      ""longitude"": -49.528886,
      ""name"": ""Elisabeth Meyer"",
      ""country_code"": ""BR"",
      ""province_code"": ""SC""
    }},
    ""customer"": {{
      ""id"": 6029302530210,
      ""email"": ""elisabethmbmeyer@gmail.com"",
      ""accepts_marketing"": true,
      ""created_at"": ""2022-02-01T09:22:57-03:00"",
      ""updated_at"": ""2022-04-07T11:28:11-03:00"",
      ""first_name"": ""ELISABETH MARIA"",
      ""last_name"": ""MEYER"",
      ""orders_count"": 17,
      ""state"": ""enabled"",
      ""total_spent"": ""4199.90"",
      ""last_order_id"": 4939815092386,
      ""note"": null,
      ""verified_email"": false,
      ""multipass_identifier"": null,
      ""tax_exempt"": false,
      ""phone"": null,
      ""tags"": """",
      ""last_order_name"": ""#1548"",
      ""currency"": ""BRL"",
      ""accepts_marketing_updated_at"": ""2022-02-03T13:53:37-03:00"",
      ""marketing_opt_in_level"": ""single_opt_in"",
      ""admin_graphql_api_id"": ""gid://shopify/Customer/6029302530210"",
      ""default_address"": {{
        ""id"": 7397203181730,
        ""customer_id"": 6029302530210,
        ""first_name"": ""Elisabeth"",
        ""last_name"": ""Meyer"",
        ""company"": ""035.543.609-45"",
        ""address1"": ""Rodolfo Tureck,372,Ceramarte"",
        ""address2"": ""apto 304"",
        ""city"": ""Santa Catarina"",
        ""province"": ""Santa Catarina"",
        ""country"": ""Brazil"",
        ""zip"": ""89295-000"",
        ""phone"": ""47 99927-4221"",
        ""name"": ""Elisabeth Meyer"",
        ""province_code"": ""SC"",
        ""country_code"": ""BR"",
        ""country_name"": ""Brazil"",
        ""default"": true
      }}
    }},
    ""discount_applications"": [
      {{
        ""target_type"": ""line_item"",
        ""type"": ""discount_code"",
        ""value"": ""8.5"",
        ""value_type"": ""fixed_amount"",
        ""allocation_method"": ""across"",
        ""target_selection"": ""all"",
        ""code"": ""desc-20220330132308a1d3""
      }}
    ],
    ""fulfillments"": [
      {{
        ""id"": 4408193515682,
        ""admin_graphql_api_id"": ""gid://shopify/Fulfillment/4408193515682"",
        ""created_at"": ""2022-03-30T10:31:40-03:00"",
        ""location_id"": 56043274402,
        ""name"": ""#1536.1"",
        ""order_id"": 4937459302562,
        ""receipt"": {{}},
        ""service"": ""manual"",
        ""shipment_status"": null,
        ""status"": ""cancelled"",
        ""tracking_company"": ""Other"",
        ""tracking_number"": null,
        ""tracking_numbers"": [],
        ""tracking_url"": null,
        ""tracking_urls"": [],
        ""updated_at"": ""2022-03-30T15:25:17-03:00"",
        ""line_items"": [
          {{
            ""id"": 11703803445410,
            ""admin_graphql_api_id"": ""gid://shopify/LineItem/11703803445410"",
            ""destination_location"": {{
              ""id"": 3447555129506,
              ""country_code"": ""BR"",
              ""province_code"": ""SC"",
              ""name"": ""Elisabeth Meyer"",
              ""address1"": ""Rodolfo Tureck,372,Ceramarte"",
              ""address2"": ""apto 304"",
              ""city"": ""Santa Catarina"",
              ""zip"": ""89295-000""
            }},
            ""fulfillable_quantity"": 0,
            ""fulfillment_service"": ""manual"",
            ""fulfillment_status"": null,
            ""gift_card"": false,
            ""grams"": 2560,
            ""name"": ""Balanço de Madeira Tão Perto, Tão Longe - Corda Preta - Corda Preta"",
            ""origin_location"": {{
              ""id"": 3389326524578,
              ""country_code"": ""BR"",
              ""province_code"": ""SC"",
              ""name"": ""oppa.com.br"",
              ""address1"": ""Rodovia BR 280 - km 123, 2866 - industrial sul"",
              ""address2"": """",
              ""city"": ""Rio Negrinho"",
              ""zip"": ""89295000""
            }},
            ""price"": 170.05,
            ""price_set"": {{
              ""shop_money"": {{
                ""amount"": 170.05,
                ""currency_code"": ""BRL""
              }},
              ""presentment_money"": {{
                ""amount"": ""170.05"",
                ""currency_code"": ""BRL""
              }}
            }},
            ""product_exists"": true,
            ""product_id"": 6984747122850,
            ""properties"": [],
            ""quantity"": 1,
            ""requires_shipping"": true,
            ""sku"": ""143000222"",
            ""taxable"": true,
            ""title"": ""Balanço de Madeira Tão Perto, Tão Longe - Corda Preta"",
            ""total_discount"": ""0.00"",
            ""total_discount_set"": {{
              ""shop_money"": {{
                ""amount"": ""0.00"",
                ""currency_code"": ""BRL""
              }},
              ""presentment_money"": {{
                ""amount"": ""0.00"",
                ""currency_code"": ""BRL""
              }}
            }},
            ""variant_id"": 40553906798754,
            ""variant_inventory_management"": ""shopify"",
            ""variant_title"": ""Corda Preta"",
            ""vendor"": ""Fechada"",
            ""tax_lines"": [],
            ""duties"": [],
            ""discount_allocations"": [
              {{
                ""amount"": ""8.50"",
                ""amount_set"": {{
                  ""shop_money"": {{
                    ""amount"": ""8.50"",
                    ""currency_code"": ""BRL""
                  }},
                  ""presentment_money"": {{
                    ""amount"": ""8.50"",
                    ""currency_code"": ""BRL""
                  }}
                }},
                ""discount_application_index"": 0
              }}
            ]
          }}
        ]
      }}
    ],
    ""line_items"": [
      {{
        ""id"": 11703803445410,
        ""admin_graphql_api_id"": ""gid://shopify/LineItem/11703803445410"",
        ""destination_location"": {{
          ""id"": 3447555129506,
          ""country_code"": ""BR"",
          ""province_code"": ""SC"",
          ""name"": ""Elisabeth Meyer"",
          ""address1"": ""Rodolfo Tureck,372,Ceramarte"",
          ""address2"": ""apto 304"",
          ""city"": ""Santa Catarina"",
          ""zip"": ""89295-000""
        }},
        ""fulfillable_quantity"": 0,
        ""fulfillment_service"": ""manual"",
        ""fulfillment_status"": null,
        ""gift_card"": false,
        ""grams"": 2560,
        ""name"": ""Balanço de Madeira Tão Perto, Tão Longe - Corda Preta - Corda Preta"",
        ""origin_location"": {{
          ""id"": 3389326524578,
          ""country_code"": ""BR"",
          ""province_code"": ""SC"",
          ""name"": ""oppa.com.br"",
          ""address1"": ""Rodovia BR 280 - km 123, 2866 - industrial sul"",
          ""address2"": """",
          ""city"": ""Rio Negrinho"",
          ""zip"": ""89295000""
        }},
        ""price"": 170.05,
        ""price_set"": {{
          ""shop_money"": {{
            ""amount"": 170.05,
            ""currency_code"": ""BRL""
          }},
          ""presentment_money"": {{
            ""amount"": ""170.05"",
            ""currency_code"": ""BRL""
          }}
        }},
        ""product_exists"": true,
        ""product_id"": 6984747122850,
        ""properties"": [],
        ""quantity"": 1,
        ""requires_shipping"": true,
        ""sku"": ""143000222"",
        ""taxable"": true,
        ""title"": ""Balanço de Madeira Tão Perto, Tão Longe - Corda Preta"",
        ""total_discount"": ""0.00"",
        ""total_discount_set"": {{
          ""shop_money"": {{
            ""amount"": ""0.00"",
            ""currency_code"": ""BRL""
          }},
          ""presentment_money"": {{
            ""amount"": ""0.00"",
            ""currency_code"": ""BRL""
          }}
        }},
        ""variant_id"": 40553906798754,
        ""variant_inventory_management"": ""shopify"",
        ""variant_title"": ""Corda Preta"",
        ""vendor"": ""Fechada"",
        ""tax_lines"": [],
        ""duties"": [],
        ""discount_allocations"": [
          {{
            ""amount"": ""8.50"",
            ""amount_set"": {{
              ""shop_money"": {{
                ""amount"": ""8.50"",
                ""currency_code"": ""BRL""
              }},
              ""presentment_money"": {{
                ""amount"": ""8.50"",
                ""currency_code"": ""BRL""
              }}
            }},
            ""discount_application_index"": 0
          }}
        ]
      }}
    ],
    ""refunds"": [
      {{
        ""id"": 836712399010,
        ""admin_graphql_api_id"": ""gid://shopify/Refund/836712399010"",
        ""created_at"": ""2022-03-30T15:26:39-03:00"",
        ""note"": null,
        ""order_id"": 4937459302562,
        ""processed_at"": ""2022-03-30T15:26:39-03:00"",
        ""restock"": true,
        ""total_duties_set"": {{
          ""shop_money"": {{
            ""amount"": ""0.00"",
            ""currency_code"": ""BRL""
          }},
          ""presentment_money"": {{
            ""amount"": ""0.00"",
            ""currency_code"": ""BRL""
          }}
        }},
        ""user_id"": 77242957986,
        ""order_adjustments"": [
          {{
            ""id"": 196056121506,
            ""amount"": ""-25.00"",
            ""amount_set"": {{
              ""shop_money"": {{
                ""amount"": ""-25.00"",
                ""currency_code"": ""BRL""
              }},
              ""presentment_money"": {{
                ""amount"": ""-25.00"",
                ""currency_code"": ""BRL""
              }}
            }},
            ""kind"": ""shipping_refund"",
            ""order_id"": 4937459302562,
            ""reason"": ""Shipping refund"",
            ""refund_id"": 836712399010,
            ""tax_amount"": ""0.00"",
            ""tax_amount_set"": {{
              ""shop_money"": {{
                ""amount"": ""0.00"",
                ""currency_code"": ""BRL""
              }},
              ""presentment_money"": {{
                ""amount"": ""0.00"",
                ""currency_code"": ""BRL""
              }}
            }}
          }}
        ],
        ""transactions"": [
          {{
            ""id"": 5568098926754,
            ""admin_graphql_api_id"": ""gid://shopify/OrderTransaction/5568098926754"",
            ""amount"": ""186.55"",
            ""authorization"": null,
            ""created_at"": ""2022-03-30T15:26:39-03:00"",
            ""currency"": ""BRL"",
            ""device_id"": null,
            ""error_code"": null,
            ""gateway"": ""manual"",
            ""kind"": ""refund"",
            ""location_id"": null,
            ""message"": ""Refunded 186.55 from manual gateway"",
            ""order_id"": 4937459302562,
            ""parent_id"": 5567674613922,
            ""processed_at"": ""2022-03-30T15:26:39-03:00"",
            ""receipt"": {{}},
            ""source_name"": ""1830279"",
            ""status"": ""success"",
            ""test"": false,
            ""user_id"": null
          }}
        ],
        ""refund_line_items"": [
          {{
            ""id"": 332243435682,
            ""line_item_id"": 11703803445410,
            ""location_id"": 56043274402,
            ""quantity"": 1,
            ""restock_type"": ""cancel"",
            ""subtotal"": 161.55,
            ""subtotal_set"": {{
              ""shop_money"": {{
                ""amount"": ""161.55"",
                ""currency_code"": ""BRL""
              }},
              ""presentment_money"": {{
                ""amount"": ""161.55"",
                ""currency_code"": ""BRL""
              }}
            }},
            ""total_tax"": 0.0,
            ""total_tax_set"": {{
              ""shop_money"": {{
                ""amount"": ""0.00"",
                ""currency_code"": ""BRL""
              }},
              ""presentment_money"": {{
                ""amount"": ""0.00"",
                ""currency_code"": ""BRL""
              }}
            }},
            ""line_item"": {{
              ""id"": 11703803445410,
              ""admin_graphql_api_id"": ""gid://shopify/LineItem/11703803445410"",
              ""destination_location"": {{
                ""id"": 3447555129506,
                ""country_code"": ""BR"",
                ""province_code"": ""SC"",
                ""name"": ""Elisabeth Meyer"",
                ""address1"": ""Rodolfo Tureck,372,Ceramarte"",
                ""address2"": ""apto 304"",
                ""city"": ""Santa Catarina"",
                ""zip"": ""89295-000""
              }},
              ""fulfillable_quantity"": 0,
              ""fulfillment_service"": ""manual"",
              ""fulfillment_status"": null,
              ""gift_card"": false,
              ""grams"": 2560,
              ""name"": ""Balanço de Madeira Tão Perto, Tão Longe - Corda Preta - Corda Preta"",
              ""origin_location"": {{
                ""id"": 3389326524578,
                ""country_code"": ""BR"",
                ""province_code"": ""SC"",
                ""name"": ""oppa.com.br"",
                ""address1"": ""Rodovia BR 280 - km 123, 2866 - industrial sul"",
                ""address2"": """",
                ""city"": ""Rio Negrinho"",
                ""zip"": ""89295000""
              }},
              ""price"": 170.05,
              ""price_set"": {{
                ""shop_money"": {{
                  ""amount"": 170.05,
                  ""currency_code"": ""BRL""
                }},
                ""presentment_money"": {{
                  ""amount"": ""170.05"",
                  ""currency_code"": ""BRL""
                }}
              }},
              ""product_exists"": true,
              ""product_id"": 6984747122850,
              ""properties"": [],
              ""quantity"": 1,
              ""requires_shipping"": true,
              ""sku"": ""143000222"",
              ""taxable"": true,
              ""title"": ""Balanço de Madeira Tão Perto, Tão Longe - Corda Preta"",
              ""total_discount"": ""0.00"",
              ""total_discount_set"": {{
                ""shop_money"": {{
                  ""amount"": ""0.00"",
                  ""currency_code"": ""BRL""
                }},
                ""presentment_money"": {{
                  ""amount"": ""0.00"",
                  ""currency_code"": ""BRL""
                }}
              }},
              ""variant_id"": 40553906798754,
              ""variant_inventory_management"": ""shopify"",
              ""variant_title"": ""Corda Preta"",
              ""vendor"": ""Fechada"",
              ""tax_lines"": [],
              ""duties"": [],
              ""discount_allocations"": [
                {{
                  ""amount"": ""8.50"",
                  ""amount_set"": {{
                    ""shop_money"": {{
                      ""amount"": ""8.50"",
                      ""currency_code"": ""BRL""
                    }},
                    ""presentment_money"": {{
                      ""amount"": ""8.50"",
                      ""currency_code"": ""BRL""
                    }}
                  }},
                  ""discount_application_index"": 0
                }}
              ]
            }}
          }}
        ],
        ""duties"": []
      }}
    ],
    ""shipping_address"": {{
      ""first_name"": ""Elisabeth"",
      ""address1"": ""Rodolfo Tureck,372,Ceramarte"",
      ""phone"": ""47 99927-4221"",
      ""city"": ""Santa Catarina"",
      ""zip"": ""89295-000"",
      ""province"": ""Santa Catarina"",
      ""country"": ""Brazil"",
      ""last_name"": ""Meyer"",
      ""address2"": ""apto 304"",
      ""company"": ""035.543.609-45"",
      ""latitude"": -26.258635,
      ""longitude"": -49.528886,
      ""name"": ""Elisabeth Meyer"",
      ""country_code"": ""BR"",
      ""province_code"": ""SC""
    }},
    ""shipping_lines"": [
      {{
        ""id"": 4192748830882,
        ""carrier_identifier"": null,
        ""code"": ""CORREIOS - ENTREGA EM ATÉ 20 DIAS ÚTEIS"",
        ""delivery_category"": null,
        ""discounted_price"": 25.00,
        ""discounted_price_set"": {{
          ""shop_money"": {{
            ""amount"": 25.00,
            ""currency_code"": ""BRL""
          }},
          ""presentment_money"": {{
            ""amount"": ""25.00"",
            ""currency_code"": ""BRL""
          }}
        }},
        ""phone"": null,
        ""price"": 25.00,
        ""price_set"": {{
          ""shop_money"": {{
            ""amount"": 25.00,
            ""currency_code"": ""BRL""
          }},
          ""presentment_money"": {{
            ""amount"": ""25.00"",
            ""currency_code"": ""BRL""
          }}
        }},
        ""requested_fulfillment_service_id"": null,
        ""source"": ""shopify"",
        ""title"": ""CORREIOS - ENTREGA EM ATÉ 20 DIAS ÚTEIS"",
        ""tax_lines"": [],
        ""discount_allocations"": []
      }}
    ]
  }}
}}";

            order = JsonConvert.DeserializeObject<OrderResult>(jsonOrder);
        }

        public OrderResult order { get; set; }
    }
}
