using System.Collections.Concurrent;

namespace MAS.Web.Services;

/// <summary>
/// لوجر بسيط بيكتب الأخطاء (Warning فما فوق) في ملف logs/errors.log
/// عشان نقدر نشوف سبب أي مشكلة على السيرفر بعد الرفع — من غير أي حزم خارجية.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private static readonly object _lock = new();
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        _filePath = Path.Combine(logDirectory, "errors.log");
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _filePath, _lock));

    public void Dispose() => _loggers.Clear();

    private class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly string _filePath;
        private readonly object _lock;

        public FileLogger(string category, string filePath, object lockObj)
        {
            _category = category;
            _filePath = filePath;
            _lock = lockObj;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {formatter(state, exception)}";
            if (exception != null)
                line += Environment.NewLine + exception;

            try
            {
                lock (_lock)
                {
                    // لو الملف كبر عن 5 ميجا نبدأ من جديد
                    var fi = new FileInfo(_filePath);
                    if (fi.Exists && fi.Length > 5 * 1024 * 1024)
                        File.WriteAllText(_filePath, string.Empty);

                    File.AppendAllText(_filePath, line + Environment.NewLine);
                }
            }
            catch
            {
                // مينفعش اللوجر نفسه يوقع التطبيق
            }
        }
    }
}
