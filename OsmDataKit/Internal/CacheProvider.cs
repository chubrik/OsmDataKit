namespace OsmDataKit.Internal;

using OsmDataKit.Logging;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

internal static class CacheProvider
{
    private static readonly JsonSerializerOptions _options =
        new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

    public static bool Has(string cacheName) => File.Exists(CachePath(cacheName));

    public static void Delete(string cacheName) => File.Delete(CachePath(cacheName));

    public static GeoContext Get(string cacheName)
    {
        var path = CachePath(cacheName);

        return Logger.Info($"Read cache file \"{path}\"", () =>
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var json = File.ReadAllText(path);
            var context = JsonSerializer.Deserialize<GeoContext>(json, _options);

            if (context == null)
                throw new InvalidOperationException(
                    $"Wrong cache content \"{path}\"");

            return context;
        });
    }

    public static void Put(string cacheName, GeoContext context)
    {
        var path = CachePath(cacheName);

        Logger.Info($"Write cache file \"{path}\"", () =>
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var dirPath = Path.GetFullPath(path + @"\..");

            if (!Directory.Exists(dirPath))
            {
                Logger.Debug($"Create directory \"{dirPath}\"");
                Directory.CreateDirectory(dirPath);
            }

            if (File.Exists(path))
            {
                Logger.Debug("Delete previous cache file");
                File.Delete(path);
            }

            var json = JsonSerializer.Serialize(context, _options);
            File.WriteAllText(path, json);
        });
    }

    private static string CachePath(string cacheName) =>
        @$"{OsmService.CacheDirectory}\{cacheName}.json";
}
