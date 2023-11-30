using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class OrderResult
    {
        public string id { get; set; }
        public string legacyResourceId { get; set; }
        public string name { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? cancelledAt { get; set; }
        public string note { get; set; }
        public string displayFinancialStatus { get; set; }
        public string displayFulfillmentStatus { get; set; }
        public PriceSetResult totalPriceSet { get; set; }
        public PriceSetResult subtotalPriceSet { get; set; }
        public PriceSetResult totalShippingPriceSet { get; set; }
        public PriceSetResult totalDiscountsSet { get; set; }
        public PriceSetResult totalTaxSet { get; set; }
        public List<AttributeResult> customAttributes { get; set; }
        public ShippingLineResult shippingLine { get; set; }
        public Connection<LineItemResult> lineItems { get; set; }
        public CustomerResult customer { get; set; }
        public AddressResult billingAddress { get; set; }
        public AddressResult shippingAddress { get; set; }
        public List<FulfillmentResult> fulfillments { get; set; }
        public List<DraftFulfillmentResult> draftFulfillments { get; set; }
        public List<string> tags { get; set; }


        public static string OnlyId = @"
                                id,
                                legacyResourceId
        ";

        public static string Tags = @"
                                id,
                                legacyResourceId,
                                tags,
                                createdAt";

        public static string Status = @"
                                id,
                                legacyResourceId,
                                cancelledAt,
                                displayFinancialStatus,
                                displayFulfillmentStatus,
                                fulfillments {
                                    id,
                                    legacyResourceId,
                                    deliveredAt,
                                    trackingInfo {
                                        company,
                                        number,
                                        url
                                    }
                                },
                                draftFulfillments{
                                    requiresShipping
                                    service {
                                        id,
                                        location {
                                            id,
                                            legacyResourceId,
                                            name
                                        }
                                    }
                                } 
        ";

        public static string CompleteOrder(string cursor = null) => @$"
                                        id,
                                        legacyResourceId,
                                        name,
                                        createdAt,
                                        cancelledAt,
                                        displayFinancialStatus,
                                        displayFulfillmentStatus,
                                        note,
                                        tags,
                                        fulfillments {{
                                            id,
                                            legacyResourceId,
                                            deliveredAt,
                                            trackingInfo {{
                                                company,
                                                number,
                                                url
                                            }}
                                        }},
                                        totalPriceSet {{
                                            shopMoney {{
                                                amount,
                                                currencyCode,
                                            }}
                                        }},
                                        subtotalPriceSet {{
                                            shopMoney {{
                                                amount,
                                                currencyCode
                                            }}
                                        }},
                                        totalShippingPriceSet {{
                                            shopMoney {{
                                                amount,
                                                currencyCode
                                            }}
                                        }},
                                        totalDiscountsSet {{
                                            shopMoney {{
                                                amount,
                                                currencyCode
                                            }}
                                        }},
                                        totalTaxSet {{
                                            shopMoney {{
                                                amount,
                                                currencyCode
                                            }}
                                        }},
                                        customAttributes {{
                                            key,
                                            value
                                        }},
                                        shippingLine {{
                                            title
                                        }},
                                        lineItems(first: 50 {GetCursor(cursor)}) {{
                                            pageInfo {{
                                                hasNextPage,
                                                hasPreviousPage
                                            }},
                                            edges {{
                                                cursor,
                                                node {{
                                                    id,
                                                    quantity,
                                                    sku,
                                                    originalUnitPriceSet {{
                                                        shopMoney {{
                                                            amount,
                                                            currencyCode
                                                        }}
                                                    }}
                                                }}
                                            }}
                                        }},
                                        customer {{
                                            id,
                                            legacyResourceId,
                                            firstName,
                                            lastName,
                                            phone,
                                            email,
                                            note,
                                            defaultAddress {{
                                                id,
                                                company,
                                                firstName,
                                                lastName,
                                                address1,
                                                address2,
                                                zip,
                                                city,
                                                provinceCode,
                                                phone,
                                                country,
                                                countryCodeV2
                                            }}
                                        }},
                                        billingAddress {{
                                            id,
                                            company,
                                            firstName,
                                            lastName,
                                            address1,
                                            address2,
                                            zip,
                                            city,
                                            provinceCode,
                                            phone,
                                            country,
                                            countryCodeV2
                                        }},
                                        shippingAddress {{
                                            id,
                                            firstName,
                                            lastName,
                                            company,
                                            address1,
                                            address2,
                                            zip,
                                            city,
                                            provinceCode,
                                            phone,
                                            country,
                                            countryCodeV2
                                        }}
        ";

        private static string GetCursor(string cursor)
        {
            return cursor != null ?
                $", after: \"{cursor}\"" : "";
        }
    }

    public class Orders
    {               
        public List<OrderId> orders { get; set; }        
    }

    public class OrderId
    {
        public long id { get; set; }
        public string tags { get; set; }
    }
}
