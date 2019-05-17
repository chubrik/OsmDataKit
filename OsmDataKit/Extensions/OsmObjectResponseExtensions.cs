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

        public static IEnumerable<NodeObject> DeepNodes(this OsmObjectResponse response) =>
            response.RootNodes.Values
                .Concat(response.DeepWays().SelectMany(i => i.Nodes))
                .Concat(response.DeepRelations().SelectMany(i => i.DeepNodes()))
                .Distinct();

        public static IEnumerable<WayObject> DeepWays(this OsmObjectResponse response) =>
            response.RootWays.Values
                .Concat(response.DeepRelations().SelectMany(i => i.DeepWays()))
                .Distinct();

        public static IEnumerable<RelationObject> DeepRelations(this OsmObjectResponse response) =>
            response.RootRelations.Values
                .Concat(response.RootRelations.Values.SelectMany(i => i.DeepRelations()))
                .Distinct();

        public static IEnumerable<GeoObject> DeepObjects(this OsmObjectResponse response) =>
            (response.DeepNodes() as IEnumerable<GeoObject>)
                .Concat(response.DeepWays())
                .Concat(response.DeepRelations());
    }
}
