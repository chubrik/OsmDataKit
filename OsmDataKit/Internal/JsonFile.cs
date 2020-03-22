using Kit;
using Newtonsoft.Json;
using System;
using System.IO;

namespace OsmDataKit.Internal
{
    internal class JsonFile
    {
        public static T Read<T>(string path) where T : class =>
            LogService.Log($"Read json file \"{FileClient.LogPath(path)}\"", () =>
            {
                if (path == null)
                    throw new ArgumentNullException(nameof(path));

                using var fileStream = File.OpenRead(path);
                using var streamReader = new StreamReader(fileStream);
                using var jsonTextReader = new JsonTextReader(streamReader);

                var obj = new JsonSerializer().Deserialize<T>(jsonTextReader);

                if (obj.Equals(null))
                    throw new InvalidOperationException($"Wrong json content \"{FileClient.LogPath(path)}\"");

                return obj;
            });

        public static void Write<T>(string path, T obj) where T : class =>
            LogService.Log($"Write json file \"{FileClient.LogPath(path)}\"", () =>
            {
                if (path == null)
                    throw new ArgumentNullException(nameof(path));

                if (obj == null)
                    throw new ArgumentNullException(nameof(obj));

                var dirPath = Path.GetFullPath(path + @"\..");

                if (!Directory.Exists(dirPath))
                {
                    Log.Debug($"Create directory \"{FileClient.LogPath(dirPath)}\"");
                    Directory.CreateDirectory(dirPath);
                }

                if (File.Exists(path))
                {
                    Log.Debug("Delete previous file");
                    File.Delete(path);
                }

                using var fileStream = File.OpenWrite(path);
                using var streamWriter = new StreamWriter(fileStream);
                using var jsonTextWriter = new JsonTextWriter(streamWriter);

                new JsonSerializer().Serialize(jsonTextWriter, obj);
                jsonTextWriter.Close();
            });
    }
}
