using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.AliExpress.Models.Response
{
    public class AliExpressListProductResponse
    {
        [JsonProperty("aliexpress_postproduct_redefining_findaeproductbyidfordropshipper_response")]
        public AeopFindProduct Product { get; set; }

        public class AeopFindProduct
        {
            [JsonProperty("result")]
            public AeopFindProductResult Result { get; set; }
        }

        public class AeopFindProductResult
        {
            [JsonProperty("error_code")]
            public long ErrorCode { get; set; }
            [JsonProperty("error_message")]
            public string ErrorMessage { get; set; }
            [JsonProperty("aeop_ae_product_s_k_us")]
            public AeopAeProductSKUsModel AeopAeProductSKUs { get; set; }
            public string Detail { get; set; }

            [JsonProperty("is_success")]
            public bool IsSuccess { get; set; }

            [JsonProperty("product_unit")]
            public long ProductUnit { get; set; }

            [JsonProperty("ws_offline_date")]
            public string WsOfflineDate { get; set; }

            [JsonProperty("ws_display")]
            public string WsDisplay { get; set; }

            [JsonProperty("category_id")]
            public long CategoryId { get; set; }

            [JsonProperty("aeop_a_e_multimedia")]
            public AeopAEMultimediaModel AeopAEMultimedia { get; set; }

            [JsonProperty("owner_member_id")]
            public string OwnerMemberId { get; set; }

            [JsonProperty("product_status_type")]
            public string ProductStatusType { get; set; }

            [JsonProperty("aeop_ae_product_propertys")]
            public AeopAeProductPropertysModel AeopAeProductPropertys { get; set; }

            [JsonProperty("gross_weight")]
            public string GrossWeight { get; set; }

            [JsonProperty("delivery_time")]
            public long DeliveryTime { get; set; }

            [JsonProperty("ws_valid_num")]
            public long WsValidNum { get; set; }

            [JsonProperty("gmt_modified")]
            public string GmtModified { get; set; }

            [JsonProperty("package_type")]
            public bool PackageType { get; set; }

            [JsonProperty("aeop_national_quote_configuration")]
            public AeopNationalQuoteConfigurationModel AeopNationalQuoteConfiguration { get; set; }

            public string Subject { get; set; }

            [JsonProperty("base_unit")]
            public long BaseUnit { get; set; }

            [JsonProperty("package_length")]
            public long PackageLength { get; set; }

            [JsonProperty("mobile_detail")]
            public string MobileDetail { get; set; }

            [JsonProperty("package_height")]
            public long PackageHeight { get; set; }

            [JsonProperty("package_width")]
            public long PackageWidth { get; set; }

            [JsonProperty("currency_code")]
            public string CurrencyCode { get; set; }

            [JsonProperty("gmt_create")]
            public string GmtCreate { get; set; }

            [JsonProperty("image_u_r_ls")]
            public string ImageURLs { get; set; }

            [JsonProperty("product_id")]
            public long ProductId { get; set; }

            [JsonProperty("product_price")]
            public string ProductPrice { get; set; }

            [JsonProperty("item_offer_site_sale_price")]
            public string ItemOfferSiteSalePrice { get; set; }

            [JsonProperty("total_available_stock")]
            public long TotalAvailableStock { get; set; }

            [JsonProperty("store_info")]
            public StoreInfoModel StoreInfo { get; set; }

            [JsonProperty("evaluation_count")]
            public long EvaluationCount { get; set; }

            [JsonProperty("avg_evaluation_rating")]
            public string AvgEvaluationRating { get; set; }

            [JsonProperty("order_count")]
            public long OrderCount { get; set; }

            public class AeopSkuProperty
            {
                [JsonProperty("sku_property_id")]
                public long SkuPropertyId { get; set; }

                [JsonProperty("sku_image")]
                public string SkuImage { get; set; }

                [JsonProperty("property_value_id_long")]
                public long PropertyValueIdLong { get; set; }

                [JsonProperty("property_value_definition_name")]
                public string PropertyValueDefinitionName { get; set; }

                [JsonProperty("sku_property_value")]
                public string SkuPropertyValue { get; set; }

                [JsonProperty("sku_property_name")]
                public string SkuPropertyName { get; set; }
            }

            public class AeopSKUPropertysModel
            {
                [JsonProperty("aeop_sku_property")]
                public List<AeopSkuProperty> AeopSkuProperty { get; set; }
            }

            public class AeopAeProductSkuModel
            {
                [JsonProperty("sku_stock")]
                public bool SkuStock { get; set; }

                [JsonProperty("sku_price")]
                public double SkuPrice { get; set; }

                [JsonProperty("sku_code")]
                public string SkuCode { get; set; }

                [JsonProperty("ipm_sku_stock")]
                public long IpmSkuStock { get; set; }

                public string Id { get; set; }

                [JsonProperty("currency_code")]
                public string CurrencyCode { get; set; }

                [JsonProperty("aeop_s_k_u_propertys")]
                public AeopSKUPropertysModel AeopSKUPropertys { get; set; }

                public string Barcode { get; set; }

                [JsonProperty("offer_sale_price")]
                public decimal OfferSalePrice { get; set; }

                [JsonProperty("offer_bulk_sale_price")]
                public string OfferBulkSalePrice { get; set; }

                [JsonProperty("sku_bulk_order")]
                public long SkuBulkOrder { get; set; }

                [JsonProperty("s_k_u_available_stock")]
                public long SKUAvailableStock { get; set; }
            }

            public class AeopAeProductSKUsModel
            {
                [JsonProperty("aeop_ae_product_sku")]
                public List<AeopAeProductSkuModel> AeopAeProductSku { get; set; }
            }

            public class AeopAeVideoModel
            {
                [JsonProperty("poster_url")]
                public string PosterUrl { get; set; }

                [JsonProperty("media_type")]
                public string MediaType { get; set; }

                [JsonProperty("media_status")]
                public string MediaStatus { get; set; }

                [JsonProperty("media_id")]
                public long MediaId { get; set; }

                [JsonProperty("ali_member_id")]
                public long AliMemberId { get; set; }
            }

            public class AeopAEVideos
            {
                [JsonProperty("aeop_ae_video")]
                public List<AeopAeVideoModel> AeopAeVideo { get; set; }
            }

            public class AeopAEMultimediaModel
            {
                [JsonProperty("aeop_a_e_videos")]
                public AeopAEVideos AeopAEVideos { get; set; }
            }

            public class AeopAeProductPropertyModel
            {
                [JsonProperty("attr_value_unit")]
                public string AttrValueUnit { get; set; }

                [JsonProperty("attr_value_start")]
                public string AttrValueStart { get; set; }

                [JsonProperty("attr_value_id")]
                public long AttrValueId { get; set; }

                [JsonProperty("attr_value_end")]
                public string AttrValueEnd { get; set; }

                [JsonProperty("attr_value")]
                public string AttrValue { get; set; }

                [JsonProperty("attr_name_id")]
                public long AttrNameId { get; set; }

                [JsonProperty("attr_name")]
                public string AttrName { get; set; }
            }

            public class AeopAeProductPropertysModel
            {
                [JsonProperty("aeop_ae_product_property")]
                public List<AeopAeProductPropertyModel> AeopAeProductProperty { get; set; }
            }

            public class AeopNationalQuoteConfigurationModel
            {
                [JsonProperty("configuration_type")]
                public string ConfigurationType { get; set; }

                [JsonProperty("configuration_data")]
                public string ConfigurationData { get; set; }
            }

            public class StoreInfoModel
            {
                [JsonProperty("communication_rating")]
                public string CommunicationRating { get; set; }

                [JsonProperty("item_as_descriped_rating")]
                public string ItemAsDescripedRating { get; set; }

                [JsonProperty("shipping_speed_rating")]
                public string ShippingSpeedRating { get; set; }

                [JsonProperty("store_id")]
                public long StoreId { get; set; }

                [JsonProperty("store_name")]
                public string StoreName { get; set; }
            }
        }
    }

}
