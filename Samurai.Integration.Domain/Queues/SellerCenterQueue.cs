using Microsoft.Azure.ServiceBus;

using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Queues
{
    public class SellerCenterQueue
    {
        //Init
        public static string CreateProductQueue = "sellercenter-createproduct";
        public static string ProcessVariationOptionsProductQueue = "sellercenter-processvariationoptionsproductqueue";
        public static string ProcessCategoriesProductQueue = "sellercenter-processcategoriesproductqueue";
        public static string ProcessManufacturersProductQueue = "sellercenter-processmanufacturersproductqueue";
        public static string UpdatePriceQueue = "sellecenter-updatepricequeue";
        public static string UpdateStockProductQueue = "sellecenter-updatestockproductqueue";

        //Orders
        public static string UpdateStatusOrderQueue = "sellecenter-updatestatusorderqueue";
        public static string ProcessOrderQueue = "sellecenter-processorderqueue";
        public static string ListOrderQueue = "sellecenter-listorderqueue";
        public static string UpdatePartialOrderSeller = "sellecenter-updatepartialorderseller";
        public static string UpdateOrderSellerDeliveryPackage = "sellecenter-updateordersellerdeliverypackage";  
        public static string ListNewOrdersQueue = "sellecenter-listnewordersqueue";

        public static string GetPriceProductQueueErp(SellerCenterDataMessage seller)
        {
            return seller.ErpType switch
            {
                Enums.TenantType.Millennium => MillenniumQueue.GetPriceProductQueue,
                Enums.TenantType.Bling => BlingQueue.GetPriceProductQueue,
                Enums.TenantType.PluggTo => PluggToQueue.GetPriceProductQueue,
                _ => throw new ArgumentException($"Fila nao implementada para o tipo {nameof(seller.ErpType)}"),
            };
        }

        public static string GetCreateOrderQueuerErp(SellerCenterDataMessage seller)
        {
            return seller.ErpType switch
            {
                Enums.TenantType.Millennium => MillenniumQueue.CreateOrderQueue,
                Enums.TenantType.Bling => BlingQueue.CreateOrderQueue,
                Enums.TenantType.PluggTo => PluggToQueue.CreateOrderQueue,
                _ => throw new ArgumentException($"Fila nao implementada para o tipo {nameof(seller.ErpType)}"),
            };
        }

        public static List<string> GetAllQueues()
        {
            return new List<string> {
                CreateProductQueue,
                ProcessVariationOptionsProductQueue,
                ProcessCategoriesProductQueue,
                ProcessManufacturersProductQueue,
                UpdatePriceQueue,
                UpdateStatusOrderQueue,
                ProcessOrderQueue,
                UpdateStockProductQueue,
                ListOrderQueue,
                ListNewOrdersQueue,
                UpdatePartialOrderSeller,
                UpdateOrderSellerDeliveryPackage
            };
        }

        public struct Queues
        {
            public QueueClient CreateProductQueue { get; set; }
            public QueueClient ProcessVariationOptionsProductQueue { get; set; }
            public QueueClient ProcessCategoriesProductQueue { get; set; }
            public QueueClient ProcessManufacturersProductQueue { get; set; }
            public QueueClient UpdatePriceQueue { get; set; }
            public QueueClient GetPriceProduct { get; set; }
            public QueueClient ProcessOrderQueue { get; set; }
            public QueueClient UpdateStatusOrderQueue { get; set; }
            public QueueClient CreateOrderQueueERP { get; set; }
            public QueueClient UpdateStockProductQueue { get; set; }
            public QueueClient ListOrderQueue { get; set; }
            public QueueClient ListNewOrdersQueue { get; set; }
            public QueueClient UpdatePartialOrderSeller { get; set; }
            public QueueClient UpdateOrderSellerDeliveryPackage { get; set; }


            public async Task SendMessages<T>(params (QueueClient queue, T message, bool hasItens)[] obj) where T : class
            {
                if (obj is null) return;

                for (int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].hasItens)
                    {
                        var serviceBusMessage = new ServiceBusMessage(obj[i].message);
                        await obj[i].queue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));
                    }
                }
            }

            public async Task SendScheduleMessages<T>(params (QueueClient queue, T message, DateTimeOffset time, bool hasItens)[] obj) where T : class
            {
                if (obj is null) return;

                for (int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].hasItens)
                    {
                        var serviceBusMessage = new ServiceBusMessage(obj[i].message);
                        await obj[i].queue.ScheduleMessageAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()), obj[i].time);
                    }
                }
            }
        }
    }
}
