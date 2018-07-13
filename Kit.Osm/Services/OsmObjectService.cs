using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public static class OsmObjectService
    {
        public static OsmRelation LoadRelationObject(
            string path, string cacheName, long relationId, int stepLimit = 0)
        {
            Debug.Assert(path != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Debug.Assert(!cacheName.IsNullOrEmpty());

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

            LogService.Log("Load OSM response data");
            var cacheStepPath = StepCachePath(cacheName, 1);
            OsmResponse response;

            if (FileClient.Exists(cacheStepPath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cacheStepPath);
                response = new OsmResponse(data);
            }
            else
            {
                var request = new OsmRequest
                {
                    RelationIds = new List<long> { relationId }
                };

                LogService.Log($"Step 1");
                response = OsmService.Load(path, request);
                JsonFileClient.Write(cacheStepPath, response.ToData());
            }

            return LoadSteps(path, cacheName, response, stepLimit).AllRelationsDict[relationId];
        }

        public static OsmObjectResponse LoadObjects(
            string path, string cacheName, Func<OsmGeo, bool> predicate, int stepLimit = 0)
        {
            Debug.Assert(path != null);

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Debug.Assert(!cacheName.IsNullOrEmpty());

            if (cacheName.IsNullOrEmpty())
                throw new ArgumentException(nameof(cacheName));

            Debug.Assert(predicate != null);

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

            LogService.Log("Load OSM response data");
            var cacheStepPath = StepCachePath(cacheName, 1);
            OsmResponse response;

            if (FileClient.Exists(cacheStepPath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cacheStepPath);
                response = new OsmResponse(data);
            }
            else
            {
                LogService.Log($"Step 1");
                response = OsmService.Load(path, predicate);
                JsonFileClient.Write(cacheStepPath, response.ToData());
            }

            return LoadSteps(path, cacheName, response, stepLimit);
        }

        private static OsmObjectResponse LoadSteps(
            string path, string cacheName, OsmResponse response, int stepLimit)
        {
            if (stepLimit != 1)
                for (var step = 2; stepLimit == 0 || step <= stepLimit; step++)
                    if (!FillNested(path, cacheName, response, step))
                        break;

            response.MissedNodeIds =
                response.MissedNodeIds.Distinct().OrderBy(i => i).ToList();

            response.MissedWayIds =
                response.MissedWayIds.Distinct().OrderBy(i => i).ToList();

            response.MissedRelationIds =
                response.MissedRelationIds.Distinct().OrderBy(i => i).ToList();

            LogService.Log(
                $"Loaded: {response.Nodes.Count} nodes, " +
                $"{response.Ways.Count} ways, " +
                $"{response.Relations.Count} relations");

            if (response.MissedNodeIds.Count > 0 ||
                response.MissedWayIds.Count > 0 ||
                response.MissedRelationIds.Count > 0)
                LogService.LogWarning(
                    $"Missed: {response.MissedNodeIds.Count} nodes, " +
                    $"{response.MissedWayIds.Count} ways, " +
                    $"{response.MissedRelationIds.Count} relations");

            var data = response.ToData();
            JsonFileClient.Write(FullCachePath(cacheName), data);
            var objects = BuildObjects(data);
            LogService.Log("Load OSM response data complete");
            return objects;
        }

        private static bool FillNested(
            string path, string cacheName, OsmResponse response, int step)
        {
            OsmResponse newResponse;
            var cacheStepPath = StepCachePath(cacheName, step);

            if (FileClient.Exists(cacheStepPath))
            {
                var data = JsonFileClient.Read<OsmResponseData>(cacheStepPath);
                newResponse = new OsmResponse(data);
            }
            else
            {
                var wayNodeIds = response.Ways.Values.SelectMany(i => i.Nodes);
                var relMembers = response.Relations.Values.SelectMany(i => i.Members).ToList();

                var relNodeIds = relMembers.Where(i => i.Type == OsmGeoType.Node)
                                           .Select(i => i.Id);

                var relWayIds = relMembers.Where(i => i.Type == OsmGeoType.Way)
                                          .Select(i => i.Id);

                var relRelIds = relMembers.Where(i => i.Type == OsmGeoType.Relation)
                                          .Select(i => i.Id);

                var nestedNodeIds =
                    wayNodeIds.Concat(relNodeIds).Distinct()
                              .Where(i => !response.Nodes.ContainsKey(i) &&
                                          !response.MissedNodeIds.Contains(i)).ToList();

                var nestedWayIds =
                    relWayIds.Distinct()
                             .Where(i => !response.Ways.ContainsKey(i) &&
                                         !response.MissedWayIds.Contains(i)).ToList();

                var nestedRelIds =
                    relRelIds.Distinct()
                             .Where(i => !response.Relations.ContainsKey(i) &&
                                         !response.MissedRelationIds.Contains(i)).ToList();

                if (nestedNodeIds.Count == 0 &&
                    nestedWayIds.Count == 0 &&
                    nestedRelIds.Count == 0)
                    return false;

                var request = new OsmRequest
                {
                    NodeIds = nestedNodeIds,
                    WayIds = nestedWayIds,
                    RelationIds = nestedRelIds
                };

                LogService.Log($"Step {step}");
                newResponse = OsmService.Load(path, request);
                JsonFileClient.Write(cacheStepPath, newResponse.ToData());
            }

            response.Nodes.AddRange(newResponse.Nodes);
            response.Ways.AddRange(newResponse.Ways);
            response.Relations.AddRange(newResponse.Relations);
            response.MissedNodeIds.AddRange(newResponse.MissedNodeIds);
            response.MissedWayIds.AddRange(newResponse.MissedWayIds);
            response.MissedRelationIds.AddRange(newResponse.MissedRelationIds);
            return true;
        }

        private static OsmObjectResponse BuildObjects(OsmResponseData data)
        {
            LogService.Log("Build geo objects");

            var allNodesDict =
                data.Nodes.Select(i => new OsmNode(i)).ToDictionary(i => i.Id, i => i);

            var allWaysDict =
                data.Ways.Select(i => new OsmWay(i, allNodesDict)).ToDictionary(i => i.Id, i => i);

            var allRelationsDict =
                data.Relations.Select(i => new OsmRelation(i)).ToDictionary(i => i.Id, i => i);

            foreach (var relationData in data.Relations)
            {
                var members = new List<OsmMember>();

                foreach (var memberData in relationData.Members)
                {
                    switch (memberData.Type)
                    {
                        case OsmGeoType.Node:
                            if (allNodesDict.ContainsKey(memberData.Id))
                                members.Add(
                                    new OsmMember(memberData, allNodesDict[memberData.Id]));
                            break;

                        case OsmGeoType.Way:
                            if (allWaysDict.ContainsKey(memberData.Id))
                                members.Add(
                                    new OsmMember(memberData, allWaysDict[memberData.Id]));
                            break;

                        case OsmGeoType.Relation:
                            if (allRelationsDict.ContainsKey(memberData.Id))
                                members.Add(
                                    new OsmMember(memberData, allRelationsDict[memberData.Id]));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(memberData.Type));
                    }
                }

                allRelationsDict[relationData.Id].Members = members;
            }

            var wayNodeIds =
                allWaysDict.Values.SelectMany(i => i.Nodes).Select(i => i.Id);

            var relNodeIds =
                allRelationsDict.Values.SelectMany(i => i.Nodes).Select(i => i.Id);

            var memberNodeIds =
                new HashSet<long>(wayNodeIds.Concat(relNodeIds));

            var memberWayIds =
                new HashSet<long>(allRelationsDict.Values.SelectMany(i => i.Ways).Select(i => i.Id));

            var memberRelationIds =
                new HashSet<long>(allRelationsDict.Values.SelectMany(i => i.Relations).Select(i => i.Id));

            var rootNodes =
                allNodesDict.Values.Where(i => !memberNodeIds.Contains(i.Id)).ToList();

            var rootWays =
                allWaysDict.Values.Where(i => !memberWayIds.Contains(i.Id)).ToList();

            var rootRelations =
                allRelationsDict.Values.Where(i => !memberRelationIds.Contains(i.Id)).ToList();

            var validRootNodes = rootNodes.Where(i => !i.IsBroken && i.HasTitle).ToList();
            var validRootWays = rootWays.Where(i => !i.IsBroken && i.HasTitle).ToList();
            var validRootRelations = rootRelations.Where(i => !i.IsBroken && i.HasTitle).ToList();
            var brokenRootWays = rootWays.Where(i => i.IsBroken).ToList();
            var brokenRootRelations = rootRelations.Where(i => i.IsBroken).ToList();

            return new OsmObjectResponse
            {
                Nodes = validRootNodes,
                Ways = validRootWays,
                Relations = validRootRelations,
                BrokenWays = brokenRootWays,
                BrokenRelations = brokenRootRelations,
                AllNodesDict = allNodesDict,
                AllWaysDict = allWaysDict,
                AllRelationsDict = allRelationsDict
            };
        }

        private static string FullCachePath(string cacheName) =>
            $"$osm-cache/{cacheName}.json";

        private static string StepCachePath(string cacheName, int step) =>
            $"$osm-cache/{cacheName} - step {step}.json";
    }
}
