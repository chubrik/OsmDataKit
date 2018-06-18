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
        public Dictionary<long, Node> Nodes { get; set; }
        public Dictionary<long, Way> Ways { get; set; }
        public Dictionary<long, Relation> Relations { get; set; }
        public List<long> MissedNodeIds { get; set; }
        public List<long> MissedWayIds { get; set; }
        public List<long> MissedRelationIds { get; set; }

        public OsmResponse() { }

        internal OsmResponse(OsmDataSet dataSet)
        {
            Debug.Assert(dataSet != null);

            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            Nodes = dataSet.Nodes.Select(i => new Node
            {
                Id = i.Id,
                Tags = new TagsCollection(i.Tags),
                Latitude = i.Coords[0],
                Longitude = i.Coords[1]
            }).ToDictionary(i => i.Id.Value);

            Ways = dataSet.Ways.Select(i => new Way
            {
                Id = i.Id,
                Tags = new TagsCollection(i.Tags),
                Nodes = i.NodeIds.ToArray()
            }).ToDictionary(i => i.Id.Value);

            Relations = dataSet.Relations.Select(i => new Relation
            {
                Id = i.Id,
                Tags = new TagsCollection(i.Tags),
                Members = GetRelationMembers(i)
            }).ToDictionary(i => i.Id.Value);

            MissedNodeIds = dataSet.MissedNodesIds;
            MissedWayIds = dataSet.MissedWaysIds;
            MissedRelationIds = dataSet.MissedRelationIds;
        }

        private static RelationMember[] GetRelationMembers(OsmRelationData data) =>
            data.Members.Select(i =>
                new RelationMember(i.Id, i.Role, i.Type.ToOsmType())).ToArray();
    }
}
