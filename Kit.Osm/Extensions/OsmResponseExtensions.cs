using System.Linq;

namespace Kit.Osm
{
    internal static class OsmResponseExtensions
    {
        public static OsmResponseData ToData(this OsmResponse response) =>
            new OsmResponseData
            {
                Nodes = response.Nodes?.Values.Select(i => i.ToData()).ToList(),
                Ways = response.Ways?.Values.Select(i => i.ToData()).ToList(),
                Relations = response.Relations?.Values.Select(i => i.ToData()).ToList(),
                MissedNodesIds = response.MissedNodeIds,
                MissedWaysIds = response.MissedWayIds,
                MissedRelationIds = response.MissedRelationIds
            };
    }
}
