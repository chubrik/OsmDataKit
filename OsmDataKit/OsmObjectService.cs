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

            LogService.LogInfo("Load OSM response data");
            var cacheStepPath = StepCachePath(cacheName, 1);

            if (FileClient.Exists(cacheStepPath))
                response = JsonFileClient.Read<OsmResponse>(cacheStepPath);
            else
            {
                LogService.LogInfo($"Step 1");

                if (request != null)
                    response = OsmService.Load(pbfPath, request);
                else
                if (predicate != null)
                    response = OsmService.Load(pbfPath, predicate);
                else
                    throw new InvalidOperationException();

                JsonFileClient.Write(cacheStepPath, response);
            }

            return LoadSteps(pbfPath, cacheName, response, stepLimit);
        }

        private static OsmObjectResponse LoadSteps(string pbfPath, string cacheName, OsmResponse response, int stepLimit)
        {
            if (stepLimit != 1)
                for (var step = 2; stepLimit == 0 || step <= stepLimit; step++)
                    if (!FillNested(pbfPath, cacheName, response, step))
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

        private static bool FillNested(string pbfPath, string cacheName, OsmResponse response, int step)
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
                newResponse = OsmService.Load(pbfPath, newRequest);
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

            var waysNodeIds = allWays.Values.SelectMany(i => i.Nodes).Select(i => i.Id);
            var relationsNodeIds = allRelations.Values.SelectMany(i => i.Members.Nodes()).Select(i => i.Id);
            var childNodeIds = new HashSet<long>(waysNodeIds.Concat(relationsNodeIds));
            var childWayIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Ways()).Select(i => i.Id));
            var childRelationIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Relations()).Select(i => i.Id));

            var rootNodes = allNodes.Values.Where(i => !childNodeIds.Contains(i.Id)).ToDictionary(i => i.Id);
            var rootWays = allWays.Values.Where(i => !childWayIds.Contains(i.Id)).ToDictionary(i => i.Id);
            var rootRelations = allRelations.Values.Where(i => !childRelationIds.Contains(i.Id)).ToDictionary(i => i.Id);

            return new OsmObjectResponse
            {
                RootNodes = rootNodes,
                RootWays = rootWays,
                RootRelations = rootRelations,
                MissedNodeIds = response.MissedNodeIds,
                MissedWayIds = response.MissedWayIds,
                MissedRelationIds = response.MissedRelationIds
            };
        }

        private static string FullCachePath(string cacheName) => $"$osm-cache/{cacheName}.json";

        private static string StepCachePath(string cacheName, int step) => $"$osm-cache/{cacheName} - step {step}.json";
    }
}
