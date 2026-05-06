using System.Text;
using Microsoft.AspNetCore.Http;

namespace UniqXTraceIdMW.Tests.Helpers
{
    internal static class TestHttpContextFactory
    {
        public static DefaultHttpContext MakeContext()
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            return ctx;
        }

        public static DefaultHttpContext MakeContext(string? traceparent)
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            if (traceparent != null)
                ctx.Request.Headers["traceparent"] = traceparent;

            return ctx;
        }

        public static DefaultHttpContext MakeContext((string Name, string Value)[]? headers)
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            if (headers != null)
                foreach (var (name, value) in headers)
                    ctx.Request.Headers[name] = value;

            return ctx;
        }

        public static DefaultHttpContext ContextWithBody(string body)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            return ctx;
        }
    }
}
