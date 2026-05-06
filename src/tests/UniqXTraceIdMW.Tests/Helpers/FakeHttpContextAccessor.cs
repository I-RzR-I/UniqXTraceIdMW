using Microsoft.AspNetCore.Http;

namespace UniqXTraceIdMW.Tests.Helpers
{
    internal sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }

        public FakeHttpContextAccessor(HttpContext? ctx = null) => HttpContext = ctx;
    }
}
