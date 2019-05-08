using OsmSharp;
using OsmSharp.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit.Data
{
    internal static class DataConverter
    {
        #region OSM to data

        public static OsmResponseData ToData(OsmResponse response) =>
            new OsmResponseData
            {
                Nodes = response.Nodes.Values.Select(ToData).ToList(),
                Ways = response.Ways.Values.Select(ToData).ToList(),
                Relations = response.Relations.Values.Select(ToData).ToList(),
                MissedNodeIds = response.MissedNodeIds,
                MissedWayIds = response.MissedWayIds,
                MissedRelationIds = response.MissedRelationIds
            };

        private static NodeData ToData(Node node)
        {
            var isValid = node.Id.HasValue && node.Latitude.HasValue && node.Longitude.HasValue;
            Debug.Assert(isValid);

            if (!isValid)
                throw new ArgumentException(nameof(node));

            return new NodeData
            {
                Id = node.Id.Value,
                Tags = node.Tags.Count > 0 ? node.Tags.ToDictionary(i => i.Key, i => i.Value) : null,
                Coords = new[] { node.Latitude.Value, node.Longitude.Value }
            };
        }

        private static WayData ToData(Way way)
        {
            Debug.Assert(way.Id.HasValue);

            if (!way.Id.HasValue)
                throw new ArgumentException(nameof(way));

            return new WayData
            {
                Id = way.Id.Value,
                Tags = way.Tags.Count > 0 ? way.Tags.ToDictionary(i => i.Key, i => i.Value) : null,
                NodeIds = way.Nodes.ToList()
            };
        }

        private static RelationData ToData(Relation relation)
        {
            Debug.Assert(relation.Id.HasValue);

            if (!relation.Id.HasValue)
                throw new ArgumentException(nameof(relation));

            return new RelationData
            {
                Id = relation.Id.Value,
                Tags = relation.Tags.Count > 0 ? relation.Tags.ToDictionary(i => i.Key, i => i.Value) : null,
                Members = relation.Members.Select(ToData).ToList()
            };
        }

        private static RelationMemberData ToData(RelationMember member) =>
            new RelationMemberData
            {
                Id = member.Id,
                Type = member.Type,
                Role = member.Role
            };

        #endregion

        #region Data to OSM

        public static OsmResponse ToOsm(OsmResponseData data) =>
            new OsmResponse
            {
                Nodes = data.Nodes.Select(ToOsm).ToDictionary(i => i.Id.Value),
                Ways = data.Ways.Select(ToOsm).ToDictionary(i => i.Id.Value),
                Relations = data.Relations.Select(ToOsm).ToDictionary(i => i.Id.Value),
                MissedNodeIds = data.MissedNodeIds,
                MissedWayIds = data.MissedWayIds,
                MissedRelationIds = data.MissedRelationIds
            };

        private static Node ToOsm(NodeData data) =>
            new Node
            {
                Id = data.Id,
                Tags = new TagsCollection(data.Tags),
                Latitude = data.Coords[0],
                Longitude = data.Coords[1]
            };

        private static Way ToOsm(WayData data) =>
            new Way
            {
                Id = data.Id,
                Tags = new TagsCollection(data.Tags),
                Nodes = data.NodeIds.ToArray()
            };

        private static Relation ToOsm(RelationData data) =>
            new Relation
            {
                Id = data.Id,
                Tags = new TagsCollection(data.Tags),
                Members = data.Members.Select(ToOsm).ToArray()
            };

        private static RelationMember ToOsm(RelationMemberData data) =>
            new RelationMember
            {
                Id = data.Id,
                Role = data.Role,
                Type = data.Type
            };

        #endregion

        #region Data to objects

        public static NodeObject ToObject(NodeData data)
        {
            Debug.Assert(data != null);
            Debug.Assert(data?.Coords?.Length == 2);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Coords?.Length != 2)
                throw new ArgumentException(nameof(data));

            return new NodeObject(
                id: data.Id,
                latitude: data.Coords[0],
                longitude: data.Coords[1],
                tags: data.Tags);
        }

        public static WayObject ToObject(WayData data, IDictionary<long, NodeObject> allNodes)
        {
            Debug.Assert(data != null);
            Debug.Assert(allNodes != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (allNodes == null)
                throw new ArgumentNullException(nameof(allNodes));

            return new WayObject(
                id: data.Id,
                nodes: data.NodeIds.Where(allNodes.ContainsKey).Select(i => allNodes[i]).ToList(),
                tags: data.Tags);
        }

        public static RelationObject ToObject(RelationData data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new RelationObject(
                id: data.Id,
                tags: data.Tags);
        }

        public static RelationMemberObject ToObject(RelationMemberData data, GeoObject geo)
        {
            Debug.Assert(data != null);
            Debug.Assert(geo != null);
            Debug.Assert(geo?.Type == data.Type);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (geo == null)
                throw new ArgumentNullException(nameof(geo));

            if (geo.Type != data.Type)
                throw new ArgumentException(nameof(geo));

            return new RelationMemberObject(
                role: data.Role,
                geo: geo);
        }

        #endregion
    }
}
