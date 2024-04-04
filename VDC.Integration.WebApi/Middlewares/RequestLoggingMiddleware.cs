using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VDC.Integration.WebApi.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RequestLoggingMiddleware(RequestDelegate next,
                                            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);
        }

        private async Task LogRequest(HttpContext context)
        {

            context.Request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            var body = ReadStreamInChunks(requestStream);
            context.Request.Body.Position = 0;

            await _next(context);

            if (!CanLogRequest(context))
            {
                return;
            }

            _logger.LogInformation($"Http Request Information: {Environment.NewLine}" +
                                   "UserEmail: {UserEmail} " +
                                   "Method: {Method} " +
                                   "QueryString: {QueryString} " +
                                   "RequestBody: {Body}",
                                   context.User?.FindFirstValue(ClaimTypes.Email),
                                   context.Request.Method,
                                   context.Request.QueryString,
                                   body);
        }

        private bool CanLogRequest(HttpContext context)
        {
            return (context.User?.Identity?.IsAuthenticated ?? false) &&
                new HttpResponseMessage((HttpStatusCode)context.Response.StatusCode).IsSuccessStatusCode;
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                                                   0,
                                                   readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);
            return textWriter.ToString();
        }
    }
}
