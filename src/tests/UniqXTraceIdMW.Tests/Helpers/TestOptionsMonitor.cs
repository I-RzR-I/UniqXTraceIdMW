using Microsoft.Extensions.Options;

namespace UniqXTraceIdMW.Tests.Helpers
{
    internal sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private T _current;

        public TestOptionsMonitor(T initial) => _current = initial;

        public T CurrentValue => _current;

        public T Get(string? name) => _current;

        /// <summary>Replaces the current value, simulating a hot-reload configuration change.</summary>
        public void Update(T value) => _current = value;

        public IDisposable OnChange(Action<T, string> listener) => NullDisposable.Instance;

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }
    }
}
