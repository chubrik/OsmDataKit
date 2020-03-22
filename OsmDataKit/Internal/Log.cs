using System;

#if !DEBUG
using System.Runtime.CompilerServices;
#endif

namespace OsmDataKit.Internal
{
    internal static class Log
    {
#if DEBUG
        public static void Debug(string message) => Kit.LogService.Log(message);
        public static void Debug(string message, Action block) => Kit.LogService.Log(message, block);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message, Action block) => block();
#endif
    }
}
