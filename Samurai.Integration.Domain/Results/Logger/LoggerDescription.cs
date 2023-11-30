using Newtonsoft.Json;
using Samurai.Integration.Domain.Messages;
using System;

namespace Samurai.Integration.Domain.Results.Logger
{
    public class LoggerDescription
    {
        public Guid? LogId { get; set; }
        public string TenantId { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public string Method { get; set; }
        public object Response { get; set; }
        public object Request { get; set; }
       

        public static string From(string tenantId, string type, string method, object request, object response, Guid? logId = null)
        {
            var log = new LoggerDescription
            {
                LogId = logId,
                TenantId = tenantId,
                Method = method,
                Type = type,
                Response = response,
                Request = request                
            };

            var result = JsonConvert.SerializeObject(log, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });

            if (result.Length > 20000)
                result = result[0..20000];

            return result;
        }

        #region FromOrder
        public static string FromOrder(string tenantId, string id, string method, object request, object response, Guid? logId = null)
        {
            var log = new LoggerDescription
            {
                LogId = logId,
                TenantId = tenantId,
                Method = method,
                Type = "order",
                Id = id,
                Request = request,
                Response = response,
            };

            return JsonConvert.SerializeObject(log, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
        #endregion

        #region FromProduct
        public static string FromProduct(string tenantId, string id, string method, object request, object response, Guid? logId = null)
        {
            var log = new LoggerDescription
            {
                LogId = logId,
                TenantId = tenantId,
                Method = method,
                Type = "product",
                Id = id,
                Request = request,
                Response = response,
            };

            return JsonConvert.SerializeObject(log, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        #endregion
    }


}

