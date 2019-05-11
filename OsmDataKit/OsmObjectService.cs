using Kit;
using OsmDataKit.Data;
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

            if (FileClient.Exists(cachePath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cachePath);
                return BuildObjects(data).AllRelationsDict[relationId];
            }

            if (!FileClient.Exists(path))
                throw new InvalidOperationException();

            LogService.LogInfo("Load OSM response data");
            var cacheStepPath = StepCachePath(cacheName, 1);
            OsmResponse response;

            if (FileClient.Exists(cacheStepPath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cacheStepPath);
                response = DataConverter.ToOsm(data);
            }
            else
            {
                LogService.LogInfo($"Step 1");
                var request = new OsmRequest { RelationIds = new List<long> { relationId } };
                response = OsmService.Load(path, request);
                var data = DataConverter.ToData(response);
                JsonFileClient.Write(cacheStepPath, data);
            }

            return LoadSteps(path, cacheName, response, stepLimit).AllRelationsDict[relationId];
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

            if (FileClient.Exists(cachePath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cachePath);
                return BuildObjects(data);
            }

            if (!FileClient.Exists(path))
                throw new InvalidOperationException();

            LogService.LogInfo("Load OSM response data");
            var cacheStepPath = StepCachePath(cacheName, 1);
            OsmResponse response;

            if (FileClient.Exists(cacheStepPath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cacheStepPath);
                response = DataConverter.ToOsm(data);
            }
            else
            {
                LogService.LogInfo($"Step 1");
                response = OsmService.Load(path, predicate);
                var data = DataConverter.ToData(response);
                JsonFileClient.Write(cacheStepPath, data);
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

            var data = DataConverter.ToData(response);
            JsonFileClient.Write(FullCachePath(cacheName), data);
            var objects = BuildObjects(data);
            LogService.LogInfo("Load OSM response data complete");
            return objects;
        }

        private static bool FillNested(string path, string cacheName, OsmResponse response, int step)
        {
            OsmResponse newResponse;
            var cacheStepPath = StepCachePath(cacheName, step);

            if (FileClient.Exists(cacheStepPath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cacheStepPath);
                newResponse = DataConverter.ToOsm(data);
            }
            else
            {
                var wayNodeIds = response.Ways.Values.SelectMany(i => i.MissedNodeIds);
                var relMembers = response.Relations.Values.SelectMany(i => i.Members).ToList();
                var relNodeIds = relMembers.Where(i => i.Type == OsmGeoType.Node).Select(i => i.Id);
                var relWayIds = relMembers.Where(i => i.Type == OsmGeoType.Way).Select(i => i.Id);
                var relRelIds = relMembers.Where(i => i.Type == OsmGeoType.Relation).Select(i => i.Id);

                var nestedNodeIds = wayNodeIds.Concat(relNodeIds).Distinct()
                                              .Where(i => !response.Nodes.ContainsKey(i) &&
                                                          !response.MissedNodeIds.Contains(i)).ToList();

                var nestedWayIds = relWayIds.Distinct()
                                            .Where(i => !response.Ways.ContainsKey(i) &&
                                                        !response.MissedWayIds.Contains(i)).ToList();

                var nestedRelIds = relRelIds.Distinct()
                                            .Where(i => !response.Relations.ContainsKey(i) &&
                                                        !response.MissedRelationIds.Contains(i)).ToList();

                if (nestedNodeIds.Count == 0 && nestedWayIds.Count == 0 && nestedRelIds.Count == 0)
                    return false;

                var request = new OsmRequest { NodeIds = nestedNodeIds, WayIds = nestedWayIds, RelationIds = nestedRelIds };

                LogService.LogInfo($"Step {step}");
                newResponse = OsmService.Load(path, request);
                var newData = DataConverter.ToData(newResponse);
                JsonFileClient.Write(cacheStepPath, newData);
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
                way.SetNodes(nodes);
            }

            foreach (var relation in allRelations.Values)
            {
                var members = new List<RelationMemberObject>();

                foreach (var memberInfo in relation.MissedMembersInfo)
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

                allRelations[relation.Id].SetMembers(members);
            }

            var wayNodeIds = allWays.Values.SelectMany(i => i.Nodes).Select(i => i.Id);
            var relNodeIds = allRelations.Values.SelectMany(i => i.Members.Nodes()).Select(i => i.Id);
            var memberNodeIds = new HashSet<long>(wayNodeIds.Concat(relNodeIds));
            var memberWayIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Ways()).Select(i => i.Id));
            var memberRelationIds = new HashSet<long>(allRelations.Values.SelectMany(i => i.Members.Relations()).Select(i => i.Id));
            var rootNodes = allNodes.Values.Where(i => !memberNodeIds.Contains(i.Id)).ToList();
            var rootWays = allWays.Values.Where(i => !memberWayIds.Contains(i.Id)).ToList();
            var rootRelations = allRelations.Values.Where(i => !memberRelationIds.Contains(i.Id)).ToList();

            var validRootWays = rootWays.Where(i => !i.HasMissedNodes).ToList();
            var validRootRelations = rootRelations.Where(i => !i.HasMissedParts()).ToList();
            var brokenRootWays = rootWays.Where(i => i.HasMissedNodes).ToList();
            var brokenRootRelations = rootRelations.Where(i => i.HasMissedParts()).ToList();

            return new OsmObjectResponse
            {
                Nodes = rootNodes,
                Ways = validRootWays,
                Relations = validRootRelations,
                BrokenWays = brokenRootWays,
                BrokenRelations = brokenRootRelations
            };
        }

        private static string FullCachePath(string cacheName) => $"$osm-cache/{cacheName}.json";

        private static string StepCachePath(string cacheName, int step) => $"$osm-cache/{cacheName} - step {step}.json";
    }
}
