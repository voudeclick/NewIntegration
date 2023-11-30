using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages
{
    public class TenantDataMessage : IBaseQueue
    {
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public TenantType Type { get; set; }
        public IntegrationType IntegrationType { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public bool ProductIntegrationStatus { get; set; }

        public TenantDataMessage(Tenant tenant)
        {
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            IntegrationType = tenant.IntegrationType;
            Type = tenant.Type;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;
        }

        public bool EqualsTo(TenantDataMessage data)
        {
            return
                Id == data.Id &&
                StoreName == data.StoreName &&
                StoreHandle == data.StoreHandle &&
                IntegrationType == data.IntegrationType &&
                Type == data.Type &&
                ProductIntegrationStatus == data.ProductIntegrationStatus &&
                OrderIntegrationStatus == data.OrderIntegrationStatus;
        }
    }
}
