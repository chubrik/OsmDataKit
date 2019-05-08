using OsmSharp;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class RelationObjectExtensions
    {
        public static IEnumerable<NodeObject> Nodes(this RelationObject relation) =>
            relation.Members.Where(i => i.Geo.Type == OsmGeoType.Node)
                            .Select(i => (NodeObject)i.Geo);

        public static IEnumerable<WayObject> Ways(this RelationObject relation) =>
            relation.Members.Where(i => i.Geo.Type == OsmGeoType.Way)
                            .Select(i => (WayObject)i.Geo);

        public static IEnumerable<RelationObject> Relations(this RelationObject relation) =>
            relation.Members.Where(i => i.Geo.Type == OsmGeoType.Relation)
                            .Select(i => (RelationObject)i.Geo);

        public static IEnumerable<NodeObject> GetAllNodes(this RelationObject relation) =>
            relation.Nodes().Concat(relation.Ways().SelectMany(i => i.Nodes))
                            .Concat(relation.Relations().SelectMany(i => i.GetAllNodes()));
    }
}
