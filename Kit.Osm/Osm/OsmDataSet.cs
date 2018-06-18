using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    [JsonObject]
    internal class OsmDataSet
    {
        [JsonProperty]
        public List<OsmNodeData> Nodes { get; private set; }

        [JsonProperty]
        public List<OsmWayData> Ways { get; private set; }

        [JsonProperty]
        public List<OsmRelationData> Relations { get; private set; }

        [JsonProperty]
        public List<long> MissedNodesIds { get; private set; }

        [JsonProperty]
        public List<long> MissedWaysIds { get; private set; }

        [JsonProperty]
        public List<long> MissedRelationIds { get; private set; }

        public OsmDataSet() { }

        public OsmDataSet(OsmResponse response, bool preventMissed = false)
        {
            Debug.Assert(response != null);

            if (response == null)
                throw new ArgumentNullException(nameof(response));

            Nodes = response.Nodes?.Values.Select(i => new OsmNodeData(i)).ToList();
            Ways = response.Ways?.Values.Select(i => new OsmWayData(i)).ToList();
            Relations = response.Relations?.Values.Select(i => new OsmRelationData(i)).ToList();

            if (!preventMissed)
            {
                MissedNodesIds = response.MissedNodeIds;
                MissedWaysIds = response.MissedWayIds;
                MissedRelationIds = response.MissedRelationIds;
            }
        }
    }
}
