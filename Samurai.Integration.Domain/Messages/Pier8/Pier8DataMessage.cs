using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Messages.Omie;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;

namespace Samurai.Integration.Domain.Messages.Pier8
{
    public class Pier8DataMessage : IBaseQueue
    {
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        public string ApiKey { get; set; }
        public string Token { get; set; }
        public bool EnableIntegration { get; set; }
        public ShopifyDataMessage ShopifyDataMessage { get; set; }
        public OmieData OmieData { get; set; }


        public Pier8DataMessage(Tenant tenant)
        {
            Id = tenant.Id;
            StoreName = tenant.StoreName;
            StoreHandle = tenant.StoreHandle;
            ApiKey = tenant.Pier8Data.ApiKey;
            Token = tenant.Pier8Data.Token;
            EnableIntegration = tenant.EnablePier8Integration;
            ShopifyDataMessage = new ShopifyDataMessage(tenant);
            OmieData = new OmieData(tenant);


        }

        public bool EqualsTo(Pier8DataMessage data)
        {
            return
                Id == data.Id &&
                StoreName == data.StoreName &&
                StoreHandle == data.StoreHandle &&
                ApiKey == data.ApiKey &&
                Token == data.Token &&
                EnableIntegration == data.EnableIntegration &&
                ShopifyDataMessage == ShopifyDataMessage;
        }

    }
}
