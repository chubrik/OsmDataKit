using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class CompleteGeoObjectsExtensions
    {
        public static IEnumerable<NodeObject> AllNodes(this CompleteGeoObjects completeGeos) =>
            completeGeos.RootNodes
                  .Concat(completeGeos.AllWays().SelectMany(i => i.Nodes))
                  .Concat(completeGeos.AllRelations().SelectMany(i => i.AllChildNodes()))
                  .Distinct();

        public static IEnumerable<WayObject> AllWays(this CompleteGeoObjects completeGeos) =>
            completeGeos.RootWays
                  .Concat(completeGeos.AllRelations().SelectMany(i => i.AllChildWays()))
                  .Distinct();

        public static IEnumerable<RelationObject> AllRelations(this CompleteGeoObjects completeGeos) =>
            completeGeos.RootRelations
                  .Concat(completeGeos.RootRelations.SelectMany(i => i.AllChildRelations()))
                  .Distinct();
    }
}
