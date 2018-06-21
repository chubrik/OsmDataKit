using OsmSharp;
using OsmSharp.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class OsmResponse
    {
        public Dictionary<long, Node> Nodes { get; internal set; }
        public Dictionary<long, Way> Ways { get; internal set; }
        public Dictionary<long, Relation> Relations { get; internal set; }
        public List<long> MissedNodeIds { get; internal set; }
        public List<long> MissedWayIds { get; internal set; }
        public List<long> MissedRelationIds { get; internal set; }

        public OsmResponse() { }

        internal OsmResponse(OsmResponseData data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Nodes = data.Nodes.Select(i => new Node
            {
                Id = i.Id,
                Tags = new TagsCollection(i.Tags),
                Latitude = i.Coords[0],
                Longitude = i.Coords[1]
            }).ToDictionary(i => i.Id.Value);

            Ways = data.Ways.Select(i => new Way
            {
                Id = i.Id,
                Tags = new TagsCollection(i.Tags),
                Nodes = i.NodeIds.ToArray()
            }).ToDictionary(i => i.Id.Value);

            Relations = data.Relations.Select(i => new Relation
            {
                Id = i.Id,
                Tags = new TagsCollection(i.Tags),
                Members = GetRelationMembers(i)
            }).ToDictionary(i => i.Id.Value);

            MissedNodeIds = data.MissedNodeIds;
            MissedWayIds = data.MissedWayIds;
            MissedRelationIds = data.MissedRelationIds;
        }

        private static RelationMember[] GetRelationMembers(RelationData data) =>
            data.Members.Select(i =>
                new RelationMember(i.Id, i.Role, i.Type)).ToArray();
    }
}
