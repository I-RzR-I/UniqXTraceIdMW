using Microsoft.Extensions.Logging;
using RzR.Web.Middleware.TraceId.Middleware;

namespace UniqXTraceIdMW.Tests.Helpers
{
    internal sealed class CapturingLogger : ILogger<TraceMiddleware>
    {
        private readonly List<object> _scopes = new List<object>();

        public IReadOnlyList<object> CapturedScopes => _scopes;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            _scopes.Add(state);

            return NoopDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();
            public void Dispose() { }
        }
    }
}
