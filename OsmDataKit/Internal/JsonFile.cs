using Kit;
using Newtonsoft.Json;
using System;
using System.IO;

namespace OsmDataKit.Internal
{
    internal static class CacheProvider
    {
        public static bool Has(string cacheName) => File.Exists(CachePath(cacheName));

        public static void Delete(string cacheName) => File.Delete(CachePath(cacheName));

        public static GeoContext Get(string cacheName)
        {
            var path = CachePath(cacheName);

            return LogService.Log($"Read cache file \"{FileClient.LogPath(path)}\"", () =>
            {
                if (path == null)
                    throw new ArgumentNullException(nameof(path));

                using var fileStream = File.OpenRead(path);
                using var streamReader = new StreamReader(fileStream);
                using var jsonTextReader = new JsonTextReader(streamReader);

                var context = new JsonSerializer().Deserialize<GeoContext>(jsonTextReader);

                if (context.Equals(null))
                    throw new InvalidOperationException(
                        $"Wrong cache content \"{FileClient.LogPath(path)}\"");

                return context;
            });
        }

        public static void Put(string cacheName, GeoContext context)
        {
            var path = CachePath(cacheName);

            LogService.Log($"Write cache file \"{FileClient.LogPath(path)}\"", () =>
            {
                if (path == null)
                    throw new ArgumentNullException(nameof(path));

                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                var dirPath = Path.GetFullPath(path + @"\..");

                if (!Directory.Exists(dirPath))
                {
                    Log.Debug($"Create directory \"{FileClient.LogPath(dirPath)}\"");
                    Directory.CreateDirectory(dirPath);
                }

                if (File.Exists(path))
                {
                    Log.Debug("Delete previous cache file");
                    File.Delete(path);
                }

                using var fileStream = File.OpenWrite(path);
                using var streamWriter = new StreamWriter(fileStream);
                using var jsonTextWriter = new JsonTextWriter(streamWriter);

                new JsonSerializer().Serialize(jsonTextWriter, context);
                jsonTextWriter.Close();
            });
        }

        private static string CachePath(string cacheName) =>
            @$"{OsmService.CacheDirectory}\{cacheName}.json";
    }
}
