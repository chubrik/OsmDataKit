using Kit;
using System;

namespace OsmDataKit.Internal
{
    internal static class Log
    {
#if DEBUG
        public static void Debug(string message) => LogService.Log(message);
        public static void Debug(string message, Action block) => LogService.Log(message, block);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message, Action block) => block();
#endif
    }
}
