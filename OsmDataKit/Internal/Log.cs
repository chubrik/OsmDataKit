using Kit;
using System;
using System.Runtime.CompilerServices;

namespace OsmDataKit.Internal
{
    internal static class Log
    {
#if DEBUG
        public static void Debug(string message) => LogService.Info(message);
        public static void Debug(string message, Action block) => Wrap(message, block, LogLevel.Info);
        public static T Debug<T>(string message, Func<T> block) => Wrap(message, block, LogLevel.Info);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message, Action block) => block();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Debug<T>(string message, Func<T> block) => block();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trace(string message) => LogService.Log(message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string message) => LogService.Warning(message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trace(string message, Action block) => Wrap(message, block, LogLevel.Log);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Trace<T>(string message, Func<T> block) => Wrap(message, block, LogLevel.Log);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Wrap(string message, Action block, LogLevel level)
        {
            LogService.Begin(message, level);
            block();
            LogService.End(message, level);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Wrap<T>(string message, Func<T> block, LogLevel level)
        {
            LogService.Begin(message, level);
            var result = block();
            LogService.End(message, level);
            return result;
        }
    }
}
