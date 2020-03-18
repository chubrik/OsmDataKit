using Kit;
using Newtonsoft.Json;
using System;
using System.IO;

namespace OsmDataKit.Internal
{
    internal class JsonFile
    {
        public static T Read<T>(string path) where T : class
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            LogService.Begin($"Read json file: {path}");
            T obj;

            using (var fileStream = File.OpenRead(path))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
                obj = new JsonSerializer().Deserialize<T>(jsonTextReader);

            if (obj.Equals(null))
                throw new InvalidOperationException($"Wrong json content: {path}");

            LogService.End("Read json file completed");
            return obj;
        }

        public static void Write<T>(string path, T obj) where T : class
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            LogService.Begin($"Write json file: {path}");
            var dirPath = PathHelper.Parent(path);

            if (!Directory.Exists(dirPath))
            {
                LogService.Log($"Create directory: {dirPath}");
                Directory.CreateDirectory(dirPath);
            }

            if (File.Exists(path))
            {
                LogService.Log("Delete previous file");
                File.Delete(path);
            }

            using (var fileStream = File.OpenWrite(path))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                new JsonSerializer().Serialize(jsonTextWriter, obj);
                jsonTextWriter.Close();
            }

            LogService.End("Write json file completed");
        }
    }
}
