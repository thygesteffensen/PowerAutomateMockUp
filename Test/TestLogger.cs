using System;
using Microsoft.Extensions.Logging;

namespace Test
{
    // https://alastaircrabtree.com/using-logging-in-unit-tests-in-net-core/
    public static class TestLogger
    {
        public static ILogger<T> Create<T>()
        {
            var logger = new NUnitLogger<T>();
            return logger;
        }

        private class NUnitLogger<T> : ILogger<T>, IDisposable
        {
            private readonly Action<string> _output = Console.WriteLine;

            public void Dispose()
            {
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter) => _output(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => this;
        }
    }
}