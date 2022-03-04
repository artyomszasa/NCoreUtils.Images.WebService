using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images.WebService
{
    public static class Cleanup
    {
        private sealed class DummyDisposable : IDisposable
        {
            public static DummyDisposable Singleton { get; } = new DummyDisposable();

            private DummyDisposable() { }

            public void Dispose() { }
        }

        private sealed class DummyLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
                => DummyDisposable.Singleton;

            public bool IsEnabled(LogLevel logLevel)
                => false;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            { /* noop */ }
        }

        private static DateTime GetLastTimestamp(FileInfo finfo)
        {
            try
            {
                return finfo.LastAccessTime;
            }
            catch
            {
                return finfo.LastWriteTime;
            }
        }

        public static void PerformCleanup(string? path = default, ILogger? logger = default)
        {
            path ??= Path.GetTempPath();
            logger ??= new DummyLogger();
            if (!Directory.Exists(path))
            {
                logger.LogWarning("Specified directory does not exist: {Path}", path);
                return;
            }
            var now = DateTimeOffset.Now;
            var toRemove = Directory
                .EnumerateFileSystemEntries(path, "*", new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
#if NET6_0_OR_GREATER
                    MaxRecursionDepth = 5,
#endif
                    ReturnSpecialDirectories = false
                })
                .Where(subpath =>
                {
                    var finfo = new FileInfo(subpath);
                    return finfo.Exists && (now - GetLastTimestamp(finfo)) > TimeSpan.FromMinutes(15);
                });
            foreach (var subpath in toRemove)
            {
                try
                {
                    File.Delete(subpath);
                    logger.LogInformation("Succesfully removed {Path} as part of cleanup.", subpath);
                }
                catch (Exception exn)
                {
                    logger.LogWarning(exn, "Failed to remove {Path} as part of cleanup.", subpath);
                }
            }
        }
    }
}