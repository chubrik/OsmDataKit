using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class OsmWay : OsmObject
    {
        public IReadOnlyList<long> NodeIds { get; }
        public IReadOnlyList<OsmNode> Nodes { get; }

        public override bool IsBroken() => NodeIds.Count != Nodes.Count;

        public override GeoCoords AverageCoords() => OsmHelper.AverageCoords(Nodes);

        internal OsmWay(WayData data, IDictionary<long, OsmNode> allNodes) : base(data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Debug.Assert(allNodes != null);

            if (allNodes == null)
                throw new ArgumentNullException(nameof(allNodes));

            Type = OsmGeoType.Way;
            NodeIds = data.NodeIds;
            Nodes = data.NodeIds.Where(allNodes.ContainsKey).Select(i => allNodes[i]).ToList();
        }
    }
}
