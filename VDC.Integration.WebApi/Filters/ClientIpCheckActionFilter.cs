using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;

namespace VDC.Integration.WebApi.Filters
{
    public class ClientIpCheckActionFilter : ActionFilterAttribute
    {
        readonly string _safeList;
        readonly ILogger _logger;


        public ClientIpCheckActionFilter(ILoggerFactory logger, string safeList = "")
        {
            _logger = logger.CreateLogger<ClientIpCheckActionFilter>();
            _safeList = safeList;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            _logger.LogDebug("Remote IpAddress: {RemoteIp}", remoteIp);
            var ip = _safeList.Split(';');
            var badIp = true;

            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            foreach (var address in ip)
            {
                var testIp = IPAddress.Parse(address);

                if (testIp.Equals(remoteIp))
                {
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {
                _logger.LogWarning("Forbidden Request from IP: {RemoteIp}", remoteIp);
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
