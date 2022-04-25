using Kit;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace OsmDataKit.Internal
{
    internal static class CacheProvider
    {
        private static readonly JsonSerializerOptions _options =
            new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

        public static bool Has(string cacheName) => File.Exists(CachePath(cacheName));

        public static void Delete(string cacheName) => File.Delete(CachePath(cacheName));

        public static GeoContext Get(string cacheName)
        {
            var path = CachePath(cacheName);

            return LogService.Log($"Read cache file \"{FileClient.PathForLog(path)}\"", () =>
            {
                if (path == null)
                    throw new ArgumentNullException(nameof(path));

                var json = File.ReadAllText(path);
                var context = JsonSerializer.Deserialize<GeoContext>(json, _options);

                if (context == null)
                    throw new InvalidOperationException(
                        $"Wrong cache content \"{FileClient.PathForLog(path)}\"");

                return context;
            });
        }

        public static void Put(string cacheName, GeoContext context)
        {
            var path = CachePath(cacheName);

            LogService.Log($"Write cache file \"{FileClient.PathForLog(path)}\"", () =>
            {
                if (path == null)
                    throw new ArgumentNullException(nameof(path));

                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                var dirPath = Path.GetFullPath(path + @"\..");

                if (!Directory.Exists(dirPath))
                {
                    Log.Debug($"Create directory \"{FileClient.PathForLog(dirPath)}\"");
                    Directory.CreateDirectory(dirPath);
                }

                if (File.Exists(path))
                {
                    Log.Debug("Delete previous cache file");
                    File.Delete(path);
                }

                var json = JsonSerializer.Serialize(context, _options);
                File.WriteAllText(path, json);
            });
        }

        private static string CachePath(string cacheName) =>
            @$"{OsmService.CacheDirectory}\{cacheName}.json";
    }
}
