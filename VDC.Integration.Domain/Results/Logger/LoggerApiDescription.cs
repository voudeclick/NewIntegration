using Newtonsoft.Json;

namespace VDC.Integration.Domain.Results.Logger
{
    public class LoggerApiDescription
    {
        public string TenantId { get; set; }
        public string Method { get; set; }
        public object Request { get; set; }
        public object Response { get; set; }

        public static string From(string tenantId, string method, object request, object response)
        {
            var log = new LoggerApiDescription
            {
                TenantId = tenantId,
                Method = method,
                Request = request,
                Response = response,
            };

            return JsonConvert.SerializeObject(log, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
