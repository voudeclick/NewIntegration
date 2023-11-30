using Microsoft.AspNetCore.Builder;
using Samurai.Integration.WebApi.Middlewares;

namespace Samurai.Integration.WebApi.Extensions
{
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
