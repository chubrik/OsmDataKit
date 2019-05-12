using Kit;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public static class OsmObjectService
    {
        public static RelationObject LoadRelationObject(string path, string cacheName, long relationId, int stepLimit = 0)
        {
            Debug.Assert(path != null);
            Debug.Assert(!cacheName.IsNullOrEmpty());

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (cacheName.IsNullOrEmpty())
                throw new ArgumentException(nameof(cacheName));

            var cachePath = FullCachePath(cacheName);
            OsmResponse response;
            OsmObjectResponse objects;

            if (FileClient.Exists(cachePath))
            {
                response = JsonFileClient.Read<OsmResponse>(cachePath);
                objects = BuildObjects(response);
                goto Result;
            }

            if (!FileClient.Exists(path))
                throw new InvalidOperationException();

            LogService.LogInfo("Load OSM response data");
            var cacheStepPath = StepCachePath(cacheName, 1);

            if (FileClient.Exists(cacheStepPath))
                response = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
                LogService.LogInfo($"Step 1");
                var request = new OsmRequest { RelationIds = new List<long> { relationId } };
                response = OsmService.Load(path, request);
                JsonFileClient.Write(cacheStepPath, response);
            }

            objects = LoadSteps(path, cacheName, response, stepLimit);

            Result:

            if (objects.Relations.TryGetValue(relationId, out var relation))
                return relation;

            if (objects.BrokenRelations.TryGetValue(relationId, out var brokenRelation))
                return brokenRelation;

            return null;
        }

        public static OsmObjectResponse LoadObjects(string path, string cacheName, Func<OsmGeo, bool> predicate, int stepLimit = 0)
        {
            Debug.Assert(path != null);
            Debug.Assert(!cacheName.IsNullOrEmpty());
            Debug.Assert(predicate != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (cacheName.IsNullOrEmpty())
                throw new ArgumentException(nameof(cacheName));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var cachePath = FullCachePath(cacheName);
            OsmResponse response;

            if (FileClient.Exists(cachePath))
            {
                response = JsonFileClient.Read<OsmResponse>(cachePath);
                return BuildObjects(response);
            }

            if (!FileClient.Exists(path))
                throw new InvalidOperationException();

            LogService.LogInfo("Load OSM response data");
            var cacheStepPath = StepCachePath(cacheName, 1);

            if (FileClient.Exists(cacheStepPath))
                response = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
                LogService.LogInfo($"Step 1");
                response = OsmService.Load(path, predicate);
                JsonFileClient.Write(cacheStepPath, response);
            }

            return LoadSteps(path, cacheName, response, stepLimit);
        }

        private static OsmObjectResponse LoadSteps(string path, string cacheName, OsmResponse response, int stepLimit)
        {
            if (stepLimit != 1)
                for (var step = 2; stepLimit == 0 || step <= stepLimit; step++)
                    if (!FillNested(path, cacheName, response, step))
                        break;

            response.MissedNodeIds = response.MissedNodeIds.Distinct().OrderBy(i => i).ToList();
            response.MissedWayIds = response.MissedWayIds.Distinct().OrderBy(i => i).ToList();
            response.MissedRelationIds = response.MissedRelationIds.Distinct().OrderBy(i => i).ToList();

            LogService.LogInfo(
                $"Loaded: {response.Nodes.Count} nodes, {response.Ways.Count} ways, {response.Relations.Count} relations");

            if (response.MissedNodeIds.Count > 0 || response.MissedWayIds.Count > 0 || response.MissedRelationIds.Count > 0)
                LogService.LogWarning(
                    $"Missed: {response.MissedNodeIds.Count} nodes, " +
                    $"{response.MissedWayIds.Count} ways, " +
                    $"{response.MissedRelationIds.Count} relations");

            JsonFileClient.Write(FullCachePath(cacheName), response);
            var objects = BuildObjects(response);
            LogService.LogInfo("Load OSM response data complete");
            return objects;
        }

        private static bool FillNested(string path, string cacheName, OsmResponse response, int step)
        {
            var cacheStepPath = StepCachePath(cacheName, step);
            OsmResponse newResponse;

            if (FileClient.Exists(cacheStepPath))
                newResponse = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
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

                if (needNodeIds.Count == 0 && needWayIds.Count == 0 && needRelIds.Count == 0)
                    return false;

                var newRequest = new OsmRequest { NodeIds = needNodeIds, WayIds = needWayIds, RelationIds = needRelIds };

                LogService.LogInfo($"Step {step}");
                newResponse = OsmService.Load(path, newRequest);
                JsonFileClient.Write(cacheStepPath, newResponse);
            }

            response.Nodes.AddRange(newResponse.Nodes);
            response.Ways.AddRange(newResponse.Ways);
            response.Relations.AddRange(newResponse.Relations);
            response.MissedNodeIds.AddRange(newResponse.MissedNodeIds);
            response.MissedWayIds.AddRange(newResponse.MissedWayIds);
            response.MissedRelationIds.AddRange(newResponse.MissedRelationIds);
            return true;
        }

        private static OsmObjectResponse BuildObjects(OsmResponse response)
        {
            LogService.Log("Build geo objects");
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
                            throw new ArgumentOutOfRangeException(nameof(memberInfo.Type));
                    }

                allRelations[relation.Id].FillMembers(members);
            }

            var wayNodeIds = allWays.Values.SelectMany(i => i.Nodes).Select(i => i.Id);
            var relNodeIds = allRelations.Values.SelectMany(i => i.Members.Nodes()).Select(i => i.Id);
            var childNodeIds = new HashSet<long>(wayNodeIds.Concat(relNodeIds));
            var childWayIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Ways()).Select(i => i.Id));
            var childRelationIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Relations()).Select(i => i.Id));

            var rootNodes = allNodes.Values.Where(i => !childNodeIds.Contains(i.Id)).ToDictionary(i => i.Id);
            var rootWays = allWays.Values.Where(i => !childWayIds.Contains(i.Id));
            var rootRelations = allRelations.Values.Where(i => !childRelationIds.Contains(i.Id));

            var completedWays = new Dictionary<long, WayObject>();
            var brokenWays = new Dictionary<long, WayObject>();

            foreach (var rootWay in rootWays)
                if (!rootWay.HasMissedNodes)
                    completedWays.Add(rootWay.Id, rootWay);
                else
                    brokenWays.Add(rootWay.Id, rootWay);

            var completedRelations = new Dictionary<long, RelationObject>();
            var brokenRelations = new Dictionary<long, RelationObject>();

            foreach (var rootRelation in rootRelations)
                if (!rootRelation.HasMissedParts())
                    completedRelations.Add(rootRelation.Id, rootRelation);
                else
                    brokenRelations.Add(rootRelation.Id, rootRelation);

            return new OsmObjectResponse
            {
                Nodes = rootNodes,
                Ways = completedWays,
                Relations = completedRelations,
                BrokenWays = brokenWays,
                BrokenRelations = brokenRelations,
                MissedNodeIds = response.MissedNodeIds,
                MissedWayIds = response.MissedWayIds,
                MissedRelationIds = response.MissedRelationIds
            };
        }

        private static string FullCachePath(string cacheName) => $"$osm-cache/{cacheName}.json";

        private static string StepCachePath(string cacheName, int step) => $"$osm-cache/{cacheName} - step {step}.json";
    }
}
