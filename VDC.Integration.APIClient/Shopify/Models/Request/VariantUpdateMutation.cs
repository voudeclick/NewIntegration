﻿using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class VariantUpdateMutation : BaseMutation<ProductVariantsBulkInput, ProductVariantsBulkMutationOutput>
    {
        public VariantUpdateMutation(ProductVariantsBulkInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation productVariantsBulkUpdate($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {{
                  productVariantsBulkUpdate(productId: $productId, variants: $variants) {{
                    product {{
                        id
                    }},
                    userErrors {{
                        field,
                        message
                    }}
                  }}
                }}
            ";
        }
    }

    public class ProductVariantsBulkInput : BaseMutationInput
    {
        public string productId { get; set; }
        public List<ProductVariantsBulkVariantMutationInput> variants { get; set; }

        public class ProductVariantsBulkVariantMutationInput
        {
            public string id { get; set; }
            public string barcode { get; set; }
            public decimal? price { get; set; }
            public ProductVariantsBulkVariantInventoryItemMutationInput inventoryItem { get; set; }
            public List<ProductVariantsBulkVariantInventoryOptionValuesInput> optionValues { get; set; }
        }

        public class ProductVariantsBulkVariantInventoryItemMutationInput
        {
            public string sku { get; set; }
            public ProductVariantsBulkVariantInventoryItemMeasurementMutationInput measurement { get; set; }
        }

        public class ProductVariantsBulkVariantInventoryItemMeasurementMutationInput
        {
            public ProductVariantsBulkVariantInventoryItemMeasurementWeightMutationInput weight { get; set; }
        }

        public class ProductVariantsBulkVariantInventoryItemMeasurementWeightMutationInput
        {
            public string unit { get; set; }
            public decimal? value { get; set; }
        }

        public class ProductVariantsBulkVariantInventoryOptionValuesInput
        {
            public string optionName { get; set; }
            public string name { get; set; }
            public string linkedMetafieldValue { get; set; }
        }
    }

    public class ProductVariantsBulkMutationOutput : BaseMutationOutput
    {
        public ProductVariantsBulkProductMutationOutput product { get; set; }
        public List<UserError> userErrors { get; set; }

        public class ProductVariantsBulkProductMutationOutput
        {
            public string id { get; set; }
        }
    }
}
