using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class PluggToData : BaseEntity
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AccountUserId { get; set; }
        public string AccountSellerId { get; set; }
        public string OrderStatusMapping { get; set; }

        public void HideSensitiveData()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            Password = string.Empty;
        }
    }
}
