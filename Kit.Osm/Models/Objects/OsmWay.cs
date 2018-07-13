using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class OsmWay : OsmObject
    {
        public IReadOnlyList<long> NodeIds { get; private set; }
        public IReadOnlyList<OsmNode> Nodes { get; private set; }

        public override OsmGeoType Type => OsmGeoType.Way;
        public override bool IsBroken => NodeIds.Count != Nodes.Count;

        private GeoCoords _averageCoords;

        public override IGeoCoords AverageCoords =>
            _averageCoords ?? (_averageCoords = Nodes.AverageCoords());

        public void SetNodes(IReadOnlyList<OsmNode> nodes)
        {
            Debug.Assert(nodes != null);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            NodeIds = nodes.Select(i => i.Id).ToList();
            Nodes = nodes;
            _averageCoords = null;
        }

        internal OsmWay(WayData data, IDictionary<long, OsmNode> allNodes) : base(data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Debug.Assert(allNodes != null);

            if (allNodes == null)
                throw new ArgumentNullException(nameof(allNodes));

            NodeIds = data.NodeIds;
            Nodes = data.NodeIds.Where(allNodes.ContainsKey).Select(i => allNodes[i]).ToList();
        }

        public OsmWay(
            long id,
            IReadOnlyDictionary<string, string> tags,
            IReadOnlyList<OsmNode> nodes,
            IReadOnlyDictionary<string, string> data = null)
            : base(id, tags, data)
        {
            Debug.Assert(nodes != null);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            NodeIds = nodes.Select(i => i.Id).ToList();
            Nodes = nodes;
        }
    }
}
