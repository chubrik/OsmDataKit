using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace Kit.Osm
{
    internal class JsonFileService
    {
        public static T Read<T>(string path, string targetDirectory = null) where T : class
        {
            Debug.Assert(path != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var startTime = DateTimeOffset.Now;
            var nativePath = FileClient.NativePath(path, targetDirectory);
            LogService.Log($"Read json file: {nativePath}");
            T obj;

            try
            {
                using (var fileStream = FileClient.OpenRead(path, targetDirectory))
                using (var streamReader = new StreamReader(fileStream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                    obj = new JsonSerializer().Deserialize<T>(jsonTextReader);

                if (obj.Equals(null))
                    throw new InvalidOperationException($"Wrong json content: {nativePath}");

                LogService.Log($"Read json file completed at {TimeHelper.FormattedLatency(startTime)}");
                return obj;
            }
            catch (Exception exception)
            {
                Debug.Assert(exception.IsAllowed());
                throw;
            }
        }

        public static void Write<T>(string path, T obj, string targetDirectory = null) where T : class
        {
            Debug.Assert(path != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Debug.Assert(obj != null);

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var startTime = DateTimeOffset.Now;
            var nativePath = FileClient.NativePath(path, targetDirectory);
            LogService.Log($"Write json file: {nativePath}");
            var dirPath = PathHelper.Parent(nativePath);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                LogService.Log($"Create directory: {dirPath}");
            }

            try
            {
                using (var fileStream = FileClient.OpenWrite(path))
                using (var streamWriter = new StreamWriter(fileStream))
                using (var jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    new JsonSerializer().Serialize(jsonTextWriter, obj);
                    jsonTextWriter.Close();
                }

                LogService.Log($"Write json file completed at {TimeHelper.FormattedLatency(startTime)}");
            }
            catch (Exception exception)
            {
                Debug.Assert(exception.IsAllowed());
                throw;
            }
        }
    }
}
