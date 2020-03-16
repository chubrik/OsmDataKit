using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class OsmObjectResponseExtensions
    {
        public static IEnumerable<NodeObject> DeepNodes(this OsmObjectResponse response) =>
            response.RootNodes
                .Concat(response.DeepWays().SelectMany(i => i.Nodes))
                .Concat(response.DeepRelations().SelectMany(i => i.DeepNodes()))
                .Distinct();

        public static IEnumerable<WayObject> DeepWays(this OsmObjectResponse response) =>
            response.RootWays
                .Concat(response.DeepRelations().SelectMany(i => i.DeepWays()))
                .Distinct();

        public static IEnumerable<RelationObject> DeepRelations(this OsmObjectResponse response) =>
            response.RootRelations
                .Concat(response.RootRelations.SelectMany(i => i.DeepRelations()))
                .Distinct();
    }
}
