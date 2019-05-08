﻿using Kit;
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

            List<OsmGeo> geos;

            using (var fileStream = FileClient.OpenRead(path))
            {
                var source = new PBFOsmStreamSource(fileStream);
                geos = source.Where(predicate).ToList();
            }

            var nodes = geos.Where(i => i.Type == OsmGeoType.Node)
                            .ToDictionary(i => i.Id.GetValueOrDefault(), i => (Node)i);

            var ways = geos.Where(i => i.Type == OsmGeoType.Way)
                           .ToDictionary(i => i.Id.GetValueOrDefault(), i => (Way)i);

            var relations = geos.Where(i => i.Type == OsmGeoType.Relation)
                                .ToDictionary(i => i.Id.GetValueOrDefault(), i => (Relation)i);

            LogService.LogInfo($"Loaded: {nodes.Count} nodes, {ways.Count} ways, {relations.Count} relations");
            LogService.LogInfo("Complete");

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

        public static OsmResponse Load(string path, OsmRequest request)
        {
            Debug.Assert(path != null);
            Debug.Assert(request != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var nodeIds = request.NodeIds != null
                ? new HashSet<long>(request.NodeIds.Distinct())
                : new HashSet<long>();

            var wayIds = request.WayIds != null
                ? new HashSet<long>(request.WayIds.Distinct())
                : new HashSet<long>();

            var relationIds = request.RelationIds != null
                ? new HashSet<long>(request.RelationIds.Distinct())
                : new HashSet<long>();

            var nodes = new Dictionary<long, Node>();
            var ways = new Dictionary<long, Way>();
            var relations = new Dictionary<long, Relation>();
            var missedNodeIds = new List<long>();
            var missedWayIds = new List<long>();
            var missedRelationIds = new List<long>();
            string logMessage;

            using (var fileStream = FileClient.OpenRead(path))
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
                            LogService.LogInfo(logMessage);
                        else
                            LogService.LogWarning($"{logMessage} ({missedNodeIds.Count} missed)");

                        if (wayIds.Count == 0 && relationIds.Count == 0)
                        {
                            LogService.LogInfo("Loaded 0 ways");
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
                            LogService.LogInfo(logMessage);
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
                    LogService.LogInfo(logMessage);
                else
                    LogService.LogWarning($"{logMessage} ({missedRelationIds.Count} missed)");
            }

            LogService.LogInfo("Complete");

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
