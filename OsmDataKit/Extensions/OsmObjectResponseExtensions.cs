using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class OsmObjectResponseExtensions
    {
        public static IEnumerable<GeoObject> RootObjects(this OsmObjectResponse response) =>
            (response.RootNodes.Values as IEnumerable<GeoObject>)
                .Concat(response.RootWays.Values)
                .Concat(response.RootRelations.Values);

        public static IEnumerable<NodeObject> AllNodes(this OsmObjectResponse response) =>
            response.RootNodes.Values
                .Concat(response.AllWays().SelectMany(i => i.Nodes))
                .Concat(response.AllRelations().SelectMany(i => i.AllNodes()));

        public static IEnumerable<WayObject> AllWays(this OsmObjectResponse response) =>
            response.RootWays.Values
                .Concat(response.AllRelations().SelectMany(i => i.AllWays()))
                .Distinct();

        public static IEnumerable<RelationObject> AllRelations(this OsmObjectResponse response) =>
            response.RootRelations.Values
                .Concat(response.RootRelations.Values.SelectMany(i => i.AllRelations()))
                .Distinct();

        public static IEnumerable<GeoObject> AllObjects(this OsmObjectResponse response) =>
            (response.AllNodes() as IEnumerable<GeoObject>)
                .Concat(response.AllWays())
                .Concat(response.AllRelations());
    }
}
