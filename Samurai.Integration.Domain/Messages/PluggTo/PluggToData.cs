using Newtonsoft.Json;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.PluggTo
{
    public class PluggToData : IBaseQueue
    {
        public IntegrationType IntegrationType { get; set; }
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public bool ProductIntegrationStatus { get; set; }
        public bool OrderIntegrationStatus { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AccountUserId { get; set; }
        public string AccountSellerId { get; set; }
        public List<PluggToDataOrderStatusMapping> OrderStatusMapping { get; set; }

        public PluggToData(Tenant tenant)
        {
            IntegrationType = tenant.IntegrationType;
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            ProductIntegrationStatus = tenant.ProductIntegrationStatus;
            OrderIntegrationStatus = tenant.OrderIntegrationStatus;

            ClientId = tenant.PluggToData.ClientId;
            ClientSecret = tenant.PluggToData.ClientSecret;
            Username = tenant.PluggToData.Username;
            Password = tenant.PluggToData.Password;
            AccountUserId = tenant.PluggToData.AccountUserId;
            AccountSellerId = tenant.PluggToData.AccountSellerId;

            OrderStatusMapping = string.IsNullOrWhiteSpace(tenant.PluggToData.OrderStatusMapping) ? new List<PluggToDataOrderStatusMapping>() :
                                 JsonConvert.DeserializeObject<List<PluggToDataOrderStatusMapping>>(tenant.PluggToData.OrderStatusMapping);

        }

        public bool EqualsTo(PluggToData data)
        {
            return
              Id == data.Id &&
              StoreName == data.StoreName &&
              StoreHandle == data.StoreHandle &&
              ProductIntegrationStatus == data.ProductIntegrationStatus &&
              OrderIntegrationStatus == data.OrderIntegrationStatus &&
              ClientId == data.ClientId &&
              ClientSecret == data.ClientSecret &&
              Username == data.Username &&
              Password == data.Password &&
              AccountUserId == data.AccountUserId &&
              AccountSellerId == data.AccountSellerId &&
              OrderStatusMapping == data.OrderStatusMapping;
        }
        public class PluggToDataOrderStatusMapping
        {
            public Guid ERPStatusId { get; set; }
            public string PluggToSituacaoNome { get; set; }
        }
    }
}