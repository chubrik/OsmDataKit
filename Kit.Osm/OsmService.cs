using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public static class OsmService
    {
        public static void Validate(string srcPath)
        {
            Debug.Assert(srcPath != null);

            if (srcPath == null)
                throw new ArgumentNullException(nameof(srcPath));

            LogService.Log($"OSM validation: {srcPath}");
            var previousType = OsmGeoType.Node;
            long? previousId = 0;
            long count = 0;
            long totalCount = 0;
            long noIdCount = 0;

            using (var fileStream = FileClient.OpenRead(srcPath))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var entry in source)
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

            LogService.Log($"OSM validation completed");
        }

        public static OsmResponse Load(string srcPath, Func<OsmGeo, bool> predicate)
        {
            Debug.Assert(srcPath != null);

            if (srcPath == null)
                throw new ArgumentNullException(nameof(srcPath));

            Debug.Assert(predicate != null);

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            List<OsmGeo> geos;

            using (var fileStream = FileClient.OpenRead(srcPath))
            {
                var source = new PBFOsmStreamSource(fileStream);
                geos = source.Where(predicate).ToList(); // ToList() should be there!
            }

            var nodes = geos.Where(i => i.Type == OsmGeoType.Node)
                            .ToDictionary(i => i.Id.GetValueOrDefault(), i => (Node)i);

            var ways = geos.Where(i => i.Type == OsmGeoType.Way)
                           .ToDictionary(i => i.Id.GetValueOrDefault(), i => (Way)i);

            var relations = geos.Where(i => i.Type == OsmGeoType.Relation)
                                .ToDictionary(i => i.Id.GetValueOrDefault(), i => (Relation)i);

            LogService.Log(
                $"loaded: {nodes.Count} nodes, {ways.Count} ways, {relations.Count} relations");

            LogService.Log("Complete"); //todo what?

            return new OsmResponse
            {
                Nodes = nodes,
                Ways = ways,
                Relations = relations,
                MissedNodeIds = new List<long>(),
                MissedWayIds = new List<long>(),
                MissedRelationIds = new List<long>()
            };
        }

        public static OsmResponse Load(string srcPath, OsmRequest query)
        {
            Debug.Assert(srcPath != null);

            if (srcPath == null)
                throw new ArgumentNullException(nameof(srcPath));

            Debug.Assert(query != null);

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var nodeIds = query.NodeIds != null
                ? new HashSet<long>(query.NodeIds.Distinct())
                : new HashSet<long>();

            var wayIds = query.WayIds != null
                ? new HashSet<long>(query.WayIds.Distinct())
                : new HashSet<long>();

            var relationIds = query.RelationIds != null
                ? new HashSet<long>(query.RelationIds.Distinct())
                : new HashSet<long>();

            var nodes = new Dictionary<long, Node>();
            var ways = new Dictionary<long, Way>();
            var relations = new Dictionary<long, Relation>();
            var missedNodeIds = new List<long>();
            var missedWayIds = new List<long>();
            var missedRelationIds = new List<long>();
            string logMessage;

            using (var fileStream = FileClient.OpenRead(srcPath))
            {
                var source = new PBFOsmStreamSource(fileStream);

                var thisType = nodeIds.Count > 0
                                   ? OsmGeoType.Node
                                   : wayIds.Count > 0
                                         ? OsmGeoType.Way
                                         : OsmGeoType.Relation;

                foreach (var osmGeo in source)
                {
                    var id = osmGeo.Id.GetValueOrDefault();

                    if (thisType == OsmGeoType.Node)
                    {
                        if (nodeIds.Contains(id))
                            nodes.Add(id, (Node)osmGeo);

                        if (nodes.Count < nodeIds.Count && osmGeo.Type == thisType)
                            continue;

                        logMessage = $"Loaded {nodes.Count} nodes";

                        missedNodeIds = nodeIds.Where(i => !nodes.ContainsKey(i))
                                               .OrderBy(i => i).ToList();

                        if (missedNodeIds.Count == 0)
                            LogService.Log(logMessage);
                        else
                            LogService.LogWarning($"{logMessage} ({missedNodeIds.Count} missed)");

                        if (wayIds.Count == 0 && relationIds.Count == 0)
                        {
                            LogService.Log("Loaded 0 ways");
                            break;
                        }

                        thisType = OsmGeoType.Way;
                    }
                    else if (thisType == OsmGeoType.Way)
                    {
                        if (osmGeo.Type < thisType)
                            continue;

                        if (wayIds.Contains(id))
                            ways.Add(id, (Way)osmGeo);

                        if (ways.Count < wayIds.Count && osmGeo.Type == thisType)
                            continue;

                        logMessage = $"Loaded {ways.Count} ways";

                        missedWayIds = wayIds.Where(i => !ways.ContainsKey(i))
                                             .OrderBy(i => i).ToList();

                        if (missedWayIds.Count == 0)
                            LogService.Log(logMessage);
                        else
                            LogService.LogWarning($"{logMessage} ({missedWayIds.Count} missed)");

                        if (relationIds.Count == 0)
                            break;

                        thisType = OsmGeoType.Relation;
                    }
                    else
                    {
                        if (osmGeo.Type < thisType)
                            continue;

                        if (relationIds.Contains(id))
                            relations.Add(id, (Relation)osmGeo);

                        if (relations.Count == relationIds.Count)
                            break;
                    }
                }

                logMessage = $"Loaded {relations.Count} relations";

                missedRelationIds = relationIds.Where(i => !relations.ContainsKey(i))
                                               .OrderBy(i => i).ToList();

                if (missedRelationIds.Count == 0)
                    LogService.Log(logMessage);
                else
                    LogService.LogWarning($"{logMessage} ({missedRelationIds.Count} missed)");
            }

            LogService.Log("Complete"); //todo what?

            return new OsmResponse
            {
                Nodes = nodes,
                Ways = ways,
                Relations = relations,
                MissedNodeIds = missedNodeIds,
                MissedWayIds = missedWayIds,
                MissedRelationIds = missedRelationIds
            };
        }
    }
}
