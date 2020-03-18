﻿using Kit;
using OsmDataKit.Internal;
using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OsmDataKit
{
    public static class OsmService
    {
        private static readonly string BaseDirectory =
            Environment.GetEnvironmentVariable("VisualStudioDir") != null
                ? PathHelper.Combine(Environment.CurrentDirectory, "../../..")
                : Environment.CurrentDirectory;

        public static string CacheDir { get; set; } = BaseDirectory + "/$work/$osm-cache";

        #region Validate

        public static void ValidateSource(string pbfPath)
        {
            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            LogService.BeginInfo($"Validate OSM source: {pbfPath}");
            var previousType = OsmGeoType.Node;
            long? previousId = 0;
            long thisCount = 0;
            long totalCount = 0;
            long noIdCount = 0;

            using (var fileStream = File.OpenRead(pbfPath))
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

            LogService.EndInfo($"Validate OSM source completed");
        }

        #endregion

        #region Load objects

        public static GeoObjectSet LoadObjects(string pbfPath, Func<OsmGeo, bool> filter) =>
            new GeoObjectSet(Load(pbfPath, filter, loadAllRelations: false));

        private static GeoContext Load(string pbfPath, Func<OsmGeo, bool> filter, bool loadAllRelations)
        {
            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            LogService.BeginInfo("Load OSM objects");
            var foundNodes = new Dictionary<long, NodeObject>();
            var foundWays = new Dictionary<long, WayObject>();
            var foundRelations = new Dictionary<long, RelationObject>();
            var allRelations = loadAllRelations ? new Dictionary<long, RelationObject>() : null;

            using (var fileStream = File.OpenRead(pbfPath))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var osmGeo in source)
                    switch (osmGeo.Type)
                    {
                        case OsmGeoType.Node:

                            if (filter(osmGeo))
                                foundNodes.Add(osmGeo.Id.Value, new NodeObject(osmGeo as Node));

                            break;

                        case OsmGeoType.Way:

                            if (filter(osmGeo))
                                foundWays.Add(osmGeo.Id.Value, new WayObject(osmGeo as Way));

                            break;

                        case OsmGeoType.Relation:
                            RelationObject relation = null;

                            if (loadAllRelations)
                                allRelations.Add(osmGeo.Id.Value, relation = new RelationObject(osmGeo as Relation));

                            if (filter(osmGeo))
                                foundRelations.Add(osmGeo.Id.Value, relation ?? new RelationObject(osmGeo as Relation));

                            break;

                        default:
                            throw new InvalidOperationException();
                    }
            }

            LogService.LogInfo($"Loaded: {foundNodes.Count} nodes, {foundWays.Count} ways, {foundRelations.Count} relations");
            LogService.EndInfo("Load OSM objects completed");
            return new GeoContext { Nodes = foundNodes, Ways = foundWays, Relations = foundRelations, AllRelations = allRelations };
        }

        public static GeoObjectSet LoadObjects(string pbfPath, GeoRequest request) =>
            new GeoObjectSet(Load(pbfPath, request, loadAllRelations: false));

        private static GeoContext Load(string pbfPath, GeoRequest request, bool loadAllRelations)
        {
            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            LogService.BeginInfo("Load OSM objects");

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
            var allRelations = loadAllRelations ? new Dictionary<long, RelationObject>() : null;
            List<long> missedNodeIds = null;
            List<long> missedWayIds = null;
            List<long> missedRelationIds = null;
            string logMessage;

            using (var fileStream = File.OpenRead(pbfPath))
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
                                id = osmGeo.Id.Value;

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

                            if (requestRelationIds.Count > 0 || loadAllRelations)
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
                                id = osmGeo.Id.Value;

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

                            if (requestRelationIds.Count > 0 || loadAllRelations)
                            {
                                thisType = OsmGeoType.Relation;
                                continue;
                            }

                            goto Complete;

                        case OsmGeoType.Relation:

                            if (osmGeo.Type < thisType)
                                continue;

                            id = osmGeo.Id.Value;
                            RelationObject relation = null;

                            if (loadAllRelations)
                                allRelations.Add(id, relation = new RelationObject(osmGeo as Relation));

                            if (requestRelationIds.Contains(id))
                            {
                                foundRelations.Add(id, relation ?? new RelationObject(osmGeo as Relation));

                                if (!loadAllRelations && foundRelations.Count == requestRelationIds.Count)
                                    goto Complete;
                            }

                            continue;

                        default:
                            throw new InvalidOperationException();
                    }
            }

            Complete:

            missedRelationIds = requestRelationIds.Where(i => !foundRelations.ContainsKey(i)).OrderBy(i => i).ToList();
            logMessage = $"Loaded {foundRelations.Count} relations";

            if (missedRelationIds.Count == 0)
                LogService.LogInfo(logMessage);
            else
                LogService.LogWarning($"{logMessage} ({missedRelationIds.Count} missed)");

            LogService.EndInfo("Load OSM objects completed");

            return new GeoContext
            {
                Nodes = foundNodes,
                Ways = foundWays,
                Relations = foundRelations,
                MissedNodeIds = missedNodeIds ?? new List<long>(0),
                MissedWayIds = missedWayIds ?? new List<long>(0),
                MissedRelationIds = missedRelationIds ?? new List<long>(0),
                AllRelations = allRelations
            };
        }

        #endregion

        #region Load complete objects

        public static CompleteGeoObjects LoadCompleteObjects(
            string pbfPath, string cacheName, GeoRequest request, int stepLimit = 0, bool lowMemoryMode = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return LoadComplete(pbfPath, cacheName, request, filter: null, stepLimit, loadAllRelations: !lowMemoryMode);
        }

        public static CompleteGeoObjects LoadCompleteObjects(
            string pbfPath, string cacheName, Func<OsmGeo, bool> filter, int stepLimit = 0, bool lowMemoryMode = false)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return LoadComplete(pbfPath, cacheName, request: null, filter, stepLimit, loadAllRelations: !lowMemoryMode);
        }

        private static CompleteGeoObjects LoadComplete(
            string pbfPath, string cacheName, GeoRequest request, Func<OsmGeo, bool> filter, int stepLimit, bool loadAllRelations)
        {
            if (pbfPath == null)
                throw new ArgumentNullException(nameof(pbfPath));

            if (cacheName.IsNullOrWhiteSpace())
                throw new ArgumentNullOrWhiteSpaceException(nameof(cacheName));

            if (stepLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(stepLimit));

            var cacheFullPath = CacheFullPath(cacheName);
            GeoContext context;

            if (File.Exists(cacheFullPath))
            {
                context = JsonFile.Read<GeoContext>(cacheFullPath);
                return CompleteObjects(context);
            }

            if (!File.Exists(pbfPath))
                throw new InvalidOperationException();

            LogService.BeginInfo("Load OSM complete objects" + (loadAllRelations ? string.Empty : " (low memory mode)"));
            var cacheStepPath = CacheStepPath(cacheName, 1);
            var doneSteps = 1;

            if (File.Exists(cacheStepPath))
                context = JsonFile.Read<GeoContext>(cacheStepPath);
            else
            {
                LogService.LogInfo($"Step 1");

                if (request != null)
                    context = Load(pbfPath, request, loadAllRelations);
                else
                if (filter != null)
                    context = Load(pbfPath, filter, loadAllRelations);
                else
                    throw new InvalidOperationException();

                if (loadAllRelations)
                    FilterAllRelations(context);

                JsonFile.Write(cacheStepPath, context);
            }

            if (stepLimit != 1)
                for (var step = 2; stepLimit == 0 || step <= stepLimit; step++)
                {
                    var needNextStep = LoadStep(pbfPath, cacheName, context, step);
                    doneSteps = step;

                    if (!needNextStep)
                        break;
                }

            LogService.Begin("Sort missed ids");
            context.MissedNodeIds = context.MissedNodeIds.Distinct().OrderBy(i => i).ToList();
            context.MissedWayIds = context.MissedWayIds.Distinct().OrderBy(i => i).ToList();
            context.MissedRelationIds = context.MissedRelationIds.Distinct().OrderBy(i => i).ToList();
            LogService.End("Sort missed ids completed");

            LogService.LogInfo(
                $"Loaded: {context.Nodes.Count} nodes, {context.Ways.Count} ways, {context.Relations.Count} relations");

            if (context.MissedNodeIds.Count > 0 || context.MissedWayIds.Count > 0 || context.MissedRelationIds.Count > 0)
                LogService.LogWarning(
                    $"Missed: {context.MissedNodeIds.Count} nodes, " +
                    $"{context.MissedWayIds.Count} ways, " +
                    $"{context.MissedRelationIds.Count} relations");

            JsonFile.Write(cacheFullPath, context);

            if (!File.Exists(cacheFullPath))
                throw new InvalidOperationException();

            for (var i = 1; i <= doneSteps; i++)
                File.Delete(CacheStepPath(cacheName, i));

            var completeGeos = CompleteObjects(context);
            LogService.EndInfo("Load OSM complete objects completed");
            return completeGeos;
        }

        private static void FilterAllRelations(GeoContext context)
        {
            Debug.Assert(context.AllRelations != null);

            LogService.Begin("Filter all relations");
            var relations = context.Relations;
            var missedRelationIds = new HashSet<long>(context.MissedRelationIds);
            var allRelations = context.AllRelations;

            void proceedRelationId(long relationId)
            {
                if (relations.ContainsKey(relationId) || missedRelationIds.Contains(relationId))
                    return;

                if (allRelations.TryGetValue(relationId, out var relation))
                {
                    relations.Add(relation.Id, relation);

                    var memberRelationIds = relation.MissedMembers.Where(i => i.Type == OsmGeoType.Relation)
                                                                  .Select(i => i.Id);

                    foreach (var memberRelationId in memberRelationIds)
                        proceedRelationId(memberRelationId);
                }
                else
                    missedRelationIds.Add(relationId);
            }

            var allMemberRelationIds = relations.Values.SelectMany(i => i.MissedMembers)
                                                       .Where(i => i.Type == OsmGeoType.Relation)
                                                       .Select(i => i.Id)
                                                       .Distinct();

            foreach (var memberRelationId in allMemberRelationIds)
                proceedRelationId(memberRelationId);

            context.AllRelations = null;
            LogService.End("Filter all relations completed");
        }

        private static bool LoadStep(string pbfPath, string cacheName, GeoContext context, int step)
        {
            var cacheStepPath = CacheStepPath(cacheName, step);
            GeoContext newContext;

            if (File.Exists(cacheStepPath))
                newContext = JsonFile.Read<GeoContext>(cacheStepPath);
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
                var newRequest = new GeoRequest { NodeIds = needNodeIds, WayIds = needWayIds, RelationIds = needRelIds };
                newContext = Load(pbfPath, newRequest, loadAllRelations: false);
                JsonFile.Write(cacheStepPath, newContext);
            }

            LogService.Begin("Merge data");
            context.Nodes.AddRange(newContext.Nodes);
            context.Ways.AddRange(newContext.Ways);
            context.Relations.AddRange(newContext.Relations);
            context.MissedNodeIds.AddRange(newContext.MissedNodeIds);
            context.MissedWayIds.AddRange(newContext.MissedWayIds);
            context.MissedRelationIds.AddRange(newContext.MissedRelationIds);
            LogService.End("Merge data completed");

            return true;
        }

        private static CompleteGeoObjects CompleteObjects(GeoContext context)
        {
            LogService.Begin("Build complete objects");
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
                                members.Add(new RelationMemberObject(allNodes[memberInfo.Id], memberInfo.Role));

                            break;

                        case OsmGeoType.Way:

                            if (allWays.ContainsKey(memberInfo.Id))
                                members.Add(new RelationMemberObject(allWays[memberInfo.Id], memberInfo.Role));

                            break;

                        case OsmGeoType.Relation:

                            if (allRelations.ContainsKey(memberInfo.Id))
                                members.Add(new RelationMemberObject(allRelations[memberInfo.Id], memberInfo.Role));

                            break;

                        default:
                            throw new InvalidOperationException();
                    }

                allRelations[relation.Id].FillMembers(members);
            }

            var waysNodeIds = allWays.Values.SelectMany(i => i.Nodes).Select(i => i.Id);
            var relationsNodeIds = allRelations.Values.SelectMany(i => i.Members.Nodes()).Select(i => i.Id);
            var childNodeIds = new HashSet<long>(waysNodeIds.Concat(relationsNodeIds));
            var childWayIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Ways()).Select(i => i.Id));
            var childRelationIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Relations()).Select(i => i.Id));

            var rootNodes = allNodes.Values.Where(i => !childNodeIds.Contains(i.Id)).ToList();
            var rootWays = allWays.Values.Where(i => !childWayIds.Contains(i.Id)).ToList();
            var rootRelations = allRelations.Values.Where(i => !childRelationIds.Contains(i.Id)).ToList();

            var completeGeos = new CompleteGeoObjects
            {
                RootNodes = rootNodes,
                RootWays = rootWays,
                RootRelations = rootRelations,
                MissedNodeIds = context.MissedNodeIds,
                MissedWayIds = context.MissedWayIds,
                MissedRelationIds = context.MissedRelationIds
            };

            LogService.End("Build complete objects completed");
            return completeGeos;
        }

        private static string CacheFullPath(string cacheName) => $"{CacheDir}/{cacheName}.json";

        private static string CacheStepPath(string cacheName, int step) => $"{CacheDir}/{cacheName} - step {step}.json";

        #endregion
    }
}
