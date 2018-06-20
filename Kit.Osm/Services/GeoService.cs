﻿using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public static class GeoService
    {
        public static GeoGroup LoadGroup(
            string path, string cacheName, long osmRelationId, int stepLimit = 0)
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
                var dataSet = JsonFileService.Read<OsmDataSet>(cachePath);
                return BuildObjects(dataSet).AllGroupsDict[osmRelationId];
            }

            if (!FileClient.Exists(path))
                throw new InvalidOperationException();

            LogService.Log("Load OSM dataset");
            var cacheStepPath = StepCachePath(cacheName, 1);
            OsmResponse response;

            if (FileClient.Exists(cacheStepPath))
            {
                var dataSet = JsonFileService.Read<OsmDataSet>(cacheStepPath);
                response = new OsmResponse(dataSet);
            }
            else
            {
                var request = new OsmRequest
                {
                    RelationIds = new List<long> { osmRelationId }
                };

                LogService.Log($"Step 1");
                response = OsmService.Load(path, request);
                JsonFileService.Write(cacheStepPath, new OsmDataSet(response));
            }

            return LoadSteps(path, cacheName, response, stepLimit).AllGroupsDict[osmRelationId];
        }

        public static GeoSet Load(
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
                var dataSet = JsonFileService.Read<OsmDataSet>(cachePath);
                return BuildObjects(dataSet);
            }

            if (!FileClient.Exists(path))
                throw new InvalidOperationException();

            LogService.Log("Load OSM dataset");
            var cacheStepPath = StepCachePath(cacheName, 1);
            OsmResponse response;

            if (FileClient.Exists(cacheStepPath))
            {
                var dataSet = JsonFileService.Read<OsmDataSet>(cacheStepPath);
                response = new OsmResponse(dataSet);
            }
            else
            {
                LogService.Log($"Step 1");
                response = OsmService.Load(path, predicate);
                JsonFileService.Write(cacheStepPath, new OsmDataSet(response));
            }

            return LoadSteps(path, cacheName, response, stepLimit);
        }

        private static GeoSet LoadSteps(
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

            var dataSet = new OsmDataSet(response, preventMissed: true);
            JsonFileService.Write(FullCachePath(cacheName), dataSet);
            var geoSet = BuildObjects(dataSet);
            LogService.Log("Load OSM dataset complete");
            return geoSet;
        }

        private static bool FillNested(
            string path, string cacheName, OsmResponse response, int step)
        {
            OsmResponse newResponse;
            var cacheStepPath = StepCachePath(cacheName, step);

            if (FileClient.Exists(cacheStepPath))
            {
                var dataSet = JsonFileService.Read<OsmDataSet>(cacheStepPath);
                newResponse = new OsmResponse(dataSet);
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
                JsonFileService.Write(cacheStepPath, new OsmDataSet(newResponse));
            }

            response.Nodes.AddRange(newResponse.Nodes);
            response.Ways.AddRange(newResponse.Ways);
            response.Relations.AddRange(newResponse.Relations);
            response.MissedNodeIds.AddRange(newResponse.MissedNodeIds);
            response.MissedWayIds.AddRange(newResponse.MissedWayIds);
            response.MissedRelationIds.AddRange(newResponse.MissedRelationIds);
            return true;
        }

        private static GeoSet BuildObjects(OsmDataSet dataSet)
        {
            LogService.Log("Build geo objects");

            var allPointsDict =
                dataSet.Nodes.Select(i => new GeoPoint(i)).ToDictionary(i => i.Id, i => i);

            var allLinesDict =
                dataSet.Ways.Select(i => new GeoLine(i, allPointsDict)).ToDictionary(i => i.Id, i => i);

            var allGroupsDict =
                dataSet.Relations.Select(i => new GeoGroup(i)).ToDictionary(i => i.Id, i => i);

            foreach (var relationData in dataSet.Relations)
            {
                var members = new List<GeoMember>();

                foreach (var memberData in relationData.Members)
                {
                    switch (memberData.Type)
                    {
                        case GeoType.Point:
                            if (allPointsDict.ContainsKey(memberData.Id))
                                members.Add(
                                    new GeoMember(memberData, allPointsDict[memberData.Id]));
                            break;

                        case GeoType.Line:
                            if (allLinesDict.ContainsKey(memberData.Id))
                                members.Add(
                                    new GeoMember(memberData, allLinesDict[memberData.Id]));
                            break;

                        case GeoType.Group:
                            if (allGroupsDict.ContainsKey(memberData.Id))
                                members.Add(
                                    new GeoMember(memberData, allGroupsDict[memberData.Id]));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(memberData.Type));
                    }
                }

                allGroupsDict[relationData.Id].SetMembers(members);
            }

            var linePointIds =
                allLinesDict.Values.SelectMany(i => i.Points).Select(i => i.Id);

            var groupPointIds =
                allGroupsDict.Values.SelectMany(i => i.Points()).Select(i => i.Id);

            var memberPointIds =
                new HashSet<long>(linePointIds.Concat(groupPointIds));

            var memberLineIds =
                new HashSet<long>(allGroupsDict.Values.SelectMany(i => i.Lines()).Select(i => i.Id));

            var memberGroupIds =
                new HashSet<long>(allGroupsDict.Values.SelectMany(i => i.Groups()).Select(i => i.Id));

            var rootPoints =
                allPointsDict.Values.Where(i => !memberPointIds.Contains(i.Id)).ToList();

            var rootLines =
                allLinesDict.Values.Where(i => !memberLineIds.Contains(i.Id)).ToList();

            var rootGroups =
                allGroupsDict.Values.Where(i => !memberGroupIds.Contains(i.Id)).ToList();

            var validRootPoints = rootPoints.Where(i => !i.IsBroken() && !i.Title.IsNullOrWhiteSpace()).ToList();
            var validRootLines = rootLines.Where(i => !i.IsBroken() && !i.Title.IsNullOrWhiteSpace()).ToList();
            var validRootGroups = rootGroups.Where(i => !i.IsBroken() && !i.Title.IsNullOrWhiteSpace()).ToList();
            var brokenRootLines = rootLines.Where(i => i.IsBroken()).ToList();
            var brokenRootGroups = rootGroups.Where(i => i.IsBroken()).ToList();

            return new GeoSet
            {
                Points = validRootPoints,
                Lines = validRootLines,
                Groups = validRootGroups,
                BrokenLines = brokenRootLines,
                BrokenGroups = brokenRootGroups,
                AllPointsDict = allPointsDict,
                AllLinesDict = allLinesDict,
                AllGroupsDict = allGroupsDict
            };
        }

        private static string FullCachePath(string cacheName) =>
            $"$osm-cache/{cacheName}.json";

        private static string StepCachePath(string cacheName, int step) =>
            $"$osm-cache/{cacheName}/{cacheName} - step {step}.json";
    }
}
