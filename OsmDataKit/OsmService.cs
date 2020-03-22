using Kit;
using OsmDataKit.Internal;
using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsmDataKit
{
    public static class OsmService
    {
        public static string CacheDirectory { get; set; } = Environment.CurrentDirectory + @"\$osm-cache";

        #region Validate

        public static void ValidateSource(string pbfPath) =>
            LogService.Log($"Validate OSM source \"{FileClient.LogPath(pbfPath)}\"", () =>
            {
                if (pbfPath == null)
                    throw new ArgumentNullException(nameof(pbfPath));

                var previousType = OsmGeoType.Node;
                long? previousId = 0;
                long thisCount = 0;
                long totalCount = 0;
                long noIdCount = 0;

                using (var fileStream = File.OpenRead(pbfPath))
                {
                    var osmSource = new PBFOsmStreamSource(fileStream);

                    foreach (var osmGeo in osmSource)
                    {
                        if (osmGeo.Type > previousType)
                        {
                            LogService.Log($"{thisCount} {previousType.ToString().ToLower()}s found");
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
                                LogService.Warning($"Was: {previousType}-{previousId}, now: {osmGeo.Type}-{id}");

                            previousId = id;
                        }
                        else
                            noIdCount++;
                    }
                }

                totalCount += thisCount;
                LogService.Log($"{thisCount} {previousType.ToString().ToLower()}s found");
                LogService.Log($"{totalCount} total entries");

                if (noIdCount > 0)
                    LogService.Warning($"{noIdCount} entries has no id");
            });

        #endregion

        #region Load objects

        public static GeoObjectSet LoadObjects(string pbfPath, Func<OsmGeo, bool> filter) =>
            new GeoObjectSet(Load(pbfPath, cacheName: null, filter, loadAllRelations: false));

        public static GeoObjectSet LoadObjects(string pbfPath, string cacheName, Func<OsmGeo, bool> filter)
        {
            if (cacheName.IsNullOrWhiteSpace())
                throw new ArgumentNullOrWhiteSpaceException(nameof(cacheName));

            return new GeoObjectSet(Load(pbfPath, cacheName, filter, loadAllRelations: false));
        }

        private static GeoContext Load(string pbfPath, string cacheName, Func<OsmGeo, bool> filter, bool loadAllRelations) =>
            LogService.Log($"Load geo objects \"{FileClient.LogPath(pbfPath)}\"", () =>
            {
                if (pbfPath == null)
                    throw new ArgumentNullException(nameof(pbfPath));

                if (filter == null)
                    throw new ArgumentNullException(nameof(filter));

                var useCache = cacheName != null;

                if (useCache && CacheProvider.Has(cacheName))
                    return CacheProvider.Get(cacheName);

                var loadedNodes = new Dictionary<long, NodeObject>();
                var loadedWays = new Dictionary<long, WayObject>();
                var loadedRelations = new Dictionary<long, RelationObject>();
                var allRelations = loadAllRelations ? new Dictionary<long, RelationObject>() : null;

                using (var fileStream = File.OpenRead(pbfPath))
                {
                    var source = new PBFOsmStreamSource(fileStream);

                    foreach (var osmGeo in source)
                        switch (osmGeo.Type)
                        {
                            case OsmGeoType.Node:

                                if (filter(osmGeo))
                                    loadedNodes.Add(osmGeo.Id.Value, new NodeObject(osmGeo as Node));

                                break;

                            case OsmGeoType.Way:

                                if (filter(osmGeo))
                                    loadedWays.Add(osmGeo.Id.Value, new WayObject(osmGeo as Way));

                                break;

                            case OsmGeoType.Relation:
                                RelationObject relation = null;

                                if (loadAllRelations)
                                    allRelations.Add(osmGeo.Id.Value, relation = new RelationObject(osmGeo as Relation));

                                if (filter(osmGeo))
                                    loadedRelations.Add(osmGeo.Id.Value, relation ?? new RelationObject(osmGeo as Relation));

                                break;

                            default:
                                throw new InvalidOperationException();
                        }
                }

                LogService.Log($"Loaded: {loadedNodes.Count} nodes, {loadedWays.Count} ways, {loadedRelations.Count} relations");

                var context = new GeoContext
                {
                    Nodes = loadedNodes,
                    Ways = loadedWays,
                    Relations = loadedRelations
                };

                if (loadAllRelations)
                    FilterAllRelations(context, allRelations);

                if (useCache)
                    CacheProvider.Put(cacheName, context);

                return context;
            });

        public static GeoObjectSet LoadObjects(string pbfPath, GeoRequest request) =>
            new GeoObjectSet(Load(pbfPath, cacheName: null, request, loadAllRelations: false));

        public static GeoObjectSet LoadObjects(string pbfPath, string cacheName, GeoRequest request)
        {
            if (cacheName.IsNullOrWhiteSpace())
                throw new ArgumentNullOrWhiteSpaceException(nameof(cacheName));

            return new GeoObjectSet(Load(pbfPath, cacheName, request, loadAllRelations: false));
        }

        private static GeoContext Load(string pbfPath, string cacheName, GeoRequest request, bool loadAllRelations) =>
            LogService.Log($"Load geo objects \"{FileClient.LogPath(pbfPath)}\"", () =>
            {
                if (pbfPath == null)
                    throw new ArgumentNullException(nameof(pbfPath));

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                var useCache = cacheName != null;

                if (useCache && CacheProvider.Has(cacheName))
                    return CacheProvider.Get(cacheName);

                var requestNodeIds = request.NodeIds != null
                    ? new HashSet<long>(request.NodeIds.Distinct())
                    : new HashSet<long>();

                var requestWayIds = request.WayIds != null
                    ? new HashSet<long>(request.WayIds.Distinct())
                    : new HashSet<long>();

                var requestRelationIds = request.RelationIds != null
                    ? new HashSet<long>(request.RelationIds.Distinct())
                    : new HashSet<long>();

                var loadedNodes = new Dictionary<long, NodeObject>();
                var loadedWays = new Dictionary<long, WayObject>();
                var loadedRelations = new Dictionary<long, RelationObject>();
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
                                        loadedNodes.Add(id, new NodeObject(osmGeo as Node));

                                        if (loadedNodes.Count < requestNodeIds.Count)
                                            continue;
                                    }
                                    else
                                        continue;
                                }

                                missedNodeIds = requestNodeIds.Where(i => !loadedNodes.ContainsKey(i)).OrderBy(i => i).ToList();
                                logMessage = $"{loadedNodes.Count} nodes loaded";

                                if (missedNodeIds.Count == 0)
                                    LogService.Log(logMessage);
                                else
                                    LogService.Warning($"{logMessage} ({missedNodeIds.Count} missed)");

                                if (requestWayIds.Count > 0)
                                {
                                    thisType = OsmGeoType.Way;
                                    continue;
                                }

                                LogService.Log("0 ways loaded");

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
                                        loadedWays.Add(id, new WayObject(osmGeo as Way));

                                        if (loadedWays.Count < requestWayIds.Count)
                                            continue;
                                    }
                                    else
                                        continue;
                                }

                                missedWayIds = requestWayIds.Where(i => !loadedWays.ContainsKey(i)).OrderBy(i => i).ToList();
                                logMessage = $"{loadedWays.Count} ways loaded";

                                if (missedWayIds.Count == 0)
                                    LogService.Log(logMessage);
                                else
                                    LogService.Warning($"{logMessage} ({missedWayIds.Count} missed)");

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
                                    loadedRelations.Add(id, relation ?? new RelationObject(osmGeo as Relation));

                                    if (!loadAllRelations && loadedRelations.Count == requestRelationIds.Count)
                                        goto Complete;
                                }

                                continue;

                            default:
                                throw new InvalidOperationException();
                        }
                }

                Complete:

                missedRelationIds = requestRelationIds.Where(i => !loadedRelations.ContainsKey(i)).OrderBy(i => i).ToList();
                logMessage = $"{loadedRelations.Count} relations loaded";

                if (missedRelationIds.Count == 0)
                    LogService.Log(logMessage);
                else
                    LogService.Warning($"{logMessage} ({missedRelationIds.Count} missed)");

                var context = new GeoContext
                {
                    Nodes = loadedNodes,
                    Ways = loadedWays,
                    Relations = loadedRelations,
                    MissedNodeIds = missedNodeIds ?? new List<long>(0),
                    MissedWayIds = missedWayIds ?? new List<long>(0),
                    MissedRelationIds = missedRelationIds ?? new List<long>(0)
                };

                if (loadAllRelations)
                    FilterAllRelations(context, allRelations);

                if (useCache)
                    CacheProvider.Put(cacheName, context);

                return context;
            });

        #endregion

        #region Load complete objects

        public static CompleteGeoObjects LoadCompleteObjects(
            string pbfPath, GeoRequest request, int? stepLimit = null, bool lowMemoryMode = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return LoadComplete(pbfPath, cacheName: null, request, filter: null, stepLimit, loadAllRelations: !lowMemoryMode);
        }

        public static CompleteGeoObjects LoadCompleteObjects(
            string pbfPath, string cacheName, GeoRequest request, int? stepLimit = null, bool lowMemoryMode = false)
        {
            if (cacheName.IsNullOrWhiteSpace())
                throw new ArgumentNullOrWhiteSpaceException(nameof(cacheName));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return LoadComplete(pbfPath, cacheName, request, filter: null, stepLimit, loadAllRelations: !lowMemoryMode);
        }

        public static CompleteGeoObjects LoadCompleteObjects(
            string pbfPath, Func<OsmGeo, bool> filter, int? stepLimit = null, bool lowMemoryMode = false)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return LoadComplete(pbfPath, cacheName: null, request: null, filter, stepLimit, loadAllRelations: !lowMemoryMode);
        }

        public static CompleteGeoObjects LoadCompleteObjects(
            string pbfPath, string cacheName, Func<OsmGeo, bool> filter, int? stepLimit = null, bool lowMemoryMode = false)
        {
            if (cacheName.IsNullOrWhiteSpace())
                throw new ArgumentNullOrWhiteSpaceException(nameof(cacheName));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return LoadComplete(pbfPath, cacheName, request: null, filter, stepLimit, loadAllRelations: !lowMemoryMode);
        }

        private static CompleteGeoObjects LoadComplete(
            string pbfPath, string cacheName, GeoRequest request, Func<OsmGeo, bool> filter, int? stepLimit, bool loadAllRelations) =>
            LogService.Log("Load complete geo objects" + (loadAllRelations ? "" : " (Low memory mode)"), () =>
            {
                if (pbfPath == null)
                    throw new ArgumentNullException(nameof(pbfPath));

                if (stepLimit <= 0)
                    throw new ArgumentOutOfRangeException(nameof(stepLimit));

                var useCache = cacheName != null;
                GeoContext context = null;

                if (useCache && CacheProvider.Has(cacheName))
                {
                    context = CacheProvider.Get(cacheName);
                    return CompleteObjects(context);
                }

                LogService.Log($"Step 1", () =>
                {
                    var cacheFirstStepName = CacheStepName(cacheName, 1);

                    if (request != null)
                        context = Load(pbfPath, cacheFirstStepName, request, loadAllRelations);
                    else
                    if (filter != null)
                        context = Load(pbfPath, cacheFirstStepName, filter, loadAllRelations);
                    else
                        throw new InvalidOperationException();
                });

                var doneSteps = 1;

                if (stepLimit != 1)
                    for (var step = 2; stepLimit == null || step <= stepLimit; step++)
                    {
                        var needNextStep = LoadStep(pbfPath, cacheName, context, step);
                        doneSteps = step;

                        if (!needNextStep)
                            break;
                    }

                Log.Debug("Sort missed ids", () =>
                {
                    context.MissedNodeIds = context.MissedNodeIds.Distinct().OrderBy(i => i).ToList();
                    context.MissedWayIds = context.MissedWayIds.Distinct().OrderBy(i => i).ToList();
                    context.MissedRelationIds = context.MissedRelationIds.Distinct().OrderBy(i => i).ToList();
                });

                LogService.Log(
                    $"Loaded: {context.Nodes.Count} nodes, " +
                    $"{context.Ways.Count} ways, " +
                    $"{context.Relations.Count} relations");

                if (context.MissedNodeIds.Count > 0 || context.MissedWayIds.Count > 0 || context.MissedRelationIds.Count > 0)
                    LogService.Warning(
                        $"Missed: {context.MissedNodeIds.Count} nodes, " +
                        $"{context.MissedWayIds.Count} ways, " +
                        $"{context.MissedRelationIds.Count} relations");

                if (useCache)
                {
                    CacheProvider.Put(cacheName, context);
                    LogService.Log($"Delete {doneSteps} extra cache files");

                    for (var i = 1; i <= doneSteps; i++)
                        CacheProvider.Delete(CacheStepName(cacheName, i));
                }

                return CompleteObjects(context);
            });

        private static void FilterAllRelations(GeoContext context, Dictionary<long, RelationObject> allRelations) =>
            Log.Debug("Filter all relations", () =>
            {
                var relations = context.Relations;
                var missedRelationIds = new HashSet<long>(context.MissedRelationIds);

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
                                                           .Distinct()
                                                           .ToList();

                foreach (var memberRelationId in allMemberRelationIds)
                    proceedRelationId(memberRelationId);
            });

        private static bool LoadStep(string pbfPath, string cacheName, GeoContext context, int step) =>
            LogService.Log($"Step {step}", () =>
            {
                var useCache = cacheName != null;
                var cacheStepName = CacheStepName(cacheName, step);
                GeoContext newContext;

                if (useCache && CacheProvider.Has(cacheStepName))
                    newContext = CacheProvider.Get(cacheStepName);
                else
                {
                    List<long> needNodeIds = null;
                    List<long> needWayIds = null;
                    List<long> needRelIds = null;

                    Log.Debug("Calculate request", () =>
                    {
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

                        needNodeIds = wayNodeIds.Concat(relNodeIds).Distinct()
                                                .Where(i => !nodes.ContainsKey(i) &&
                                                            !missedNodeIds.Contains(i)).ToList();

                        needWayIds = relWayIds.Distinct()
                                              .Where(i => !ways.ContainsKey(i) &&
                                                          !missedWayIds.Contains(i)).ToList();

                        needRelIds = relRelIds.Distinct()
                                              .Where(i => !relations.ContainsKey(i) &&
                                                          !missedRelationIds.Contains(i)).ToList();
                    });

                    if (needNodeIds.Count == 0 && needWayIds.Count == 0 && needRelIds.Count == 0)
                    {
                        LogService.Log("No load needed");
                        return false;
                    }

                    var newRequest = new GeoRequest { NodeIds = needNodeIds, WayIds = needWayIds, RelationIds = needRelIds };
                    newContext = Load(pbfPath, cacheStepName, newRequest, loadAllRelations: false);
                }

                Log.Debug("Merge data", () =>
                {
                    context.Nodes.AddRange(newContext.Nodes);
                    context.Ways.AddRange(newContext.Ways);
                    context.Relations.AddRange(newContext.Relations);
                    context.MissedNodeIds.AddRange(newContext.MissedNodeIds);
                    context.MissedWayIds.AddRange(newContext.MissedWayIds);
                    context.MissedRelationIds.AddRange(newContext.MissedRelationIds);
                });

                return true;
            });

        private static CompleteGeoObjects CompleteObjects(GeoContext context) =>
            LogService.Log("Build complete objects", () =>
            {
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

                return completeGeos;
            });

        #endregion

        private static string CacheStepName(string cacheName, int step) =>
            cacheName != null ? $"{cacheName} - step {step}" : null;
    }
}
