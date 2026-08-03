using System.Collections.Concurrent;

namespace MAS.Web.Services;

/// <summary>
/// لوجر بسيط بيكتب الأخطاء (Warning فما فوق) في ملف logs/errors.log
/// عشان نقدر نشوف سبب أي مشكلة على السيرفر بعد الرفع — من غير أي حزم خارجية.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly string? _filePath;
    private static readonly object _lock = new();
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string logDirectory)
    {
        // مجلد التطبيق مش دايماً بيبقى قابل للكتابة — على Azure App Service مثلاً
        // التطبيق بيشتغل من حزمة read-only. فبنجرّب أكتر من مكان بالترتيب،
        // ولو كلهم فشلوا بنطفّي الكتابة في ملف بدل ما نوقّع التطبيق من أوله.
        foreach (var dir in CandidateDirectories(logDirectory))
        {
            try
            {
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "errors.log");

                // نتأكد إننا فعلاً نقدر نكتب، مش بس نعمل المجلد
                File.AppendAllText(path, string.Empty);

                _filePath = path;
                return;
            }
            catch
            {
                // نجرّب اللي بعده
            }
        }
    }

    private static IEnumerable<string> CandidateDirectories(string preferred)
    {
        yield return preferred;

        // Azure App Service بيحط المسار ده في متغير HOME وبيكون قابل للكتابة
        var home = Environment.GetEnvironmentVariable("HOME");
        if (!string.IsNullOrEmpty(home))
            yield return Path.Combine(home, "LogFiles", "mas");

        yield return Path.Combine(Path.GetTempPath(), "mas-logs");
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _filePath, _lock));

    public void Dispose() => _loggers.Clear();

    private class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly string? _filePath;
        private readonly object _lock;

        public FileLogger(string category, string? filePath, object lockObj)
        {
            _category = category;
            _filePath = filePath;
            _lock = lockObj;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        // لو ملقيناش مكان نكتب فيه، اللوجر بيبقى مطفّي
        public bool IsEnabled(LogLevel logLevel) => _filePath != null && logLevel >= LogLevel.Warning;

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
