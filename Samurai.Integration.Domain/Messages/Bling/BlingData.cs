using Newtonsoft.Json;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Enums.Bling;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Messages.Bling
{
    public class BlingData : IBaseQueue
    {
        public IntegrationType IntegrationType { get; set; }
        public long Id { get; set; }
        public string StoreHandle { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public bool EnabledMultiLocation { get; set; }
        public string ApiBaseUrl { get; set; }
        public string APIKey { get; set; }
        public string OrderPrefix { get; set; }
        public DateTime? LastProductUpdateDate { get; set; }
        public List<BlingDataOrderStatusMapping> OrderStatusMapping { get; set; }
        public string CategoriaId { get; set; }
        public OrderFieldBlingType OrderField { get; set; }
        public int? OrderStatusId { get; set; }


        public BlingData(Tenant tenant)
        {
            IntegrationType = tenant.IntegrationType;
            Id = tenant.Id;
            StoreHandle = tenant.StoreHandle;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;
            ApiBaseUrl = tenant.BlingData.ApiBaseUrl;
            APIKey = tenant.BlingData.APIKey;
            LastProductUpdateDate = tenant.BlingData.LastProductUpdateDate;
            OrderPrefix = tenant.BlingData.OrderPrefix;
            OrderStatusMapping = string.IsNullOrWhiteSpace(tenant.BlingData.OrderStatusMapping) ? new List<BlingDataOrderStatusMapping>() : JsonConvert.DeserializeObject<List<BlingDataOrderStatusMapping>>(tenant.BlingData.OrderStatusMapping);
            EnabledMultiLocation = tenant.MultiLocation;
            CategoriaId = tenant.BlingData.CategoriaId;
            OrderField = tenant.BlingData.OrderField;
            OrderStatusId = tenant.BlingData.OrderStatusId;
        }

        public bool EqualsTo(BlingData data)
        {
            return
              Id == data.Id &&
              StoreHandle == data.StoreHandle &&
              ProductIntegrationStatus == data.ProductIntegrationStatus &&
              OrderIntegrationStatus == data.OrderIntegrationStatus &&
              ApiBaseUrl == data.ApiBaseUrl &&
              APIKey == data.APIKey &&
              OrderPrefix == data.OrderPrefix &&
              EnabledMultiLocation == data.EnabledMultiLocation &&
              CategoriaId == data.CategoriaId &&
              OrderField == data.OrderField &&
              OrderStatusId == data.OrderStatusId &&
              OrderStatusMapping == data.OrderStatusMapping;
        }
    }

    public class BlingDataOrderStatusMapping
    {
        public Guid ERPStatusId { get; set; }
        public int BlingSituacaoId { get; set; }
        public string BlingSituacaoNome { get; set; }
    }
}
