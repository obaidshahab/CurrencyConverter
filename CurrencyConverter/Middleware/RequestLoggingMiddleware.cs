using System.Diagnostics;
using System.IO;

namespace CurrencyConverter.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
           

            var stopWatch = Stopwatch.StartNew();
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var method = context.Request.Method;
            var endpoint = context.Request.Path;
            var clientId = context.User?.FindFirst("clientId")?.Value
                                              ?? "anonymous";

            var correlationId = context.TraceIdentifier;

            //_logger.LogInformation("ClientId: {clientId}", clientId);
            //_logger.LogInformation("Incoming request from IP: {IP}", ip);
            //_logger.LogInformation("MethodType {method}", method);
            //_logger.LogInformation("Endpoint {endpoint}", endpoint);

            await _next(context); 
            var statusCode = context.Response.StatusCode;
            stopWatch.Stop();
            //_logger.LogInformation("StatusCode {statuscode}", statusCode);
            //_logger.LogInformation("Execution Time {time}", stopWatch.ElapsedMilliseconds);

            _logger.LogInformation(
            "Request completed {@log}",
            new
            {
                ClientIP = ip,
                ClientId = clientId,
                Method = method,
                Endpoint = endpoint,
                StatusCode = statusCode,
                DurationMs = stopWatch.ElapsedMilliseconds,
                CorrelationId = correlationId
            });



        }
    }
}
