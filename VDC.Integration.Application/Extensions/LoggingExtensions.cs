using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using VDC.Integration.Domain.Messages.Millennium;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Queues;

namespace VDC.Integration.Application.Extensions
{
    public static class LoggingExtensions
    {
        #region Aux Methods
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var st = new StackTrace(new StackFrame(1));
            return st.GetFrame(0).GetMethod().Name;
        }
        private static Dictionary<string, string> SetupBaseLog(string methodName, string bllMethodError)
        {
            var log = new Dictionary<string, string>
            {
                { "Method", methodName }
            };

            if (bllMethodError != null)
            {
                log.Add("bllMethodError", bllMethodError);
            }

            return log;
        }
        #endregion

        #region FromService

        #region Handler
        /// <summary>
        /// Logging Ext Method meant to be used in Services routines.<para> The parameter
        /// <paramref name="methodName"/> can be setted as a hardcoded param or use
        /// <see cref="GetCurrentMethod"/> to get it dynamically.</para>
        /// </summary>
        /// <remarks>
        /// If <paramref name="methodName"/> it's not cited and delegated to its log method, it'll fall directly into <see cref="DefaultLoggingCase(string, object, string)"/>.
        /// </remarks>
        /// <param name="methodName"> MethodName where occurred logging event or error. </param>
        /// <param name="methodArgMessage"> Main param's routine's input </param>
        /// <param name="serviceDataMessage">Usually 'NameOfTheServiceData'</param>
        /// <param name="bllMethodError">Aditional Comments. ex.: "Error because it's a feature not a Bug"</param>
        /// <returns>(<see cref="string"/>) Logging message</returns>
        /// <exception cref="NullReferenceException">
        /// If <paramref name="methodName"/>, <paramref name="methodArgMessage"/>, <paramref name="serviceDataMessage"/> are null.
        /// </exception>
        public static string FromService(string methodName, object methodArgMessage, object serviceDataMessage, string bllMethodError = null) =>
            methodName switch
            {
                "UpdateFullProduct" => ProductLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "UpdatePartialProduct" => ProductLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "UpdatePartialSku" => SkuLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "UpdatePrice" => PriceLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "UpdateStock" => StockLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),

