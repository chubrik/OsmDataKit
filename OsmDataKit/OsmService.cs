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
            long thisCount = 0;
            long totalCount = 0;
            long noIdCount = 0;

            using (var fileStream = FileClient.OpenRead(pbfPath))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var osmGeo in source)
                {
                    if (osmGeo.Type > previousType)
                    {
                        LogService.LogInfo($"Found {thisCount} {previousType.ToString().ToLower()}s");
                        totalCount += thisCount;
                        thisCount = 0;
                        previousType = osmGeo.Type;
                        previousId = 0;
                    }

                    thisCount++;
                    var id = osmGeo.Id;

                    if (id != null)
                    {
                        if (osmGeo.Type < previousType || id < previousId)
                            LogService.LogWarning($"Was: {previousType}-{previousId}, now: {osmGeo.Type}-{id}");

                        previousId = id;
                    }
                    else
                        noIdCount++;
                }
            }

            totalCount += thisCount;
            LogService.LogInfo($"Found {thisCount} {previousType.ToString().ToLower()}s");
            LogService.LogInfo($"Total {totalCount} entries");

            if (noIdCount > 0)
                LogService.LogWarning($"{noIdCount} entries has no id");

            LogService.EndInfo($"OSM validation completed");
        }

        #endregion

        #region Load

        public static OsmResponse Load(string pbfPath, Func<OsmGeo, bool> filter)
        {
            Debug.Assert(pbfPath != null);
            Debug.Assert(filter != null);

            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            LogService.BeginInfo("Load OSM data");
            var foundNodes = new Dictionary<long, NodeObject>();
            var foundWays = new Dictionary<long, WayObject>();
            var foundRelations = new Dictionary<long, RelationObject>();

            using (var fileStream = FileClient.OpenRead(pbfPath))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var osmGeo in source.Where(filter))
                    switch (osmGeo.Type)
                    {
                        case OsmGeoType.Node:
                            foundNodes.Add(osmGeo.Id.GetValueOrDefault(), new NodeObject(osmGeo as Node));
                            break;

                        case OsmGeoType.Way:
                            foundWays.Add(osmGeo.Id.GetValueOrDefault(), new WayObject(osmGeo as Way));
                            break;

                        case OsmGeoType.Relation:
                            foundRelations.Add(osmGeo.Id.GetValueOrDefault(), new RelationObject(osmGeo as Relation));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(OsmGeoType));
                    }
            }

            LogService.LogInfo($"Loaded: {foundNodes.Count} nodes, {foundWays.Count} ways, {foundRelations.Count} relations");
            LogService.EndInfo("Load OSM data completed");
            return new OsmResponse { Nodes = foundNodes, Ways = foundWays, Relations = foundRelations };
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

            return LoadObjects(pbfPath, cacheName, request, filter: null, stepLimit);
        }

        public static OsmObjectResponse LoadObjects(string pbfPath, string cacheName, Func<OsmGeo, bool> filter, int stepLimit = 0)
        {
            Debug.Assert(filter != null);

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return LoadObjects(pbfPath, cacheName, request: null, filter, stepLimit);
        }

        private static OsmObjectResponse LoadObjects(
            string pbfPath, string cacheName, OsmRequest request, Func<OsmGeo, bool> filter, int stepLimit)
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
            OsmResponse context;

            if (FileClient.Exists(cachePath))
            {
                context = JsonFileClient.Read<OsmResponse>(cachePath);
                return BuildObjects(context);
            }

            if (!FileClient.Exists(pbfPath))
                throw new InvalidOperationException();

            LogService.BeginInfo("Load OSM objects");
            var cacheStepPath = StepCachePath(cacheName, 1);

            if (FileClient.Exists(cacheStepPath))
                context = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
                LogService.LogInfo($"Step 1");

                if (request != null)
                    context = Load(pbfPath, request);
                else
                if (filter != null)
                    context = Load(pbfPath, filter);
                else
                    throw new InvalidOperationException();

                JsonFileClient.Write(cacheStepPath, context);
            }

            if (stepLimit != 1)
                for (var step = 2; stepLimit == 0 || step <= stepLimit; step++)
                    if (!LoadStep(pbfPath, cacheName, context, step))
                        break;

            LogService.Begin("Distinct & sort missed ids");
            context.MissedNodeIds = context.MissedNodeIds.Distinct().OrderBy(i => i).ToList();
            context.MissedWayIds = context.MissedWayIds.Distinct().OrderBy(i => i).ToList();
            context.MissedRelationIds = context.MissedRelationIds.Distinct().OrderBy(i => i).ToList();
            LogService.End("Distinct & sort missed ids completed");

            LogService.LogInfo(
                $"Loaded: {context.Nodes.Count} nodes, {context.Ways.Count} ways, {context.Relations.Count} relations");

            if (context.MissedNodeIds.Count > 0 || context.MissedWayIds.Count > 0 || context.MissedRelationIds.Count > 0)
                LogService.LogWarning(
                    $"Missed: {context.MissedNodeIds.Count} nodes, " +
                    $"{context.MissedWayIds.Count} ways, " +
                    $"{context.MissedRelationIds.Count} relations");

            JsonFileClient.Write(FullCachePath(cacheName), context);
            var objects = BuildObjects(context);
            LogService.EndInfo("Load OSM objects completed");
            return objects;
        }

        private static bool LoadStep(string pbfPath, string cacheName, OsmResponse context, int step)
        {
            var cacheStepPath = StepCachePath(cacheName, step);
            OsmResponse response;

            if (FileClient.Exists(cacheStepPath))
                response = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
                LogService.Begin("Calculate next request");

                var nodes = context.Nodes;
                var ways = context.Ways;
                var relations = context.Relations;
                var missedNodeIds = new HashSet<long>(context.MissedNodeIds);
                var missedWayIds = new HashSet<long>(context.MissedWayIds);
                var missedRelationIds = new HashSet<long>(context.MissedRelationIds);

                var wayNodeIds = context.Ways.Values.SelectMany(i => i.MissedNodeIds);
                var relMembers = context.Relations.Values.SelectMany(i => i.MissedMembers).ToList();
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

                LogService.LogInfo($"Step {step}");
                response = Load(pbfPath, new OsmRequest { NodeIds = needNodeIds, WayIds = needWayIds, RelationIds = needRelIds });
                JsonFileClient.Write(cacheStepPath, response);
            }

            LogService.Begin("Merge data");
            context.Nodes.AddRange(response.Nodes);
            context.Ways.AddRange(response.Ways);
            context.Relations.AddRange(response.Relations);
            context.MissedNodeIds.AddRange(response.MissedNodeIds);
            context.MissedWayIds.AddRange(response.MissedWayIds);
            context.MissedRelationIds.AddRange(response.MissedRelationIds);
            LogService.End("Merge data completed");

            return true;
        }

        private static OsmObjectResponse BuildObjects(OsmResponse context)
        {
            LogService.Begin("Build geo objects");
            var allNodes = context.Nodes;
            var allWays = context.Ways;
            var allRelations = context.Relations;

            foreach (var way in allWays.Values)
            {
                var wayNodes = way.MissedNodeIds.Where(allNodes.ContainsKey).Select(i => allNodes[i]).ToList();
                way.FillNodes(wayNodes);
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
                MissedNodeIds = context.MissedNodeIds,
                MissedWayIds = context.MissedWayIds,
                MissedRelationIds = context.MissedRelationIds
            };

            LogService.End("Build geo objects completed");
            return objects;
        }

        private static string FullCachePath(string cacheName) => $"$osm-cache/{cacheName}.json";

        private static string StepCachePath(string cacheName, int step) => $"$osm-cache/{cacheName} - step {step}.json";

        #endregion
    }
}
