using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsmSharp;
using System.Linq;

namespace OsmDataKit.Tests
{
    [TestClass]
    public class Tests : TestsBase
    {
        private const string SrcPath = "../$data/antarctica.osm.pbf";

        [TestMethod]
        public void LoadByFilter()
        {
            TestInitialize(nameof(LoadByFilter));
            var response = OsmService.Load(SrcPath, i => i.Tags.ContainsKey("place"));
            Assert.IsTrue(response.Nodes.Count > 100);
            Assert.IsTrue(response.Ways.Count > 5000);
            Assert.IsTrue(response.Relations.Count > 1000);
            Assert.IsTrue(response.MissedNodeIds.Count == 0);
            Assert.IsTrue(response.MissedWayIds.Count == 0);
            Assert.IsTrue(response.MissedRelationIds.Count == 0);
        }

        [TestMethod]
        public void LoadRelationObject()
        {
            TestInitialize(nameof(LoadRelationObject));

            // https://www.openstreetmap.org/relation/2969204
            var title = "Vega Island";
            long relationId = 2969204;

            var request = new OsmRequest { RelationIds = new[] { relationId } };
            var response = OsmService.LoadObjects(SrcPath, cacheName: title, request);

            Assert.IsTrue(response.RootNodes.Count == 0);
            Assert.IsTrue(response.RootWays.Count == 0);
            Assert.IsTrue(response.RootRelations.Count == 1);
            Assert.IsTrue(response.MissedNodeIds.Count == 0);
            Assert.IsTrue(response.MissedWayIds.Count == 0);
            Assert.IsTrue(response.MissedRelationIds.Count == 0);

            var relation = response.RootRelations.Single();

            Assert.IsTrue(relation.Type == OsmGeoType.Relation);
            Assert.IsTrue(relation.Id == relationId);
            Assert.IsTrue(relation.Tags["type"] == "multipolygon");
            Assert.IsTrue(relation.Tags["place"] == "island");
            Assert.IsTrue(relation.Members.Count > 100);
            Assert.IsTrue(relation.IsCompleted());
        }
    }
}
