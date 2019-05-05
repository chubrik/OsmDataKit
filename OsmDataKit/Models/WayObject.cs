using OsmDataKit.Internal;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public class WayObject : GeoObject
    {
        public IReadOnlyList<long> NodeIds { get; private set; }
        public IReadOnlyList<NodeObject> Nodes { get; private set; }

        public override OsmGeoType Type => OsmGeoType.Way;
        public override bool IsBroken => NodeIds.Count != Nodes.Count;

        private GeoCoords _averageCoords;

        public override IGeoCoords AverageCoords =>
            _averageCoords ?? (_averageCoords = Nodes.AverageCoords());

        public void SetNodes(IReadOnlyList<NodeObject> nodes)
        {
            Debug.Assert(nodes != null);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            NodeIds = nodes.Select(i => i.Id).ToList();
            Nodes = nodes;
            _averageCoords = null;
        }

        internal WayObject(WayData data, IDictionary<long, NodeObject> allNodes) : base(data)
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

        public WayObject(
            long id,
            IReadOnlyDictionary<string, string> tags,
            IReadOnlyList<NodeObject> nodes,
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
