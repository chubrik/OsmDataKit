using System.Linq;

namespace Kit.Osm
{
    internal static class OsmResponseExtensions
    {
        public static OsmResponseData ToData(this OsmResponse response, bool preventMissed = false)
        {
            var data = new OsmResponseData
            {
                Nodes = response.Nodes?.Values.Select(i => i.ToData()).ToList(),
                Ways = response.Ways?.Values.Select(i => i.ToData()).ToList(),
                Relations = response.Relations?.Values.Select(i => i.ToData()).ToList()
            };

            //todo why?
            if (!preventMissed)
            {
                data.MissedNodesIds = response.MissedNodeIds;
                data.MissedWaysIds = response.MissedWayIds;
                data.MissedRelationIds = response.MissedRelationIds;
            }

            return data;
        }
    }
}
