using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using RzR.Web.Middleware.TraceId.Middleware;
using RzR.Web.Middleware.TraceId.Middleware.Options;

namespace UniqXTraceIdMW.Tests.Helpers
{
    internal static class TestMiddlewareRunner
    {
        public static TraceMiddleware MakeMiddleware(TraceOptions opts, params Func<HttpContext, Task>[] callbacks)
            => new TraceMiddleware(
                _ => Task.CompletedTask,
                opts,
                NullLogger<TraceMiddleware>.Instance,
                callbacks);

        public static async Task<DefaultHttpContext> InvokeAsync(
            TraceOptions opts, DefaultHttpContext? context = null)
        {
            context ??= TestHttpContextFactory.MakeContext();
            var mw = new TraceMiddleware(
                _ => Task.CompletedTask,
                opts,
                NullLogger<TraceMiddleware>.Instance);

            await mw.Invoke(context);

            return context;
        }

        public static async Task<(DefaultHttpContext ctx, LogCapturingLogger logger)> InvokeWithLogCaptureAsync(
            TraceOptions opts, string? traceparent = null)
        {
            var ctx = TestHttpContextFactory.MakeContext(traceparent);
            var logger = new LogCapturingLogger();
            var mw = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);
            await mw.Invoke(ctx);

            return (ctx, logger);
        }
    }
}
