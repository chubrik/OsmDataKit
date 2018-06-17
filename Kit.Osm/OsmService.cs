using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Diagnostics;

namespace Kit.Osm
{
    public class OsmService
    {
        private OsmService() { }

        public static void ValidateSource(string srcPath)
        {
            Debug.Assert(!srcPath.IsNullOrEmpty());

            if (srcPath.IsNullOrEmpty())
                throw new ArgumentException(nameof(srcPath));

            LogService.Log($"Check source: {srcPath}");
            var previousType = OsmGeoType.Node;
            long? previousId = 0;
            long count = 0;
            long totalCount = 0;
            long noIdCount = 0;

            using (var fileStream = FileClient.OpenRead(srcPath))
            {
                var osmSource = new PBFOsmStreamSource(fileStream);

                foreach (var entry in osmSource)
                {
                    if (entry.Type > previousType)
                    {
                        LogService.Log($"Found {count} {previousType.ToString().ToLower()}s");
                        totalCount += count;
                        count = 0;
                        previousType = entry.Type;
                        previousId = 0;
                    }

                    count++;
                    var id = entry.Id;

                    if (id != null)
                    {
                        if (entry.Type < previousType || id < previousId)
                            LogService.LogWarning($"Was: {previousType}-{previousId}, now: {entry.Type}-{id}");

                        previousId = id;
                    }
                    else
                        noIdCount++;
                }
            }

            totalCount += count;
            LogService.Log($"Found {count} {previousType.ToString().ToLower()}s");
            LogService.Log($"Total {totalCount} entries");

            if (noIdCount > 0)
                LogService.Log($"{noIdCount} entries has no id");

            LogService.Log($"Check source completed");
        }
    }
}
