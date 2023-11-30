using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class Pier8Data : BaseEntity
    {
        public string ApiKey { get; set; }
        public string Token { get; set; }

        public void HideSensitiveData()
        {
            ApiKey = string.Empty;
            Token = string.Empty;
        }

    }

}
