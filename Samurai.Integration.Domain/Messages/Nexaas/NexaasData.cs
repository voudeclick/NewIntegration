using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Queues;

namespace Samurai.Integration.Domain.Messages.Nexaas
{
    public class NexaasData : IBaseQueue
    {
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public bool DisableCustomerDocument { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public long OrganizationId { get; set; }
        public long SaleChannelId { get; set; }
        public long StockId { get; set; }
        public string OrderPrefix { get; set; }

        public NexaasData(Tenant tenant)
        {
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;
            DisableCustomerDocument = tenant.DisableCustomerDocument;
            Url = tenant.NexaasData.Url;
            Token = tenant.NexaasData.Token;
            OrganizationId = tenant.NexaasData.OrganizationId;
            SaleChannelId = tenant.NexaasData.SaleChannelId;
            StockId = tenant.NexaasData.StockId;
            OrderPrefix = tenant.NexaasData.OrderPrefix;
        }

        public bool EqualsTo(NexaasData data)
        {
            return
                Id == data.Id &&
                StoreName == data.StoreName &&
                StoreHandle == data.StoreHandle &&
                ProductIntegrationStatus == data.ProductIntegrationStatus &&
                OrderIntegrationStatus == data.OrderIntegrationStatus &&
                DisableCustomerDocument == data.DisableCustomerDocument &&
                Url == data.Url &&
                Token == data.Token &&
                OrganizationId == data.OrganizationId &&
                SaleChannelId == data.SaleChannelId &&
                StockId == data.StockId && 
                OrderPrefix == data.OrderPrefix;
        }
    }
}
