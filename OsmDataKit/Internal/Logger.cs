using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if !DEBUG
using System.Runtime.CompilerServices;
#endif

namespace OsmDataKit.Internal
{
    internal static class Logger
    {
        private static readonly Stack<Stopwatch> _stopwatches = new();

#if DEBUG
        public static void Debug(string message) => LogBase(message, ConsoleColor.DarkGray);
        public static void Debug(string message, Action action) => Handle(message, action, ConsoleColor.DarkGray);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message, Action action) => action();
#endif

        public static void Log(string message) => LogBase(message);
        public static void Log(string message, Action action) => Handle(message, action);
        public static T Log<T>(string message, Func<T> action) => Handle(message, action);

        public static void Warning(string message) => LogBase(message, ConsoleColor.Yellow);

        private static void LogBase(string message, ConsoleColor? color = null)
        {
            var indent = string.Concat(Enumerable.Repeat("- ", _stopwatches.Count));
            var origColor = Console.ForegroundColor;

            if (color != null)
                Console.ForegroundColor = color.Value;

            Console.WriteLine($"{DateTimeOffset.Now:HH:mm:ss} {indent}{message}");

            if (color != null)
                Console.ForegroundColor = origColor;
        }

        private static void Handle(string message, Action action, ConsoleColor? color = null)
        {
            Begin(message, color);

            try
            {
                action();
            }
            catch
            {
                Throw(message);
                throw;
            }

            End(message, color);
        }

        private static T Handle<T>(string message, Func<T> action, ConsoleColor? color = null)
        {
            Begin(message, color);
            T result;

            try
            {
                result = action();
            }
            catch
            {
                Throw(message);
                throw;
            }

            End(message, color);
            return result;
        }

        private static void Begin(string message, ConsoleColor? color)
        {
            LogBase($"{message} - started...", color);
            _stopwatches.Push(Stopwatch.StartNew());
        }

        private static void End(string message, ConsoleColor? color)
        {
            var sw = _stopwatches.Pop();
            LogBase($"{message} - completed in {FormattedLatency(sw.Elapsed)}", color);
        }

        private static void Throw(string message)
        {
            var sw = _stopwatches.Pop();
            LogBase($"{message} - failed in {FormattedLatency(sw.Elapsed)}", ConsoleColor.Red);
        }

        private static string FormattedLatency(TimeSpan timeSpan) =>
            timeSpan.Hours > 0
                ? $@"{timeSpan:h\:mm\:ss}"
                : timeSpan.Minutes > 0
                    ? $@"{timeSpan:m\:ss}"
                    : timeSpan.Seconds > 0
                        ? $@"{timeSpan:s\.fff} s"
                        : timeSpan.Milliseconds + " ms";
    }
}