                //Millenniuum
                "ListProduct" => ProductLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "ListNewProducts" => ProductLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "ProcessProductImage" => ProductLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "GetPartialProductMessage" => ProductLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "ListNewPrices" => PriceLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "GetPriceProduct" => PriceLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "ListNewStocks" => StockLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "ListNewStockMto" => StockLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "UpdateOrder" => OrderLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "UpdateOrderNew" => OrderLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "ListNewOrders" => OrderLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),
                "ListOrder" => OrderLoggingCase(methodName, methodArgMessage, serviceDataMessage, bllMethodError),

                _ => DefaultLoggingCase(methodName, methodArgMessage, bllMethodError),
            };
        #endregion

        #region Method Cases
        private static string DefaultLoggingCase(string methodName, object methodArgMessage, string bllMethodError = null)
        {
            Dictionary<string, string> log = SetupBaseLog(methodName, bllMethodError);

            var methodArgMessageInput = JsonConvert.SerializeObject(methodArgMessage, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (serializer, error) => error.ErrorContext.Handled = true
            });

            log.Add("MessageInput", methodArgMessageInput);

            return ConvertToLog(log);
        }

        private static string ProductLoggingCase(string methodName, object methodArgMessage, object serviceDataMessage, string bllMethodError = null)
        {
            var log = SetupBaseLog(methodName, bllMethodError);

            Domain.Models.Product.Info productInfo = null;
            string variants = default(string);

            if (methodArgMessage is ShopifyUpdateFullProductMessage || methodArgMessage is ShopifyUpdatePartialProductMessage)
            {
                var typedProductMessage = methodArgMessage as ShopifyUpdateFullProductMessage;
                var typedPartialProductMessage = methodArgMessage as ShopifyUpdatePartialProductMessage;

                if (typedProductMessage != null)
                {
                    productInfo = typedProductMessage.ProductInfo;
                }
                else if (typedPartialProductMessage != null)
                {
                    productInfo = typedPartialProductMessage.ProductInfo;
                }

                if (productInfo != null)
                {
                    variants = JsonConvert.SerializeObject(productInfo.Variants, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Error = (serializer, error) => error.ErrorContext.Handled = true
                    });

                    log.Add("ShopifyId", productInfo.ShopifyId.HasValue ? $"{productInfo.ShopifyId}" : "");
                    log.Add("ExternalId", $"{productInfo.ShopifyId}");
                    log.Add("ProductTitle", $"{productInfo.Title}");
                    log.Add("ProductVariants", variants);
                }
            }

            return ConvertToLog(log, serviceDataMessage);
        }

        private static string SkuLoggingCase(string methodName, object methodArgMessage, object serviceDataMessage, string bllMethodError = null)
        {
            Dictionary<string, string> log = SetupBaseLog(methodName, bllMethodError);

            if (methodArgMessage is ShopifyUpdatePartialSkuMessage typedArgMessage)
            {
                string skuInfo = default;

                log.Add("ExternalProductId", typedArgMessage.ExternalProductId);

                if (typedArgMessage.SkuInfo != null)
                {
                    skuInfo = JsonConvert.SerializeObject(typedArgMessage.SkuInfo, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Error = (serializer, error) => error.ErrorContext.Handled = true
                    });

                    log.Add("SkuInfo", skuInfo);
                }
            }
            return ConvertToLog(log, serviceDataMessage);
        }

        private static string PriceLoggingCase(string methodName, object methodArgMessage, object serviceDataMessage, string bllMethodError = null)
        {
            Dictionary<string, string> log = SetupBaseLog(methodName, bllMethodError);

            if (methodArgMessage is ShopifyUpdatePriceMessage typedArgMessage)
            {
                string skuPrice = default;

                log.Add("ExternalProductId", typedArgMessage.ExternalProductId);

                if (typedArgMessage.Value != null)
                {
                    skuPrice = JsonConvert.SerializeObject(typedArgMessage.Value, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Error = (serializer, error) => error.ErrorContext.Handled = true
                    });

                    log.Add("SkuPrice", skuPrice);
                }
            }
            return ConvertToLog(log, serviceDataMessage);
        }

        private static string StockLoggingCase(string methodName, object methodArgMessage, object serviceDataMessage, string bllMethodError = null)
        {
            Dictionary<string, string> log = SetupBaseLog(methodName, bllMethodError);

            if (methodArgMessage is ShopifyUpdateStockMessage typedArgMessage)
            {
                string skuStock = default;
                log.Add("ExternalProductId", typedArgMessage.ExternalProductId);
                if (typedArgMessage.Value != null)
                {
                    skuStock = JsonConvert.SerializeObject(typedArgMessage.Value, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Error = (serializer, error) => error.ErrorContext.Handled = true
                    });
                    log.Add("SkuStock", skuStock);
                }
            }
            return ConvertToLog(log, serviceDataMessage);
        }

        private static string OrderLoggingCase(string methodName, object methodArgMessage, object serviceDataMessage, string bllMethodError = null)
        {
            return ConvertToLog(PropsArguments(SetupBaseLog(methodName, bllMethodError), methodArgMessage), serviceDataMessage);
        }

        #endregion

        #region Summarizer
        private static string ConvertToLog(Dictionary<string, string> log, object serviceDataMessage = null)
        {
            return JsonConvert.SerializeObject(PropsArguments(log, serviceDataMessage), new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (serializer, error) => error.ErrorContext.Handled = true
            });
        }

        private static string[] PropsRequired()
        {
            return @"ID;
                      TenantId;
                      ExternalId;
                      TransId;
                      ExternalOrderId;
                      IdProduto;
                      IdFoto;
                      ShopifyId;
                      sku;
                      Name;
                      Number;
                      Approved;
                      Shipped;
                      Status;
                      TrackingNumber;
                      Delivered;
                      Cancelled;
                      DataInicial;
                      DataFinal;
                      TenantType;
                      EnabledMultiLocation;
                      BlockFulfillmentNotificationPerShipmentService;
                      ShipmentServicesForFulfillmentNotification;
                      ProductGroupingEnabled;
                      IntegrationType;
                      StoreName;
                      StoreHandle;
                      Url;
                      VitrineId;
                      SplitEnabled;
                      SaleWithoutStockEnabled;
                      NameField;
                      CorDescription;
                      CorField;
                      Retry;".ToLower().Split(";");
        }

        private static Dictionary<string, string> PropsArguments(Dictionary<string, string> log, object methodArgMessage = null)
        {
            if (methodArgMessage is null)
                return log;

            var propsArguments = methodArgMessage.GetType()
                                                 .GetProperties()
                                                 .Where(x => PropsRequired().Contains(x.Name.ToLower()))
                                                 .ToList();

            foreach (var prop in propsArguments)
            {
                log.Add(prop.Name, $"{prop.GetValue(methodArgMessage)}");
            }

            return log;
        }
        #endregion

        #endregion

        #region CustomProperties (DLQ) (Obsolete)
        /// <summary>
        /// Method for custom properties for DLQ use
        /// </summary>
        /// <returns> Dictionary(string,object) </returns>
        [Obsolete("dictionary meant to use in DLQ's, custom properties, too much data, don't use it.", false)]
        public static IDictionary<string, object> CustomPropertiesDLQ
            (
            IBaseQueue qData, Message message, string type, string method,
            object request, object response, bool isCritical = false
            )
        {
            if (qData is MillenniumData mData)
            {
                return new Dictionary<string, object>
                {
                    { "TenantId", mData?.Id.ToString() },
                    { "OrderId", type },
                    { "Method", method },
                    { "Request Object", JsonConvert.SerializeObject(request) },
                    { "Response Object", JsonConvert.SerializeObject(response) },
                    { "SessionId", message?.SessionId },
                    { "OperatorType", mData?.OperatorType },
                    { "IntegrationType", mData?.IntegrationType },
                    { "Level", isCritical ? "Critical" : "Error" },
                };
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }
        #endregion
    }
}
