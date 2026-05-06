using RzR.Web.Middleware.TraceId.Abstractions;

namespace UniqXTraceIdMW.Tests.Helpers
{
    internal sealed class StubTraceIdAccessor : ITraceIdAccessor
    {
        public string TraceId { get; init; } = "stub-trace-123";
    }
}
