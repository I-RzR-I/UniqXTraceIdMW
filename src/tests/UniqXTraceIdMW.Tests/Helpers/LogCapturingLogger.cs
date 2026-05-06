using Microsoft.Extensions.Logging;
using RzR.Web.Middleware.TraceId.Middleware;

namespace UniqXTraceIdMW.Tests.Helpers
{
    internal sealed class LogCapturingLogger : ILogger<TraceMiddleware>
    {
        public record LogEntry(LogLevel Level, string FormattedMessage);

        private readonly List<LogEntry> _debug = new List<LogEntry>();

        public IReadOnlyList<LogEntry> DebugMessages => _debug;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Debug)
                _debug.Add(new LogEntry(logLevel, formatter(state, exception)));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => NoopDisposable.Instance;

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();
            public void Dispose() { }
        }
    }
}
