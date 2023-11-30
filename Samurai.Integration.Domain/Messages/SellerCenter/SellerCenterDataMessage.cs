using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Queues;

namespace Samurai.Integration.Domain.Messages.SellerCenter
{
    public class SellerCenterDataMessage : IBaseQueue
    {
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool DisableCustomerDocument { get; set; }
        public string Username { get; set; }
        public string SellerId { get; set; }
        public string SellerWarehouseId { get; set; }
        public string Password { get; set; }
        public string TenantId { get; set; }
        public TenantType ErpType { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public OrderIntegrationStatusEnum StatusIntegration { get; set; }
        public bool SellWithoutStock { get; set; }
        public bool DisableUpdateProduct { get; set; }
        
        public SellerCenterDataMessage(Tenant tenant)
        {
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            Username = tenant.SellerCenterData.Username;
            Password = tenant.SellerCenterData.Password;
            ErpType = tenant.Type;
            TenantId = tenant.SellerCenterData.TenantId;
            SellerId = tenant.SellerCenterData.SellerId;
            SellerWarehouseId = tenant.SellerCenterData.SellerWarehouseId;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;
            DisableCustomerDocument = tenant.DisableCustomerDocument;
            SellWithoutStock = tenant.SellerCenterData.SellWithoutStock;
            StatusIntegration = tenant.SellerCenterData.OrderIntegrationStatus;
            DisableUpdateProduct = tenant.SellerCenterData.DisableUpdateProduct;
        }

        public bool EqualsTo(SellerCenterDataMessage data)
        {
            return
                Id == data.Id &&
                StoreName == data.StoreName &&
                StoreHandle == data.StoreHandle &&
                Password == data.Password &&
                Username == data.Username &&
                TenantId == data.TenantId &&
                ErpType == data.ErpType &&
                SellerId == data.SellerId &&
                SellerWarehouseId == data.SellerWarehouseId &&
                ProductIntegrationStatus == data.ProductIntegrationStatus &&
                OrderIntegrationStatus == data.OrderIntegrationStatus &&
                SellWithoutStock == data.SellWithoutStock &&
                DisableCustomerDocument == data.DisableCustomerDocument &&
                StatusIntegration == data.StatusIntegration;
        }

    }
}
