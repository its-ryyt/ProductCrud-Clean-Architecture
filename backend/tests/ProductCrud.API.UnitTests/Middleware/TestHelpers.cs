using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProductCrud.API.UnitTests.Middleware;

/// <summary>Logger that stores every log entry so tests can check them.</summary>
public class TestLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = new();
    public List<IReadOnlyDictionary<string, object>> Scopes { get; } = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        if (state is IReadOnlyDictionary<string, object> dict) Scopes.Add(dict);
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
        => Entries.Add((logLevel, formatter(state, exception)));

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

/// <summary>Simple IHostEnvironment so tests can switch Development / Production.</summary>
public class FakeHostEnvironment : IHostEnvironment
{
    public FakeHostEnvironment(string environmentName) => EnvironmentName = environmentName;

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; } = "ProductCrud.API";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
