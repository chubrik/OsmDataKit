using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsmSharp;
using System.Linq;

namespace OsmDataKit.Tests
{
    [TestClass]
    public class Tests : TestsBase
    {
        [TestMethod]
        public void LoadByFilter()
        {
            TestInitialize(nameof(LoadByFilter));
            var geos = OsmService.LoadObjects(PbfPath, "Places", i => i.Tags.ContainsKey("place"));
            Assert.IsTrue(geos.Nodes.Count > 100);
            Assert.IsTrue(geos.Ways.Count > 5000);
            Assert.IsTrue(geos.Relations.Count > 1000);
            Assert.IsTrue(geos.MissedNodeIds.Count == 0);
            Assert.IsTrue(geos.MissedWayIds.Count == 0);
            Assert.IsTrue(geos.MissedRelationIds.Count == 0);
        }

        [TestMethod]
        public void LoadRelationObject()
        {
            TestInitialize(nameof(LoadRelationObject));

            // https://www.openstreetmap.org/relation/2969204
            var title = "Vega Island";
            long relationId = 2969204;

            var request = new GeoRequest { RelationIds = new[] { relationId } };
            var completeGeos = OsmService.LoadCompleteObjects(PbfPath, cacheName: title, request);

            Assert.IsTrue(completeGeos.RootNodes.Count == 0);
            Assert.IsTrue(completeGeos.RootWays.Count == 0);
            Assert.IsTrue(completeGeos.RootRelations.Count == 1);
            Assert.IsTrue(completeGeos.MissedNodeIds.Count == 0);
            Assert.IsTrue(completeGeos.MissedWayIds.Count == 0);
            Assert.IsTrue(completeGeos.MissedRelationIds.Count == 0);

            var relation = completeGeos.RootRelations.Single();

            Assert.IsTrue(relation.Type == OsmGeoType.Relation);
            Assert.IsTrue(relation.Id == relationId);
            Assert.IsTrue(relation.Tags["type"] == "multipolygon");
            Assert.IsTrue(relation.Tags["place"] == "island");
            Assert.IsTrue(relation.Members.Count > 100);
            Assert.IsTrue(relation.IsComplete());
        }
    }
}
