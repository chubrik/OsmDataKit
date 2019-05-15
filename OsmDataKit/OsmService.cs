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
        #region Validate

        public static void ValidateSource(string pbfPath)
        {
            Debug.Assert(pbfPath != null);

            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            LogService.BeginInfo($"OSM validation: {pbfPath}");
            var previousType = OsmGeoType.Node;
            long? previousId = 0;
            long count = 0;
            long totalCount = 0;
            long noIdCount = 0;

            using (var fileStream = FileClient.OpenRead(pbfPath))
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

            LogService.EndInfo($"OSM validation completed");
        }

        #endregion

        #region Load

        public static OsmResponse Load(string pbfPath, Func<OsmGeo, bool> predicate)
        {
            Debug.Assert(pbfPath != null);
            Debug.Assert(predicate != null);

            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            LogService.BeginInfo("Load OSM data");
            var nodes = new Dictionary<long, NodeObject>();
            var ways = new Dictionary<long, WayObject>();
            var relations = new Dictionary<long, RelationObject>();

            using (var fileStream = FileClient.OpenRead(pbfPath))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var osmGeo in source.Where(predicate))
                    switch (osmGeo.Type)
                    {
                        case OsmGeoType.Node:
                            nodes.Add(osmGeo.Id.GetValueOrDefault(), new NodeObject(osmGeo as Node));
                            break;

                        case OsmGeoType.Way:
                            ways.Add(osmGeo.Id.GetValueOrDefault(), new WayObject(osmGeo as Way));
                            break;

                        case OsmGeoType.Relation:
                            relations.Add(osmGeo.Id.GetValueOrDefault(), new RelationObject(osmGeo as Relation));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(OsmGeoType));
                    }
            }

            LogService.LogInfo($"Loaded: {nodes.Count} nodes, {ways.Count} ways, {relations.Count} relations");
            LogService.EndInfo("Load OSM data completed");
            return new OsmResponse { Nodes = nodes, Ways = ways, Relations = relations };
        }

        public static OsmResponse Load(string pbfPath, OsmRequest request)
        {
            Debug.Assert(pbfPath != null);
            Debug.Assert(request != null);

            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            LogService.BeginInfo("Load OSM data");

            var requestNodeIds = request.NodeIds != null
                ? new HashSet<long>(request.NodeIds.Distinct())
                : new HashSet<long>();

            var requestWayIds = request.WayIds != null
                ? new HashSet<long>(request.WayIds.Distinct())
                : new HashSet<long>();

            var requestRelationIds = request.RelationIds != null
                ? new HashSet<long>(request.RelationIds.Distinct())
                : new HashSet<long>();

            var foundNodes = new Dictionary<long, NodeObject>();
            var foundWays = new Dictionary<long, WayObject>();
            var foundRelations = new Dictionary<long, RelationObject>();
            List<long> missedNodeIds = null;
            List<long> missedWayIds = null;
            List<long> missedRelationIds = null;
            string logMessage;

            using (var fileStream = FileClient.OpenRead(pbfPath))
            {
                var source = new PBFOsmStreamSource(fileStream);
                var thisType = requestNodeIds.Count > 0 ? OsmGeoType.Node : requestWayIds.Count > 0 ? OsmGeoType.Way : OsmGeoType.Relation;
                long id;

                foreach (var osmGeo in source)
                    switch (thisType)
                    {
                        case OsmGeoType.Node:

                            if (osmGeo.Type == OsmGeoType.Node)
                            {
                                id = osmGeo.Id.GetValueOrDefault();

                                if (requestNodeIds.Contains(id))
                                {
                                    foundNodes.Add(id, new NodeObject(osmGeo as Node));

                                    if (foundNodes.Count < requestNodeIds.Count)
                                        continue;
                                }
                                else
                                    continue;
                            }

                            missedNodeIds = requestNodeIds.Where(i => !foundNodes.ContainsKey(i)).OrderBy(i => i).ToList();
                            logMessage = $"Loaded {foundNodes.Count} nodes";

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

                            if (osmGeo.Type == OsmGeoType.Way)
                            {
                                id = osmGeo.Id.GetValueOrDefault();

                                if (requestWayIds.Contains(id))
                                {
                                    foundWays.Add(id, new WayObject(osmGeo as Way));

                                    if (foundWays.Count < requestWayIds.Count)
                                        continue;
                                }
                                else
                                    continue;
                            }

                            missedWayIds = requestWayIds.Where(i => !foundWays.ContainsKey(i)).OrderBy(i => i).ToList();
                            logMessage = $"Loaded {foundWays.Count} ways";

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
                                foundRelations.Add(id, new RelationObject(osmGeo as Relation));

                                if (foundRelations.Count == requestRelationIds.Count)
                                    goto Complete;
                            }

                            continue;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(OsmGeoType));
                    }
            }

            Complete:

            missedRelationIds = requestRelationIds.Where(i => !foundRelations.ContainsKey(i)).OrderBy(i => i).ToList();
            logMessage = $"Loaded {foundRelations.Count} relations";

            if (missedRelationIds.Count == 0)
                LogService.LogInfo(logMessage);
            else
                LogService.LogWarning($"{logMessage} ({missedRelationIds.Count} missed)");

            LogService.EndInfo("Load OSM data completed");

            return new OsmResponse
            {
                Nodes = foundNodes,
                Ways = foundWays,
                Relations = foundRelations,
                MissedNodeIds = missedNodeIds ?? new List<long>(0),
                MissedWayIds = missedWayIds ?? new List<long>(0),
                MissedRelationIds = missedRelationIds ?? new List<long>(0)
            };
        }

        #endregion

        #region Load objects

        public static OsmObjectResponse LoadObjects(string pbfPath, string cacheName, OsmRequest request, int stepLimit = 0)
        {
            Debug.Assert(request != null);

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return LoadObjects(pbfPath, cacheName, request, predicate: null, stepLimit);
        }

        public static OsmObjectResponse LoadObjects(string pbfPath, string cacheName, Func<OsmGeo, bool> predicate, int stepLimit = 0)
        {
            Debug.Assert(predicate != null);

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return LoadObjects(pbfPath, cacheName, request: null, predicate, stepLimit);
        }

        private static OsmObjectResponse LoadObjects(
            string pbfPath, string cacheName, OsmRequest request, Func<OsmGeo, bool> predicate, int stepLimit)
        {
            Debug.Assert(pbfPath != null);
            Debug.Assert(!cacheName.IsNullOrWhiteSpace());
            Debug.Assert(stepLimit >= 0);

            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            if (cacheName.IsNullOrWhiteSpace())
                throw new ArgumentNullOrWhiteSpaceException(nameof(cacheName));

            if (stepLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(stepLimit));

            var cachePath = FullCachePath(cacheName);
            OsmResponse response;

            if (FileClient.Exists(cachePath))
            {
                response = JsonFileClient.Read<OsmResponse>(cachePath);
                return BuildObjects(response);
            }

            if (!FileClient.Exists(pbfPath))
                throw new InvalidOperationException();

            LogService.BeginInfo("Load OSM objects");
            var cacheStepPath = StepCachePath(cacheName, 1);

            if (FileClient.Exists(cacheStepPath))
                response = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
                LogService.LogInfo($"Step 1");

                if (request != null)
                    response = Load(pbfPath, request);
                else
                if (predicate != null)
                    response = Load(pbfPath, predicate);
                else
                    throw new InvalidOperationException();

                JsonFileClient.Write(cacheStepPath, response);
            }

            if (stepLimit != 1)
                for (var step = 2; stepLimit == 0 || step <= stepLimit; step++)
                    if (!FillNested(pbfPath, cacheName, response, step))
                        break;

            LogService.Begin("Distinct & sort missed ids");
            response.MissedNodeIds = response.MissedNodeIds.Distinct().OrderBy(i => i).ToList();
            response.MissedWayIds = response.MissedWayIds.Distinct().OrderBy(i => i).ToList();
            response.MissedRelationIds = response.MissedRelationIds.Distinct().OrderBy(i => i).ToList();
            LogService.End("Distinct & sort missed ids completed");

            LogService.LogInfo(
                $"Loaded: {response.Nodes.Count} nodes, {response.Ways.Count} ways, {response.Relations.Count} relations");

            if (response.MissedNodeIds.Count > 0 || response.MissedWayIds.Count > 0 || response.MissedRelationIds.Count > 0)
                LogService.LogWarning(
                    $"Missed: {response.MissedNodeIds.Count} nodes, " +
                    $"{response.MissedWayIds.Count} ways, " +
                    $"{response.MissedRelationIds.Count} relations");

            JsonFileClient.Write(FullCachePath(cacheName), response);
            var objects = BuildObjects(response);
            LogService.EndInfo("Load OSM objects completed");
            return objects;
        }

        private static bool FillNested(string pbfPath, string cacheName, OsmResponse response, int step)
        {
            var cacheStepPath = StepCachePath(cacheName, step);
            OsmResponse newResponse;

            if (FileClient.Exists(cacheStepPath))
                newResponse = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
                LogService.Begin("Calculate next request");

                var nodes = response.Nodes;
                var ways = response.Ways;
                var relations = response.Relations;
                var missedNodeIds = new HashSet<long>(response.MissedNodeIds);
                var missedWayIds = new HashSet<long>(response.MissedWayIds);
                var missedRelationIds = new HashSet<long>(response.MissedRelationIds);

                var wayNodeIds = response.Ways.Values.SelectMany(i => i.MissedNodeIds);
                var relMembers = response.Relations.Values.SelectMany(i => i.MissedMembers).ToList();
                var relNodeIds = relMembers.Where(i => i.Type == OsmGeoType.Node).Select(i => i.Id);
                var relWayIds = relMembers.Where(i => i.Type == OsmGeoType.Way).Select(i => i.Id);
                var relRelIds = relMembers.Where(i => i.Type == OsmGeoType.Relation).Select(i => i.Id);

                var needNodeIds = wayNodeIds.Concat(relNodeIds).Distinct()
                                            .Where(i => !nodes.ContainsKey(i) &&
                                                        !missedNodeIds.Contains(i)).ToList();

                var needWayIds = relWayIds.Distinct()
                                          .Where(i => !ways.ContainsKey(i) &&
                                                      !missedWayIds.Contains(i)).ToList();

                var needRelIds = relRelIds.Distinct()
                                          .Where(i => !relations.ContainsKey(i) &&
                                                      !missedRelationIds.Contains(i)).ToList();

                LogService.End("Calculate next request completed");

                if (needNodeIds.Count == 0 && needWayIds.Count == 0 && needRelIds.Count == 0)
                    return false;

                var newRequest = new OsmRequest { NodeIds = needNodeIds, WayIds = needWayIds, RelationIds = needRelIds };

                LogService.LogInfo($"Step {step}");
                newResponse = Load(pbfPath, newRequest);
                JsonFileClient.Write(cacheStepPath, newResponse);
            }

            LogService.Begin("Merge data");
            response.Nodes.AddRange(newResponse.Nodes);
            response.Ways.AddRange(newResponse.Ways);
            response.Relations.AddRange(newResponse.Relations);
            response.MissedNodeIds.AddRange(newResponse.MissedNodeIds);
            response.MissedWayIds.AddRange(newResponse.MissedWayIds);
            response.MissedRelationIds.AddRange(newResponse.MissedRelationIds);
            LogService.End("Merge data completed");

            return true;
        }

        private static OsmObjectResponse BuildObjects(OsmResponse response)
        {
            LogService.Begin("Build geo objects");
            var allNodes = response.Nodes;
            var allWays = response.Ways;
            var allRelations = response.Relations;

            foreach (var way in allWays.Values)
            {
                var nodes = way.MissedNodeIds.Where(allNodes.ContainsKey).Select(i => allNodes[i]).ToList();
                way.FillNodes(nodes);
            }

            foreach (var relation in allRelations.Values)
            {
                var members = new List<RelationMemberObject>();

                foreach (var memberInfo in relation.MissedMembers)
                    switch (memberInfo.Type)
                    {
                        case OsmGeoType.Node:

                            if (allNodes.ContainsKey(memberInfo.Id))
                                members.Add(new RelationMemberObject(memberInfo.Role, allNodes[memberInfo.Id]));

                            break;

                        case OsmGeoType.Way:

                            if (allWays.ContainsKey(memberInfo.Id))
                                members.Add(new RelationMemberObject(memberInfo.Role, allWays[memberInfo.Id]));

                            break;

                        case OsmGeoType.Relation:

                            if (allRelations.ContainsKey(memberInfo.Id))
                                members.Add(new RelationMemberObject(memberInfo.Role, allRelations[memberInfo.Id]));

                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(OsmGeoType));
                    }

                allRelations[relation.Id].FillMembers(members);
            }

            var waysNodeIds = allWays.Values.SelectMany(i => i.Nodes).Select(i => i.Id);
            var relationsNodeIds = allRelations.Values.SelectMany(i => i.Members.Nodes()).Select(i => i.Id);
            var childNodeIds = new HashSet<long>(waysNodeIds.Concat(relationsNodeIds));
            var childWayIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Ways()).Select(i => i.Id));
            var childRelationIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Relations()).Select(i => i.Id));

            var rootNodes = allNodes.Values.Where(i => !childNodeIds.Contains(i.Id)).ToDictionary(i => i.Id);
            var rootWays = allWays.Values.Where(i => !childWayIds.Contains(i.Id)).ToDictionary(i => i.Id);
            var rootRelations = allRelations.Values.Where(i => !childRelationIds.Contains(i.Id)).ToDictionary(i => i.Id);

            var objects = new OsmObjectResponse
            {
                RootNodes = rootNodes,
                RootWays = rootWays,
                RootRelations = rootRelations,
                MissedNodeIds = response.MissedNodeIds,
                MissedWayIds = response.MissedWayIds,
                MissedRelationIds = response.MissedRelationIds
            };

            LogService.End("Build geo objects completed");
            return objects;
        }

        private static string FullCachePath(string cacheName) => $"$osm-cache/{cacheName}.json";

        private static string StepCachePath(string cacheName, int step) => $"$osm-cache/{cacheName} - step {step}.json";

        #endregion
    }
}
