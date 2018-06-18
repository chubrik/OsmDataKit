using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace Kit.Osm
{
    internal class FileService
    {
        public static T Read<T>(string filePath) where T : class
        {
            Debug.Assert(filePath != null);

            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            var startTime = DateTimeOffset.Now;
            var fullPath = FileClient.FullPath(filePath); //todo targetDirectory
            LogService.Log($"Read json file: {fullPath}");
            T obj;

            try
            {
                using (var fileStream = FileClient.OpenRead(fullPath))
                using (var streamReader = new StreamReader(fileStream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                    obj = new JsonSerializer().Deserialize<T>(jsonTextReader);

                if (obj.Equals(null))
                    throw new InvalidOperationException($"Wrong content \"{fullPath}\"");

                LogService.Log($"Read json file completed at {TimeHelper.FormattedLatency(startTime)}");
                return obj;
            }
            catch (Exception exception)
            {
                Debug.Assert(exception.IsAllowed());
                throw;
            }
        }

        public static void Save<T>(string filePath, T obj) where T : class
        {
            Debug.Assert(filePath != null);

            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            Debug.Assert(obj != null);

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var startTime = DateTimeOffset.Now;
            var fullPath = FileClient.FullPath(filePath); //todo targetDirectory
            LogService.Log($"Write json file: {fullPath}");
            var dirPath = PathHelper.Parent(fullPath);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                LogService.Log($"Create directory: {dirPath}");
            }

            try
            {
                using (var fileStream = FileClient.OpenWrite(fullPath))
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
