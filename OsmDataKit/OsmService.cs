using Kit;
using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public static class OsmService
    {
        public static void ValidateSource(string path)
        {
            Debug.Assert(path != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            LogService.LogInfo($"OSM validation: {path}");
            var previousType = OsmGeoType.Node;
            long? previousId = 0;
            long count = 0;
            long totalCount = 0;
            long noIdCount = 0;

            using (var fileStream = FileClient.OpenRead(path))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var entry in source)
                {
                    if (entry.Type > previousType)
                    {
                        LogService.LogInfo($"Found {count} {previousType.ToString().ToLower()}s");
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
            LogService.LogInfo($"Found {count} {previousType.ToString().ToLower()}s");
            LogService.LogInfo($"Total {totalCount} entries");

            if (noIdCount > 0)
                LogService.LogWarning($"{noIdCount} entries has no id");

            LogService.LogInfo($"OSM validation completed");
        }

        public static OsmResponse Load(string path, Func<OsmGeo, bool> predicate)
        {
            Debug.Assert(path != null);
            Debug.Assert(predicate != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var nodes = new Dictionary<long, NodeObject>();
            var ways = new Dictionary<long, WayObject>();
            var relations = new Dictionary<long, RelationObject>();

            using (var fileStream = FileClient.OpenRead(path))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var osmGeo in source.Where(predicate))
                    switch (osmGeo)
                    {
                        case Node osmNode:
                            nodes.Add(osmGeo.Id.GetValueOrDefault(), new NodeObject(osmNode));
                            break;

                        case Way osmWay:
                            ways.Add(osmGeo.Id.GetValueOrDefault(), new WayObject(osmWay));
                            break;

                        case Relation osmRelation:
                            relations.Add(osmGeo.Id.GetValueOrDefault(), new RelationObject(osmRelation));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(osmGeo));
                    }
            }

            LogService.LogInfo($"Loaded: {nodes.Count} nodes, {ways.Count} ways, {relations.Count} relations");
            LogService.LogInfo("Complete");
            return new OsmResponse { Nodes = nodes, Ways = ways, Relations = relations };
        }

        public static OsmResponse Load(string path, OsmRequest request)
        {
            Debug.Assert(path != null);
            Debug.Assert(request != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var requestNodeIds = request.NodeIds != null
                ? new HashSet<long>(request.NodeIds.Distinct())
                : new HashSet<long>();

            var requestWayIds = request.WayIds != null
                ? new HashSet<long>(request.WayIds.Distinct())
                : new HashSet<long>();

            var requestRelationIds = request.RelationIds != null
                ? new HashSet<long>(request.RelationIds.Distinct())
                : new HashSet<long>();

            var nodes = new Dictionary<long, NodeObject>();
            var ways = new Dictionary<long, WayObject>();
            var relations = new Dictionary<long, RelationObject>();
            List<long> missedNodeIds = null;
            List<long> missedWayIds = null;
            List<long> missedRelationIds = null;
            string logMessage;

            using (var fileStream = FileClient.OpenRead(path))
            {
                var source = new PBFOsmStreamSource(fileStream);
                var thisType = requestNodeIds.Count > 0 ? OsmGeoType.Node : requestWayIds.Count > 0 ? OsmGeoType.Way : OsmGeoType.Relation;
                long id;

                foreach (var osmGeo in source)
                    switch (thisType)
                    {
                        case OsmGeoType.Node:

                            if (osmGeo is Node)
                            {
                                id = osmGeo.Id.GetValueOrDefault();

                                if (requestNodeIds.Contains(id))
                                {
                                    nodes.Add(id, new NodeObject(osmGeo as Node));

                                    if (nodes.Count < requestNodeIds.Count)
                                        continue;
                                }
                                else
                                    continue;
                            }

                            missedNodeIds = requestNodeIds.Where(i => !nodes.ContainsKey(i)).OrderBy(i => i).ToList();
                            logMessage = $"Loaded {nodes.Count} nodes";

                            if (missedNodeIds.Count == 0)
                                LogService.LogInfo(logMessage);
                            else
                                LogService.LogWarning($"{logMessage} ({missedNodeIds.Count} missed)");

                            if (requestWayIds.Count > 0)
                            {
                                thisType = OsmGeoType.Way;
                                continue;
                            }

                            LogService.LogInfo("Loaded 0 ways");

                            if (requestRelationIds.Count > 0)
                            {
                                thisType = OsmGeoType.Relation;
                                continue;
                            }

                            goto Complete;

                        case OsmGeoType.Way:

                            if (osmGeo.Type < thisType)
                                continue;

                            if (osmGeo is Way)
                            {
                                id = osmGeo.Id.GetValueOrDefault();

                                if (requestWayIds.Contains(id))
                                {
                                    ways.Add(id, new WayObject(osmGeo as Way));

                                    if (ways.Count < requestWayIds.Count)
                                        continue;
                                }
                                else
                                    continue;
                            }

                            missedWayIds = requestWayIds.Where(i => !ways.ContainsKey(i)).OrderBy(i => i).ToList();
                            logMessage = $"Loaded {ways.Count} ways";

                            if (missedWayIds.Count == 0)
                                LogService.LogInfo(logMessage);
                            else
                                LogService.LogWarning($"{logMessage} ({missedWayIds.Count} missed)");

                            if (requestRelationIds.Count > 0)
                            {
                                thisType = OsmGeoType.Relation;
                                continue;
                            }

                            goto Complete;

                        case OsmGeoType.Relation:

                            if (osmGeo.Type < thisType)
                                continue;

                            id = osmGeo.Id.GetValueOrDefault();

                            if (requestRelationIds.Contains(id))
                            {
                                relations.Add(id, new RelationObject(osmGeo as Relation));

                                if (relations.Count == requestRelationIds.Count)
                                    goto Complete;
                            }

                            continue;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(thisType));
                    }
            }

            Complete:

            missedRelationIds = requestRelationIds.Where(i => !relations.ContainsKey(i)).OrderBy(i => i).ToList();
            logMessage = $"Loaded {relations.Count} relations";

            if (missedRelationIds.Count == 0)
                LogService.LogInfo(logMessage);
            else
                LogService.LogWarning($"{logMessage} ({missedRelationIds.Count} missed)");

            LogService.LogInfo("Complete");

            return new OsmResponse
            {
                Nodes = nodes,
                Ways = ways,
                Relations = relations,
                MissedNodeIds = missedNodeIds ?? new List<long>(0),
                MissedWayIds = missedWayIds ?? new List<long>(0),
                MissedRelationIds = missedRelationIds ?? new List<long>(0)
            };
        }
    }
}
