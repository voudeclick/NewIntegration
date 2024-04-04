using Microsoft.AspNetCore.Builder;
using VDC.Integration.WebApi.Middlewares;

namespace VDC.Integration.WebApi.Extensions
{
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
