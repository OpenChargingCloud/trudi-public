namespace TRuDI.Backend.Utils
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    using Serilog;

    public class RequestLogging
    {
        private readonly RequestDelegate next;

        public RequestLogging(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            Log.Debug(
                "Backend request: {0} {1}{2} {3}",
                context.Request.Scheme,
                context.Request.Host,
                context.Request.Path,
                DateTime.Now.ToString("hh:mm:ss.fff"));

            await this.next(context);

            Log.Debug(
                "Backend response: {0}, {1} ms",
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }

    public static class RequestLoggingExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLogging>();
        }
    }
}
